using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

using XtremeIdiots.InvisionCommunity;
using XtremeIdiots.InvisionCommunity.Models;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.UserProfiles;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Auth.XtremeIdiots
{
    /// <summary>
    /// Provides authentication services for XtremeIdiots external login integration.
    /// Handles user registration, login, and profile synchronization with the forums system.
    /// </summary>
    public class XtremeIdiotsAuth : IXtremeIdiotsAuth
    {
        private const string XtremeIdiotsProvider = "XtremeIdiots";
        private const string UnknownUsername = "Unknown";
        private const string UnknownEmail = "unknown@example.com";

        private readonly IInvisionApiClient forumsClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly ILogger<XtremeIdiotsAuth> logger;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;

        public XtremeIdiotsAuth(
            ILogger<XtremeIdiotsAuth> logger,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IInvisionApiClient forumsClient,
            IRepositoryApiClient repositoryApiClient)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.forumsClient = forumsClient ?? throw new ArgumentNullException(nameof(forumsClient));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string? redirectUrl)
        {
            return signInManager.ConfigureExternalAuthenticationProperties(XtremeIdiotsProvider, redirectUrl);
        }

        public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(CancellationToken cancellationToken = default)
        {
            return await signInManager.GetExternalLoginInfoAsync().ConfigureAwait(false);
        }

        public async Task<XtremeIdiotsAuthResult> ProcessExternalLogin(ExternalLoginInfo info, CancellationToken cancellationToken = default)
        {
            ValidateExternalLoginInfo(info);

            try
            {
                var result = await AttemptExternalLoginSignIn(info, cancellationToken).ConfigureAwait(false);

                if (result != XtremeIdiotsAuthResult.Failed)
                {
                    return result;
                }

                // If sign in failed, try to register new user
                await RegisterNewUser(info, cancellationToken).ConfigureAwait(false);
                return XtremeIdiotsAuthResult.Success;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing external login for provider {LoginProvider}", info.LoginProvider);
                return XtremeIdiotsAuthResult.Failed;
            }
        }

        public async Task SignOutAsync(CancellationToken cancellationToken = default)
        {
            await signInManager.SignOutAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an existing user's profile and claims based on the latest forum data.
        /// </summary>
        /// <param name="info">The external login information containing user details.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous update operation.</returns>
        private async Task UpdateExistingUser(ExternalLoginInfo info, CancellationToken cancellationToken = default)
        {
            if (info?.Principal is null)
            {
                throw new ArgumentException("External login info or principal is null", nameof(info));
            }

            var id = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("No NameIdentifier claim found in external login info", nameof(info));
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var member = await forumsClient.Core.GetMember(id).ConfigureAwait(false);
                if (member is null)
                {
                    throw new InvalidOperationException($"Member not found with ID: {id}");
                }

                var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey).ConfigureAwait(false);
                if (user is null)
                {
                    throw new InvalidOperationException($"User not found for login provider: {info.LoginProvider}, key: {info.ProviderKey}");
                }

                cancellationToken.ThrowIfCancellationRequested();

                var userClaims = await userManager.GetClaimsAsync(user).ConfigureAwait(false);
                await userManager.RemoveClaimsAsync(user, userClaims).ConfigureAwait(false);

                var userProfile = await EnsureUserProfileExists(member.Id.ToString(), member, cancellationToken).ConfigureAwait(false);

                if (userProfile?.UserProfileClaims != null)
                {
                    var claims = userProfile.UserProfileClaims.Select(upc => new Claim(upc.ClaimType, upc.ClaimValue)).ToList();
                    await userManager.AddClaimsAsync(user, claims).ConfigureAwait(false);
                }

                await signInManager.SignInAsync(user, true).ConfigureAwait(false);
                await signInManager.RefreshSignInAsync(user).ConfigureAwait(false);

                logger.LogInformation("Successfully updated user profile for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating existing user with ID: {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Registers a new user in the system and synchronizes their profile with forum data.
        /// </summary>
        /// <param name="info">The external login information containing user details.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous registration operation.</returns>
        private async Task RegisterNewUser(ExternalLoginInfo info, CancellationToken cancellationToken = default)
        {
            if (info?.Principal is null)
            {
                throw new ArgumentException("External login info or principal is null", nameof(info));
            }

            var id = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = info.Principal.FindFirstValue(ClaimTypes.Name);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("No NameIdentifier claim found in external login info", nameof(info));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException($"No username claim found in external login info for ID: {id}", nameof(info));
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var member = await forumsClient.Core.GetMember(id).ConfigureAwait(false);
                if (member is null)
                {
                    throw new InvalidOperationException($"Member not found with ID: {id}");
                }

                var user = new IdentityUser { Id = id, UserName = username, Email = email };
                var createUserResult = await userManager.CreateAsync(user).ConfigureAwait(false);

                if (!createUserResult.Succeeded)
                {
                    logger.LogWarning("Failed to create user {Username}. Errors: {Errors}",
                        username, string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                    return;
                }

                var addLoginResult = await userManager.AddLoginAsync(user, info).ConfigureAwait(false);
                if (!addLoginResult.Succeeded)
                {
                    logger.LogWarning("Failed to add external login for user {Username}. Errors: {Errors}",
                        username, string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();

                var userProfile = await EnsureUserProfileExists(member.Id.ToString(), member, cancellationToken).ConfigureAwait(false);

                if (userProfile?.UserProfileClaims != null)
                {
                    var claims = userProfile.UserProfileClaims.Select(upc => new Claim(upc.ClaimType, upc.ClaimValue)).ToList();
                    await userManager.AddClaimsAsync(user, claims).ConfigureAwait(false);
                }

                await signInManager.SignInAsync(user, true).ConfigureAwait(false);
                await signInManager.RefreshSignInAsync(user).ConfigureAwait(false);

                logger.LogInformation("Successfully created new user {Username} with email: {Email}", username, email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error registering new user with username: {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// Ensures a user profile exists in the repository, creating one if necessary.
        /// </summary>
        /// <param name="memberId">The member ID from the forums system.</param>
        /// <param name="member">The member data from the forums API.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{UserProfileDto}"/> representing the user profile.</returns>
        private async Task<UserProfileDto?> EnsureUserProfileExists(string memberId, Member member, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(memberId))
            {
                throw new ArgumentException("Member ID cannot be null or empty.", nameof(memberId));
            }

            if (member is null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var userProfileDtoApiResponse = await repositoryApiClient.UserProfiles.V1
                    .GetUserProfileByXtremeIdiotsId(memberId, cancellationToken).ConfigureAwait(false);

                if (userProfileDtoApiResponse.IsNotFound)
                {
                    logger.LogInformation("Creating new user profile for member {MemberId}", memberId);

                    var createResult = await repositoryApiClient.UserProfiles.V1.CreateUserProfile(new CreateUserProfileDto(
                        memberId,
                        member.Name ?? UnknownUsername,
                        member.Email ?? UnknownEmail)
                    {
                        Title = member.Title,
                        FormattedName = member.FormattedName,
                        PrimaryGroup = member.PrimaryGroup?.Name,
                        PhotoUrl = member.PhotoUrl,
                        ProfileUrl = member.ProfileUrl?.ToString(),
                        TimeZone = member.TimeZone
                    }, cancellationToken).ConfigureAwait(false);

                    if (!createResult.IsSuccess)
                    {
                        logger.LogWarning("Failed to create user profile for member {MemberId}", memberId);
                        return null;
                    }

                    // Retrieve the newly created profile
                    userProfileDtoApiResponse = await repositoryApiClient.UserProfiles.V1
                        .GetUserProfileByXtremeIdiotsId(memberId, cancellationToken).ConfigureAwait(false);
                }

                if (userProfileDtoApiResponse.IsSuccess)
                {
                    logger.LogDebug("Successfully retrieved user profile for member {MemberId}", memberId);
                    return userProfileDtoApiResponse.Result?.Data;
                }

                logger.LogWarning("Failed to retrieve user profile for member {MemberId}", memberId);
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error ensuring user profile exists for member {MemberId}", memberId);
                throw;
            }
        }

        /// <summary>
        /// Validates the external login information for required properties.
        /// </summary>
        /// <param name="info">The external login information to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when info is null.</exception>
        /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
        private static void ValidateExternalLoginInfo(ExternalLoginInfo info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (info.Principal is null)
            {
                throw new ArgumentException("External login info must contain a valid principal", nameof(info));
            }

            if (string.IsNullOrWhiteSpace(info.LoginProvider))
            {
                throw new ArgumentException("External login info must contain a valid login provider", nameof(info));
            }

            if (string.IsNullOrWhiteSpace(info.ProviderKey))
            {
                throw new ArgumentException("External login info must contain a valid provider key", nameof(info));
            }
        }

        /// <summary>
        /// Attempts to sign in using external login credentials.
        /// </summary>
        /// <param name="info">The external login information.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{XtremeIdiotsAuthResult}"/> representing the sign-in result.</returns>
        private async Task<XtremeIdiotsAuthResult> AttemptExternalLoginSignIn(ExternalLoginInfo info, CancellationToken cancellationToken)
        {
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true).ConfigureAwait(false);
            var username = info.Principal.FindFirstValue(ClaimTypes.Name);

            logger.LogDebug("User {Username} had a {SignInResult} sign in result", username, result.ToString());

            if (result.Succeeded)
            {
                await UpdateExistingUser(info, cancellationToken).ConfigureAwait(false);
                return XtremeIdiotsAuthResult.Success;
            }

            if (result.IsLockedOut)
            {
                logger.LogWarning("User {Username} account is locked", username);
                return XtremeIdiotsAuthResult.Locked;
            }

            if (result.IsNotAllowed || result.RequiresTwoFactor)
            {
                logger.LogWarning("User {Username} sign in not allowed or requires 2FA", username);
                return XtremeIdiotsAuthResult.Failed;
            }

            return XtremeIdiotsAuthResult.Failed;
        }
    }
}