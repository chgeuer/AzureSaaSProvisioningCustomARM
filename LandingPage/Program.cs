namespace LandingPage
{
    using System.IO;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using LettuceEncrypt;
    using Utils;

    public class Program
    {
        public static void Main(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables(prefix: "LandingPage_");
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                .ConfigureAppConfiguration(
                    config =>
                    {
                        // set LandingPage_ConnectionStrings__AzureAppConfigurationConnectionString="Endpoint=https://chgeuer.azconfig.io;Id=K..."
                        var connectionString = config.Build().GetConnectionString("AzureAppConfigurationConnectionString");
                        config.AddAzureAppConfiguration(connectionString: connectionString);
                    })
                    .PreferHostingUrls(false)
                    .UseUrls("http://*", "https://*")
                    .UseStartup<Startup>()
                    .UseKestrel(kestrelServerOptions =>
                    {
                        kestrelServerOptions.ConfigureHttpsDefaults(h =>
                        {
                            h.UseLettuceEncrypt(kestrelServerOptions.ApplicationServices);
                        });
                        kestrelServerOptions.ListenAnyIP(
                            port: 80,
                            configure: lo =>
                            {
                                lo.Protocols = HttpProtocols.Http1AndHttp2;
                            }
                        );

                        kestrelServerOptions.ListenAnyIP(
                            port: 443,
                            configure: lo =>
                            {
                                lo.Protocols = HttpProtocols.Http1AndHttp2;
                                lo.UseHttps(h => h.UseLettuceEncrypt(kestrelServerOptions.ApplicationServices));
                            }
                        );
                    }
                );
            }
        )
        .Build()
        .Run();
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
               .AddLettuceEncrypt()
               .PersistDataToDirectory(new DirectoryInfo(@"."), Configuration["ApiKey"]);

            services.AddRazorPages();
            services.AddControllers();
            services.AddSingleton(new DeploymentControllerAppConfiguration { ApiKey = Configuration["ApiKey"] });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}