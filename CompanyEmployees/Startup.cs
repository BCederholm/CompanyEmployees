using CompanyEmployees.ActionFilters;
using CompanyEmployees.Extensions;
using CompanyEmployees.Utility;
using Contracts;
using Entities.DataTransferObjects;
using LoggerService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Extensions.Logging;
using Repository.DataShaping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            services.ConfigureCors(); // CodeMaze (custom)
            services.ConfigureIISIntegration(); // CodeMaze (custom)
            services.ConfigureLoggerService(); // CodeMaze (custom)
            services.ConfigureSqlContext(Configuration); // CodeMaze (custom)
            services.ConfigureRepositoryManager(); // CodeMaze (custom)
            services.AddAutoMapper(typeof(Startup)); // CodeMaze
            services.AddScoped<ValidationFilterAttribute>(); // CodeMaze (custom)
            services.AddScoped<ValidateCompanyExistsAttribute>(); // CodeMaze (custom)
            services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>(); // CodeMaze (custom)

            services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>(); // CodeMaze (custom)

            services.AddScoped<ValidateMediaTypeAttribute>(); // CodeMaze (custom)

            services.AddScoped<EmployeeLinks>(); // CodeMaze (custom)

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true; // CodeMaze
            });
            // services.AddControllers(); // CodeMaze (replaced)
            services.AddControllers(config =>
            {
                config.RespectBrowserAcceptHeader = true;
                config.ReturnHttpNotAcceptable = true;
            }).AddNewtonsoftJson() // CodeMaze
              .AddXmlDataContractSerializerFormatters() // CodeMaze
              .AddCustomCSVFormatter(); // CodeMaze
            services.AddCustomMediaTypes();
            // services.AddRazorPages(); // CodeMaze (removed)
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.ConfigureExceptionHandler(logger); // CodeMaze (custom)
            app.UseHttpsRedirection();
            app.UseStaticFiles(); // CodeMaze

            app.UseForwardedHeaders(new ForwardedHeadersOptions // CodeMaze
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.UseRouting();
            app.UseCors("CorsPolicy"); // CodeMaze

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // CodeMaze
                // endpoints.MapRazorPages(); // CodeMaze (removed)
            });
        }
    }
}
