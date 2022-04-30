using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Extensions;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Players.Ingest
{
    internal class BanFileIngest : IBanFileIngest
    {
        private readonly ILogger<BanFileIngest> _logger;
        private readonly IGuidValidator _guidValidator;
        private readonly IAdminActionsRepository _adminActionsRepository;
        private readonly IPlayersForumsClient _playersForumsClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;

        public BanFileIngest(
            ILogger<BanFileIngest> logger,
            IGuidValidator guidValidator,
            IAdminActionsRepository adminActionsRepository,
            IPlayersForumsClient playersForumsClient,
            IRepositoryApiClient repositoryApiClient,
            IRepositoryTokenProvider repositoryTokenProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _guidValidator = guidValidator ?? throw new ArgumentNullException(nameof(guidValidator));
            _adminActionsRepository = adminActionsRepository ?? throw new ArgumentNullException(nameof(adminActionsRepository));
            _playersForumsClient = playersForumsClient ?? throw new ArgumentNullException(nameof(playersForumsClient));
            this.repositoryApiClient = repositoryApiClient;
            this.repositoryTokenProvider = repositoryTokenProvider;
        }

        public async Task IngestBanFileDataForGame(GameType gameType, string remoteBanFileData)
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
                var player = await repositoryApiClient.PlayersApiClient.GetPlayerByGameType(accessToken, gameType.ToString(), guid);

                if (player == null)
                {
                    _logger.LogInformation($"BanFileIngest - creating new player {name} with guid {guid} with import ban");

                    await repositoryApiClient.PlayersApiClient.CreatePlayer(accessToken, new XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models.PlayerDto()
                    {
                        GameType = gameType.ToString(),
                        Username = name,
                        Guid = guid
                    });

                    player = await repositoryApiClient.PlayersApiClient.GetPlayerByGameType(accessToken, gameType.ToString(), guid);

                    var adminActionDto = new AdminActionDto()
                        .OfType(AdminActionType.Ban);

                    adminActionDto.PlayerId = player.Id;
                    adminActionDto.GameType = Enum.Parse<GameType>(player.GameType);
                    adminActionDto.Username = player.Username;
                    adminActionDto.Guid = player.Guid;

                    adminActionDto.Text = "Imported from server";
                    adminActionDto.ForumTopicId = await _playersForumsClient.CreateTopicForAdminAction(adminActionDto);

                    await _adminActionsRepository.CreateAdminAction(adminActionDto);
                }
                else
                {
                    var adminActions = await _adminActionsRepository.GetAdminActions(new AdminActionsFilterModel
                    {
                        PlayerId = player.Id,
                        Filter = AdminActionsFilterModel.FilterType.ActiveBans
                    });

                    if (adminActions.Count(aa => aa.Type == AdminActionType.Ban) == 0)
                    {
                        _logger.LogInformation($"BanFileImport - adding import ban to existing player {player.Username} - {player.Guid} ({player.GameType})");

                        var adminActionDto = new AdminActionDto()
                            .OfType(AdminActionType.Ban);

                        adminActionDto.PlayerId = player.Id;
                        adminActionDto.GameType = Enum.Parse<GameType>(player.GameType);
                        adminActionDto.Username = player.Username;
                        adminActionDto.Guid = player.Guid;

                        adminActionDto.Text = "Imported from server";
                        adminActionDto.ForumTopicId = await _playersForumsClient.CreateTopicForAdminAction(adminActionDto);

                        await _adminActionsRepository.CreateAdminAction(adminActionDto);
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