using Monolithic.Repositories.Implement;
using Monolithic.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Monolithic.Services.Implement;
using Monolithic.Services.Interface;
using Monolithic.Models.Context;
using Monolithic.Models.Mapper;
using Microsoft.OpenApi.Models;
using Monolithic.Helpers;
using Monolithic.Common;
using Monolithic.Models.DTO;

namespace Monolithic.Extensions;

public static class ServiceExtension
{
    public static void ConfigureDataContext(this IServiceCollection services, IConfiguration configuration)
    {
        string currentDatabaseConfig = configuration.GetSection("CurrentDatabaseConfig").Value;
        string cns = configuration.GetConnectionString(currentDatabaseConfig);
        services.AddDbContext<DataContext>(options =>
        {
            options.UseMySql(cns, ServerVersion.AutoDetect(cns));
        });
    }

    public static void ConfigureModelSetting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<ClientAppSettings>(configuration.GetSection("ClientAppSettings"));
    }

    public static void ConfigureDI(this IServiceCollection services, IConfigUtil configUtil)
    {
        services.ConfigureLibraryDI();
        services.ConfigureRepositoryDI();
        services.ConfigureServiceDI();
        services.ConfigCommonServiceDI();
        services.ConfigureHelperDI();
        services.ConfigSwagger(configUtil);
    }

    private static void ConfigureLibraryDI(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingConfig));
    }

    private static void ConfigureRepositoryDI(this IServiceCollection services)
    {
        services.AddScoped<INotificationRepository, NotificationRepository>();
    }

    private static void ConfigureServiceDI(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
    }

    private static void ConfigCommonServiceDI(this IServiceCollection services)
    {
        services.AddScoped<IConfigUtil, ConfigUtil>();
    }

    private static void ConfigureHelperDI(this IServiceCollection services)
    {
        services.AddScoped<ISendMailHelper, SendMailHelper>();
        services.AddSingleton<ILoggerManager, LoggerManager>();

        services.AddSingleton<IHttpHelper<UserDTO>, HttpHelper<UserDTO>>();
    }

    private static void ConfigSwagger(this IServiceCollection services, IConfigUtil configUtil)
    {
        // Register the Swagger generator, defining 1 or more Swagger documents
        services.AddSwaggerGen(c =>
        {
            string version = "v" + configUtil.getAPIVersion();
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API PBL6", Version = version });
        });
    }
}