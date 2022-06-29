using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.ForumsIntegration;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Extensions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.SyncFunc.Interfaces;

namespace XtremeIdiots.Portal.SyncFunc.Ingest
{
    internal class BanFileIngest : IBanFileIngest
    {
        private readonly ILogger<BanFileIngest> _logger;
        private readonly IGuidValidator _guidValidator;
        private readonly IAdminActionTopics adminActionTopics;
        private readonly IRepositoryApiClient repositoryApiClient;

        public BanFileIngest(
            ILogger<BanFileIngest> logger,
            IGuidValidator guidValidator,
            IAdminActionTopics adminActionTopics,
            IRepositoryApiClient repositoryApiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _guidValidator = guidValidator ?? throw new ArgumentNullException(nameof(guidValidator));
            this.adminActionTopics = adminActionTopics ?? throw new ArgumentNullException(nameof(adminActionTopics));
            this.repositoryApiClient = repositoryApiClient;

        }

        public async Task IngestBanFileDataForGame(string gameType, string remoteBanFileData)
        {
            var skipTags = new[] { "[PBBAN]", "[B3BAN]", "[BANSYNC]", "[EXTERNAL]" };
            var gameTypeEnum = Enum.Parse<GameType>(gameType);

            foreach (var line in remoteBanFileData.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line) || skipTags.Any(skipTag => line.Contains(skipTag))) continue;

                ParseLine(line, out var guid, out var name);

                if (string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(name)) continue;

                if (!_guidValidator.IsValid(gameType, guid))
                {
                    _logger.LogWarning($"Could not validate guid {guid} for {gameType}");
                    continue;
                }
                var playerDtoApiResponse = await repositoryApiClient.Players.GetPlayerByGameType(gameTypeEnum, guid, PlayerEntityOptions.None);

                if (playerDtoApiResponse.IsNotFound)
                {
                    _logger.LogInformation($"BanFileIngest - creating new player {name} with guid {guid} with import ban");

                    await repositoryApiClient.Players.CreatePlayer(new CreatePlayerDto(name, guid, gameType.ToGameType()));

                    playerDtoApiResponse = await repositoryApiClient.Players.GetPlayerByGameType(gameTypeEnum, guid, PlayerEntityOptions.None);

                    if (playerDtoApiResponse == null)
                        throw new Exception("Newly created player could not be retrieved from database");

                    var createAdminActionDto = new CreateAdminActionDto(playerDtoApiResponse.Result.PlayerId, AdminActionType.Ban, "Imported from server");
                    createAdminActionDto.ForumTopicId = await adminActionTopics.CreateTopicForAdminAction(createAdminActionDto.Type, playerDtoApiResponse.Result.GameType, playerDtoApiResponse.Result.PlayerId, playerDtoApiResponse.Result.Username, DateTime.UtcNow, createAdminActionDto.Text, createAdminActionDto.AdminId);

                    await repositoryApiClient.AdminActions.CreateAdminAction(createAdminActionDto);
                }
                else
                {
                    var adminActionsApiResponse = await repositoryApiClient.AdminActions.GetAdminActions(null, playerDtoApiResponse.Result.PlayerId, null, AdminActionFilter.ActiveBans, 0, 500, null);

                    if (adminActionsApiResponse.Result.Entries?.Count(aa => aa.Type == AdminActionType.Ban) == 0)
                    {
                        _logger.LogInformation($"BanFileImport - adding import ban to existing player {playerDtoApiResponse.Result.Username} - {playerDtoApiResponse.Result.Guid} ({playerDtoApiResponse.Result.GameType})");

                        var createAdminActionDto = new CreateAdminActionDto(playerDtoApiResponse.Result.PlayerId, AdminActionType.Ban, "Imported from server");
                        createAdminActionDto.ForumTopicId = await adminActionTopics.CreateTopicForAdminAction(createAdminActionDto.Type, playerDtoApiResponse.Result.GameType, playerDtoApiResponse.Result.PlayerId, playerDtoApiResponse.Result.Username, DateTime.UtcNow, createAdminActionDto.Text, createAdminActionDto.AdminId);

                        await repositoryApiClient.AdminActions.CreateAdminAction(createAdminActionDto);
                    }
                }

            }
        }

        private void ParseLine(string line, out string? guid, out string? name)
        {
            try
            {
                var trimmedLine = line.Trim();
                var indexOfSpace = trimmedLine.IndexOf(' ');
                var lengthOfLine = trimmedLine.Length;

                guid = trimmedLine.Substring(0, indexOfSpace).Trim().ToLower();
                name = trimmedLine.Substring(indexOfSpace, lengthOfLine - indexOfSpace).Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to parse {line} when ingesting ban file");
                guid = name = null;
            }
        }
    }
}