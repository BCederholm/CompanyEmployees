using AspNetCoreRateLimit;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Extensions;
using CompanyEmployees.Utility;
using Contracts;
using Entities.DataTransferObjects;
using LoggerService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using Repository.DataShaping;

namespace CompanyEmployees
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            // LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config")); // CodeMaze (old)
            var config = new ConfigurationBuilder() // CodeMaze (own)
            .SetBasePath(System.IO.Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            NLog.LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog")); // CodeMaze (own)

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureCors(); // CodeMaze 1 (custom)
            services.ConfigureIISIntegration(); // CodeMaze 1 (custom)
            services.ConfigureLoggerService(); // CodeMaze 2 (custom)
            services.ConfigureSqlContext(Configuration); // CodeMaze 3 (custom)
            services.ConfigureRepositoryManager(); // CodeMaze 3 (custom)
            services.AddAutoMapper(typeof(Startup)); // CodeMaze 4
            services.AddScoped<ValidationFilterAttribute>(); // CodeMaze 15 (custom)
            services.AddScoped<ValidateCompanyExistsAttribute>(); // CodeMaze 15 (custom)
            services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>(); // CodeMaze 15 (custom)

            services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>(); // CodeMaze 20 (custom)

            services.AddScoped<ValidateMediaTypeAttribute>(); // CodeMaze 21 (custom)

            services.AddScoped<EmployeeLinks>(); // CodeMaze 21 (custom)

            services.ConfigureVersioning(); // CodeMaze 24 (custom)

            services.ConfigureResponseCaching(); // CodeMaze 25 (custom)
            services.ConfigureHttpCacheHeaders(); // CodeMaze 25 (custom)

            services.AddMemoryCache(); // CodeMaze 26

            services.ConfigureRateLimitingOptions(); // CodeMaze 26
            services.AddHttpContextAccessor(); // CodeMaze 26

            services.AddAuthentication(); // CodeMaze 27
            services.ConfigureIdentity(); // CodeMaze 27
            services.ConfigureJWT(Configuration); // CodeMaze 27
            services.AddScoped<IAuthenticationManager, AuthenticationManager>(); // CodeMaze 27

            services.ConfigureSwagger(); // CodeMaze 28

            services.Configure<ApiBehaviorOptions>(options => // CodeMaze 13
            {
                options.SuppressModelStateInvalidFilter = true; // CodeMaze 13
            });

            services.AddControllers(config => // CodeMaze 7
            {
                config.RespectBrowserAcceptHeader = true;
                config.ReturnHttpNotAcceptable = true;
                config.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 }); // CodeMaze 25
            }).AddNewtonsoftJson() // CodeMaze 12
              .AddXmlDataContractSerializerFormatters() // CodeMaze 7
              .AddCustomCSVFormatter(); // CodeMaze 7
            services.AddCustomMediaTypes(); // CodeMaze 21

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerManager logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {                
                app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            }

            app.ConfigureExceptionHandler(logger); // CodeMaze 5 (custom)
            app.UseHttpsRedirection();
            app.UseStaticFiles(); // CodeMaze 1

            app.UseForwardedHeaders(new ForwardedHeadersOptions // CodeMaze 1
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.UseIpRateLimiting(); // CodeMaze 26

            app.UseRouting();
            app.UseCors("CorsPolicy"); // CodeMaze 1
            app.UseResponseCaching(); // CodeMaze 25
            app.UseHttpCacheHeaders(); // CodeMaze 25

            app.UseAuthentication(); // CodeMaze 27
            app.UseAuthorization();

            app.UseSwagger(); // CodeMaze 28
            app.UseSwaggerUI(s => // CodeMaze 28
            {
                s.SwaggerEndpoint("/swagger/v1/swagger.json", "Code Maze API v1");
                s.SwaggerEndpoint("/swagger/v2/swagger.json", "Code Maze API v2");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
