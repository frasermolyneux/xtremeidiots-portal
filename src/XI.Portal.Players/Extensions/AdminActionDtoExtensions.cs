using System;
using XI.CommonTypes;
using XI.Portal.Players.Dto;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XI.Portal.Players.Extensions
{
    public static class AdminActionDtoExtensions
    {
        public static AdminActionDto OfType(this AdminActionDto adminActionDto, AdminActionType adminActionType)
        {
            adminActionDto.Type = adminActionType;

            return adminActionDto;
        }

        public static AdminActionDto WithPlayerDto(this AdminActionDto adminActionDto, PlayerDto playerDto)
        {
            adminActionDto.PlayerId = playerDto.Id;
            adminActionDto.GameType = Enum.Parse<GameType>(playerDto.GameType);
            adminActionDto.Username = playerDto.Username;
            adminActionDto.Guid = playerDto.Guid;

            return adminActionDto;
        }
    }
}