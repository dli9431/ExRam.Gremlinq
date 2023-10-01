﻿using System.Net.WebSockets;
using ExRam.Gremlinq.Core;

using Gremlin.Net.Driver;
using Microsoft.Extensions.Configuration;

namespace ExRam.Gremlinq.Providers.Core.AspNet
{
    public static class ProviderConfiguratorExtensions
    {
        private sealed class ConnectionPoolSettingsGremlinClientFactory : IGremlinClientFactory
        {
            private readonly IGremlinClientFactory _factory;
            private readonly IConfigurationSection _section;

            public ConnectionPoolSettingsGremlinClientFactory(IGremlinClientFactory factory, IConfigurationSection section)
            {
                _factory = factory;
                _section = section;
            }

            public IGremlinClient Create(IGremlinQueryEnvironment environment, GremlinServer gremlinServer, IMessageSerializer messageSerializer, ConnectionPoolSettings connectionPoolSettings, Action<ClientWebSocketOptions> webSocketConfiguration, string? sessionId = null)
            {
                if (int.TryParse(_section[$"{nameof(ConnectionPoolSettings.MaxInProcessPerConnection)}"], out var maxInProcessPerConnection))
                    connectionPoolSettings.MaxInProcessPerConnection = maxInProcessPerConnection;

                if (int.TryParse(_section[$"{nameof(ConnectionPoolSettings.PoolSize)}"], out var poolSize))
                    connectionPoolSettings.PoolSize = poolSize;

                return _factory.Create(environment, gremlinServer, messageSerializer, connectionPoolSettings, webSocketConfiguration, sessionId);
            }
        }

        public static IGremlinqProviderServicesBuilder<TConfigurator> ConfigureBase<TConfigurator>(this IGremlinqProviderServicesBuilder<TConfigurator> builder)
            where TConfigurator : IProviderConfigurator<TConfigurator>
        {
            return builder
                .Configure((configurator, section) =>
                {
                    if (section.GremlinqSection["Alias"] is { Length: > 0 } alias)
                    {
                        configurator = configurator
                            .ConfigureQuerySource(source => source
                                .ConfigureEnvironment(env => env
                                    .ConfigureOptions(options => options
                                        .SetValue(GremlinqOption.Alias, alias))));
                    }

                    return configurator;
                });
        }

        public static IGremlinqProviderServicesBuilder<TConfigurator> ConfigureWebSocket<TConfigurator>(this IGremlinqProviderServicesBuilder<TConfigurator> builder)
            where TConfigurator : IWebSocketProviderConfigurator<TConfigurator>
        {
            return builder
                .Configure((configurator, section) =>
                {
                    var authenticationSection = section.GetSection("Authentication");
                    var connectionPoolSection = section.GetSection("ConnectionPool");

                    if (section["Uri"] is { } uri)
                        configurator = configurator.At(uri);

                    configurator
                        .ConfigureClientFactory(factory => new ConnectionPoolSettingsGremlinClientFactory(factory, connectionPoolSection));

                    if (authenticationSection["Username"] is { } username && authenticationSection["Password"] is { } password)
                        configurator = configurator.AuthenticateBy(username, password);

                    return configurator;
                });
        }
    }
}
