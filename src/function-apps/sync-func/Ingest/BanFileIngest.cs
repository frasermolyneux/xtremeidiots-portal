using Microsoft.Extensions.Logging;
using XtremeIdiots.Portal.ForumsIntegration;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Extensions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
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
                var player = await repositoryApiClient.Players.GetPlayerByGameType(gameTypeEnum, guid);

                if (player == null)
                {
                    _logger.LogInformation($"BanFileIngest - creating new player {name} with guid {guid} with import ban");

                    await repositoryApiClient.Players.CreatePlayer(new PlayerDto()
                    {
                        GameType = gameType.ToGameType(),
                        Username = name,
                        Guid = guid
                    });

                    player = await repositoryApiClient.Players.GetPlayerByGameType(gameTypeEnum, guid);

                    var adminActionDto = new AdminActionDto
                    {
                        Type = AdminActionType.Ban
                    };

                    adminActionDto.PlayerId = player.Id;
                    adminActionDto.GameType = player.GameType;
                    adminActionDto.Username = player.Username;
                    adminActionDto.Guid = player.Guid;

                    adminActionDto.Text = "Imported from server";
                    adminActionDto.ForumTopicId = await adminActionTopics.CreateTopicForAdminAction(adminActionDto);

                    await repositoryApiClient.Players.CreateAdminActionForPlayer(adminActionDto);
                }
                else
                {
                    var adminActions = await repositoryApiClient.AdminActions.GetAdminActions(null, player.Id, null, "ActiveBans", 0, 0, null);

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
                        adminActionDto.ForumTopicId = await adminActionTopics.CreateTopicForAdminAction(adminActionDto);

                        await repositoryApiClient.Players.CreateAdminActionForPlayer(adminActionDto);
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