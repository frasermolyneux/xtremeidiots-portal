using AutoMapper;

using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
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
            // AdminActions
            CreateMap<AdminAction, AdminActionDto>()
                .ForMember(
                    dest => dest.Type,
                    src => src.MapFrom(src => src.Type.ToAdminActionType())
                );

            // Live Players
            CreateMap<LivePlayer, LivePlayerDto>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameType())
                );

            CreateMap<CreateLivePlayerDto, LivePlayer>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameTypeInt())
                );

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

            // Recent Players
            CreateMap<RecentPlayer, RecentPlayerDto>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameType())
                );

            CreateMap<CreateRecentPlayerDto, RecentPlayer>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameTypeInt())
                );

            // Reports
            CreateMap<Report, ReportDto>()
                .ForMember(
                    dest => dest.GameType,
                    src => src.MapFrom(src => src.GameType.ToGameType())
                );

            CreateMap<CreateReportDto, Report>();

            // User Profile
            CreateMap<UserProfile, UserProfileDto>()
                .ForMember(
                    dest => dest.UserProfileClaimDtos,
                    src => src.MapFrom(src => src.UserProfileClaims)
                );

            CreateMap<UserProfileClaim, UserProfileClaimDto>();

            CreateMap<CreateUserProfileDto, UserProfile>();

            CreateMap<CreateUserProfileClaimDto, UserProfileClaim>();
        }
    }
}
