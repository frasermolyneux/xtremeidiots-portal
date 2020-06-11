﻿using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Servers.Integrations.Interfaces;

namespace XI.Portal.Servers.Integrations.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddChatCommands(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChatCommand, DucCommand>();
            serviceCollection.AddScoped<IChatCommand, FuckYouCommand>();
            serviceCollection.AddScoped<IChatCommand, LikeMapCommand>();
        }
    }
}