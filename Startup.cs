using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using Rollbar.AspNetCore;
using WaitTime.Entities;
using Anemonis.AspNetCore.RequestDecompression;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace WaitTime
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //Rollbar needs HttpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            if (!this.Environment.IsDevelopment())
            {
                //Use Rollbar for logging if not development
                services.AddRollbarLogger();
            }

            //explicitly inject WaitTimeContext into controller instances
            services.AddDbContext<WaitTimeContext>(options => options.UseSqlServer(Configuration["DbConnection"]));

            services.AddRequestDecompression(o =>
            {
                o.Providers.Add<GzipDecompressionProvider>();
            });
            services.AddResponseCompression();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://securetoken.google.com/we-ski";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "https://securetoken.google.com/we-ski",
                        ValidateAudience = true,
                        ValidAudience = "we-ski",
                        ValidateLifetime = true
                    };
                });

            services.AddMvc();

            // React app production build static file location
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // Add middleware to pipeline
        public void Configure(IApplicationBuilder app)
        {
            if (!this.Environment.IsDevelopment())
            {
                //Use Rollbar to log all unhandled exceptions
                app.UseRollbarMiddleware();
                //app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRequestDecompression();
            app.UseResponseCompression();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (this.Environment.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
