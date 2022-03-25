using System;
using System.Data;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.Storage;
using Doppler.HtmlEditorApi.Storage.DapperProvider;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    static public IServiceCollection AddDapperProvider(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddScoped<ICampaignContentRepository, DapperCampaignContentRepository>()
            .AddScoped<IFieldsRepository, DapperFieldsRepository>()
            .AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>()
            .AddScoped<IDbContext, DapperWrapperDbContext>()
            .Configure<DopplerDatabaseSettings>(configuration.GetSection(nameof(DopplerDatabaseSettings)));
}
