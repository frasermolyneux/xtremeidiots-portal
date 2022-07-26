﻿using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.ServersApi.Abstractions;

namespace XtremeIdiots.Portal.ServersWebApi.Extensions
{
    public static class ApiResponseDtoExtensions
    {
        public static IActionResult ToHttpResult<T>(this ApiResponseDto<T> apiResponseDto)
        {
            return new ObjectResult(apiResponseDto)
            {
                StatusCode = (int?)apiResponseDto.StatusCode
            };
        }

        public static IActionResult ToHttpResult(this ApiResponseDto apiResponseDto)
        {
            return new ObjectResult(apiResponseDto)
            {
                StatusCode = (int?)apiResponseDto.StatusCode
            };
        }
    }
}
