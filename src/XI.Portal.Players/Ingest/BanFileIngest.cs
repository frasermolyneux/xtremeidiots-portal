using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Ingest
{
    internal class BanFileIngest : IBanFileIngest
    {
        private readonly ILogger<BanFileIngest> _logger;
        private readonly IGuidValidator _guidValidator;
        private readonly IPlayersRepository _playersRepository;
        private readonly IAdminActionsRepository _adminActionsRepository;
        private readonly IPlayersForumsClient _playersForumsClient;

        public BanFileIngest(
            ILogger<BanFileIngest> logger,
            IGuidValidator guidValidator,
            IPlayersRepository playersRepository, 
            IAdminActionsRepository adminActionsRepository,
            IPlayersForumsClient playersForumsClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _guidValidator = guidValidator ?? throw new ArgumentNullException(nameof(guidValidator));
            _playersRepository = playersRepository ?? throw new ArgumentNullException(nameof(playersRepository));
            _adminActionsRepository = adminActionsRepository ?? throw new ArgumentNullException(nameof(adminActionsRepository));
            _playersForumsClient = playersForumsClient ?? throw new ArgumentNullException(nameof(playersForumsClient));
        }

        public async Task IngestBanFileDataForGame(GameType gameType, string remoteBanFileData)
        {
            var skipTags = new[] { "[PBBAN]", "[B3BAN]", "[BANSYNC]", "[EXTERNAL]" };

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

                var player = await _playersRepository.GetPlayer(gameType, guid);

                if (player == null)
                {
                    _logger.LogInformation($"BanFileIngest - creating new player {name} with guid {guid} with import ban");

                    await _playersRepository.CreatePlayer(new PlayerDto
                    {
                        GameType = gameType,
                        Username = name,
                        Guid = guid
                    });

                    player = await _playersRepository.GetPlayer(gameType, guid);

                    var adminActionDto = new AdminActionDto
                    {
                        PlayerId = player.PlayerId,
                        Type = AdminActionType.Ban,
                        Username = name,
                        Text = "Imported from server",
                        Created = DateTime.UtcNow
                    };

                    adminActionDto.ForumTopicId = await _playersForumsClient.CreateTopicForAdminAction(adminActionDto);

                    await _adminActionsRepository.CreateAdminAction(adminActionDto);
                }
                else
                {
                    var adminActions = await _adminActionsRepository.GetAdminActions(new AdminActionsFilterModel
                    {
                        PlayerId = player.PlayerId,
                        Filter = AdminActionsFilterModel.FilterType.ActiveBans
                    });

                    if (adminActions.Count(aa => aa.Type == AdminActionType.Ban) == 0)
                    {
                        _logger.LogInformation($"BanFileImport - adding import ban to existing player {player.Username} - {player.Guid} ({player.GameType})");

                        var adminActionDto = new AdminActionDto
                        {
                            PlayerId = player.PlayerId,
                            Type = AdminActionType.Ban,
                            Username = name,
                            Text = "Imported from server",
                            Created = DateTime.UtcNow
                        };

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