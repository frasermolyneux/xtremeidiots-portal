using AutoMapper;

using Newtonsoft.Json;

using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.ChatMessages;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.RecentPlayers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.AutoMapProfiles
{
    public class DbModelsProfile : Profile
    {
        public DbModelsProfile()
        {
            // Nullable Handling
            CreateMap<int?, int>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<long?, long>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<bool?, bool>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<string?, string>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<double?, double>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<Guid?, Guid>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<DateTime?, DateTime>().ConvertUsing((src, dest) => src ?? dest);

            // AdminActions
            CreateMap<AdminAction, AdminActionDto>()
                .ForMember(
                    dest => dest.Type,
                    src => src.MapFrom(src => src.Type.ToAdminActionType())
                )
                .ForMember(
                    dest => dest.PlayerId,
                    src => src.MapFrom(src => src.PlayerPlayerId)
                )
                .ForMember(
                    dest => dest.PlayerDto,
                    src => src.MapFrom(src => src.PlayerPlayer)
                )
                .ForMember(
                    dest => dest.UserProfileDto,
                    src => src.MapFrom(src => src.UserProfile)
                );

            CreateMap<CreateAdminActionDto, AdminAction>();

            CreateMap<EditAdminActionDto, AdminAction>();

            // Ban File Monitors
            CreateMap<BanFileMonitor, BanFileMonitorDto>()
                .ForMember(
                    dest => dest.ServerId,
                    src => src.MapFrom(src => src.GameServerServerId)
                )
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameServerServer.GameType.ToGameType())
                )
                .ForMember(
                    dest => dest.GameServerDto,
                    src => src.MapFrom(src => src.GameServerServer)
                );

            CreateMap<CreateBanFileMonitorDto, BanFileMonitor>()
                .ForMember(
                    dest => dest.GameServerServerId,
                    src => src.MapFrom(src => src.ServerId)
                )
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<EditBanFileMonitorDto, BanFileMonitor>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Chat Messages
            CreateMap<ChatLog, ChatMessageDto>()
                .ForMember(
                    dest => dest.PlayerId,
                    src => src.MapFrom(src => src.PlayerPlayerId)
                )
                .ForMember(
                    dest => dest.ChatType,
                    src => src.MapFrom(src => src.ChatType.ToChatType())
                )
                .ForMember(
                    dest => dest.PlayerDto,
                    src => src.MapFrom(src => src.PlayerPlayer)
                )
                .ForMember(
                    dest => dest.GameServerDto,
                    src => src.MapFrom(src => src.GameServerServer)
                );

            CreateMap<CreateChatMessageDto, ChatLog>()
                .ForMember(
                    dest => dest.ChatType,
                    src => src.MapFrom(src => src.ChatType.ToChatTypeInt())
                )
                .ForMember(
                    dest => dest.GameServerServerId,
                    src => src.MapFrom(src => src.GameServerId)
                )
                .ForMember(
                    dest => dest.PlayerPlayerId,
                    src => src.MapFrom(src => src.PlayerId)
                )
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


            // Demo Auth
            CreateMap<DemoAuthKey, DemoAuthDto>();

            CreateMap<CreateDemoAuthDto, DemoAuthKey>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<EditDemoAuthDto, DemoAuthKey>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Demos
            CreateMap<Demo, DemoDto>()
                .ForMember(
                    dest => dest.Game,
                    src => src.MapFrom(src => src.Game.ToGameType())
                )
                .ForMember(
                    dest => dest.UserId,
                    src => src.MapFrom(src => src.UserProfile.XtremeIdiotsForumId)
                )
                .ForMember(
                    dest => dest.UploadedBy,
                    src => src.MapFrom(src => src.UserProfile.DisplayName)
                )
                .ForMember(
                    dest => dest.UserProfileDto,
                    src => src.MapFrom(src => src.UserProfile)
                );

            CreateMap<CreateDemoDto, Demo>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Game Servers
            CreateMap<GameServer, GameServerDto>()
                .ForMember(
                    dest => dest.Id,
                    src => src.MapFrom(src => src.ServerId)
                )
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameType())
                )
                .ForMember(
                    dest => dest.BanFileMonitorDtos,
                    src => src.MapFrom(src => src.BanFileMonitors)
                )
                .ForMember(
                    dest => dest.LivePlayerDtos,
                    src => src.MapFrom(src => src.LivePlayers)
                );

            CreateMap<CreateGameServerDto, GameServer>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameTypeInt())
                )
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<EditGameServerDto, GameServer>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Game Server Stats
            CreateMap<GameServerStat, GameServerStatDto>();

            CreateMap<CreateGameServerStatDto, GameServerStat>();

            CreateMap<CreateGameServerEventDto, GameServerEvent>();

            // Live Players
            CreateMap<LivePlayer, LivePlayerDto>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameType())
                )
                .ForMember(
                    dest => dest.PlayerDto,
                    src => src.MapFrom(src => src.Player)
                );

            CreateMap<CreateLivePlayerDto, LivePlayer>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameTypeInt())
                )
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Maps
            CreateMap<Map, MapDto>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameType())
                )
                .ForMember(
                    dest => dest.MapFiles,
                    src => src.MapFrom(src => JsonConvert.DeserializeObject<List<MapFileDto>>(src.MapFiles))
                );

            CreateMap<CreateMapDto, MapDto>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameTypeInt())
                )
                .ForMember(
                    dest => dest.MapFiles,
                    src => src.MapFrom(src => JsonConvert.SerializeObject(src.MapFiles))
                )
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<EditMapDto, MapDto>()
                .ForMember(
                    dest => dest.MapFiles,
                    src => src.MapFrom(src => JsonConvert.SerializeObject(src.MapFiles))
                )
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Players
            CreateMap<Player, PlayerDto>()
                .ForMember(
                    dest => dest.Id,
                    src => src.MapFrom(src => src.PlayerId)
                )
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameType())
                )
                .ForMember(
                    dest => dest.AliasDtos,
                    src => src.MapFrom(src => src.PlayerAliases)
                )
                .ForMember(
                    dest => dest.IpAddressDtos,
                    src => src.MapFrom(src => src.PlayerIpAddresses)
                )
                .ForMember(
                    dest => dest.AdminActionDtos,
                    src => src.MapFrom(src => src.AdminActions)
                );

            CreateMap<PlayerAlias, AliasDto>();
            CreateMap<PlayerIpAddress, IpAddressDto>();

            CreateMap<PlayerIpAddress, RelatedPlayerDto>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.PlayerPlayer.GameType.ToGameType())
                )
                .ForMember(
                    dest => dest.Username,
                    src => src.MapFrom(src => src.PlayerPlayer.Username)
                )
                .ForMember(
                    dest => dest.PlayerId,
                    src => src.MapFrom(src => src.PlayerPlayer.PlayerId)
                )
                .ForMember(
                    dest => dest.IpAddress,
                    src => src.MapFrom(src => src.Address)
                );

            CreateMap<CreatePlayerDto, Player>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameTypeInt())
                )
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Recent Players
            CreateMap<RecentPlayer, RecentPlayerDto>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameType())
                )
                .ForMember(
                    dest => dest.PlayerDto,
                    src => src.MapFrom(src => src.Player)
                );

            CreateMap<CreateRecentPlayerDto, RecentPlayer>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameTypeInt())
                )
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Reports
            CreateMap<Report, ReportDto>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameType())
                );

            CreateMap<CreateReportDto, Report>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // User Profile
            CreateMap<UserProfile, UserProfileDto>()
                .ForMember(
                    dest => dest.UserProfileClaimDtos,
                    src => src.MapFrom(src => src.UserProfileClaims)
                );

            CreateMap<UserProfileClaim, UserProfileClaimDto>();

            CreateMap<CreateUserProfileDto, UserProfile>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CreateUserProfileClaimDto, UserProfileClaim>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
