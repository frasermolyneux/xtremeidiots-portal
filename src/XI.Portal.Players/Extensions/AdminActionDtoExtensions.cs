using System;
using XI.CommonTypes;
using XI.Portal.Players.Dto;

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
            adminActionDto.PlayerId = playerDto.PlayerId;
            adminActionDto.GameType = playerDto.GameType;
            adminActionDto.Username = playerDto.Username;
            adminActionDto.Guid = playerDto.Guid;

            return adminActionDto;
        }
    }
}