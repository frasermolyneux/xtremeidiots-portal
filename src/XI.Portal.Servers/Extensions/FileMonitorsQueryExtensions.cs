﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Extensions
{
    public static class FileMonitorsQueryExtensions
    {
        public static IQueryable<FileMonitors> ApplyAuth(this IQueryable<FileMonitors> fileMonitors, ClaimsPrincipal claimsPrincipal, IEnumerable<string> requiredClaims)
        {
            if (claimsPrincipal == null || requiredClaims == null)
                return fileMonitors.AsQueryable();

            var (gameTypes, serverIds) = claimsPrincipal.ClaimedGamesAndItems(requiredClaims);
            var query = fileMonitors.Include(monitor => monitor.GameServerServer).AsQueryable();

            return query.Where(server => gameTypes.Contains(server.GameServerServer.GameType)).AsQueryable();
        }
    }
}