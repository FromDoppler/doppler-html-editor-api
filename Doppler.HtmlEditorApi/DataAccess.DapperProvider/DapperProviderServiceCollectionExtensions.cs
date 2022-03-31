using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.DataAccess.DapperProvider;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class DapperProviderServiceCollectionExtensions
{
    static public IServiceCollection AddDapperDataAccessProvider(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>()
            .AddScoped<IDbContext, DapperWrapperDbContext>()
            .Configure<DatabaseSettings>(configuration.GetSection("DopplerDatabaseSettings"));
}
