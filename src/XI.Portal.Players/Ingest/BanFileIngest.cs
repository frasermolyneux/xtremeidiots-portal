using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Players.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Extensions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Players.Ingest
{
    internal class BanFileIngest : IBanFileIngest
    {
        private readonly ILogger<BanFileIngest> _logger;
        private readonly IGuidValidator _guidValidator;
        private readonly IPlayersForumsClient _playersForumsClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;

        public BanFileIngest(
            ILogger<BanFileIngest> logger,
            IGuidValidator guidValidator,
            IPlayersForumsClient playersForumsClient,
            IRepositoryApiClient repositoryApiClient,
            IRepositoryTokenProvider repositoryTokenProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _guidValidator = guidValidator ?? throw new ArgumentNullException(nameof(guidValidator));
            _playersForumsClient = playersForumsClient ?? throw new ArgumentNullException(nameof(playersForumsClient));
            this.repositoryApiClient = repositoryApiClient;
            this.repositoryTokenProvider = repositoryTokenProvider;
        }

        public async Task IngestBanFileDataForGame(string gameType, string remoteBanFileData)
        {
            var skipTags = new[] { "[PBBAN]", "[B3BAN]", "[BANSYNC]", "[EXTERNAL]" };

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();

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
                var player = await repositoryApiClient.Players.GetPlayerByGameType(accessToken, gameType.ToString(), guid);

                if (player == null)
                {
                    _logger.LogInformation($"BanFileIngest - creating new player {name} with guid {guid} with import ban");

                    await repositoryApiClient.Players.CreatePlayer(accessToken, new PlayerDto()
                    {
                        GameType = gameType.ToGameType(),
                        Username = name,
                        Guid = guid
                    });

                    player = await repositoryApiClient.Players.GetPlayerByGameType(accessToken, gameType.ToString(), guid);

                    var adminActionDto = new AdminActionDto
                    {
                        Type = AdminActionType.Ban
                    };

                    adminActionDto.PlayerId = player.Id;
                    adminActionDto.GameType = player.GameType;
                    adminActionDto.Username = player.Username;
                    adminActionDto.Guid = player.Guid;

                    adminActionDto.Text = "Imported from server";
                    adminActionDto.ForumTopicId = await _playersForumsClient.CreateTopicForAdminAction(adminActionDto);

                    await repositoryApiClient.Players.CreateAdminActionForPlayer(accessToken, adminActionDto);
                }
                else
                {
                    var adminActions = await repositoryApiClient.AdminActions.GetAdminActions(accessToken, null, player.Id, null, "ActiveBans", 0, 0, null);

                    if (adminActions.Count(aa => aa.Type == AdminActionType.Ban) == 0)
                    {
                        _logger.LogInformation($"BanFileImport - adding import ban to existing player {player.Username} - {player.Guid} ({player.GameType})");

                        var adminActionDto = new AdminActionDto
                        {
                            Type = AdminActionType.Ban
                        };

                        adminActionDto.PlayerId = player.Id;
                        adminActionDto.GameType = player.GameType;
                        adminActionDto.Username = player.Username;
                        adminActionDto.Guid = player.Guid;

                        adminActionDto.Text = "Imported from server";
                        adminActionDto.ForumTopicId = await _playersForumsClient.CreateTopicForAdminAction(adminActionDto);

                        await repositoryApiClient.Players.CreateAdminActionForPlayer(accessToken, adminActionDto);
                    }
                }

            }
        }

        private void ParseLine(string line, out string guid, out string name)
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