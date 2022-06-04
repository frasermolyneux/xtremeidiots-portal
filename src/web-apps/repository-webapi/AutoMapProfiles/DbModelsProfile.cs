using AutoMapper;

using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.RecentPlayers;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.AutoMapProfiles
{
    public class DbModelsProfile : Profile
    {
        public DbModelsProfile()
        {
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
        }
    }
}
