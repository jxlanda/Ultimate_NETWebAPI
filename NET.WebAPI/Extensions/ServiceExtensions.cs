using Contracts;
using Entities;
using LoggerService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NET5.WebAPI.Filters;
using Repository;
using System.Linq;

namespace NET5.WebAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
        }

        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            services.Configure<IISOptions>(options =>
            {
            });

        }

        public static void RegisterFilters(this IServiceCollection services)
        {
            services.AddScoped<ValidateMediaTypeAttribute>();
        }

        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerManager, LoggerManager>();
        }

        public static void ConfigureRepositoryWrapper(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
        }

        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration config)
        {
            // MSQL SERVER
            var connectionString = config["ConnectionStrings:MMSQLServerString"];
            services.AddDbContext<RepositoryContext>(o => o.UseSqlServer(connectionString));

            // MYSQL

            //var connectionString = config["ConnectionStrings:connectionString"];
            //services.AddDbContext<RepositoryContext>(o => o.UseMySql(connectionString, MySqlServerVersion.LatestSupportedServerVersion));

            // POSTGRESQL

            //var connectionString = config["ConnectionStrings:connectionStringPostgreSQL"];
            //services.AddDbContext<ProvinecContext>(o => o.UseNpgsql(connectionString));
        }

        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter = config.OutputFormatters
                        .OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();
                if (newtonsoftJsonOutputFormatter != null)
                {
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.example.hateoas+json");
                }
                var xmlOutputFormatter = config.OutputFormatters
                        .OfType<XmlDataContractSerializerOutputFormatter>()?.FirstOrDefault();
                if (xmlOutputFormatter != null)
                {
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.example.hateoas+xml");
                }
            });
        }

    }
}
