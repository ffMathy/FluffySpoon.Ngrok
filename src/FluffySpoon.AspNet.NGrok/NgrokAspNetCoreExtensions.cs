// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Copyright (c) 2019 Kevin Gysberg

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluffySpoon.AspNet.Ngrok.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluffySpoon.AspNet.Ngrok
{
	public static class NgrokAspNetCoreExtensions
	{
        public static IApplicationBuilder UseNgrokAutomaticUrlDetection(this IApplicationBuilder app)
        {
            var service = app.ApplicationServices.GetRequiredService<NgrokHostedService>();
            service.InjectServerAddressesFeature(app.ServerFeatures.Get<IServerAddressesFeature>());

            return app;
        }

        public static IServiceCollection AddNgrok(this IServiceCollection services)
        {
            services.TryAddSingleton<NgrokProcess>();

            services.AddHttpClient<NgrokDownloader>();
            services.AddHttpClient<NgrokApiClient>();

            services.AddLogging();

            services.AddSingleton<NgrokHostedService>();
            services.AddSingleton<INgrokHostedService>(p => p.GetRequiredService<NgrokHostedService>());

            return services;
        }

        public static IWebHostBuilder UseNgrok(this IWebHostBuilder builder, NgrokOptions? options = null)
        {
            return builder
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(options ?? new NgrokOptions());
                    services.AddSingleton<IHostedService>(p => p.GetRequiredService<NgrokHostedService>());
                });
        }
	}
}
