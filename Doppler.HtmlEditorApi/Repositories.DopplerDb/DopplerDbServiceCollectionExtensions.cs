using Doppler.HtmlEditorApi.Repositories;
using Doppler.HtmlEditorApi.Repositories.DopplerDb;

namespace Microsoft.Extensions.DependencyInjection;

public static class DopplerDbServiceCollectionExtensions
{
    public static IServiceCollection AddDopplerDbRepositories(this IServiceCollection services)
        => services
            .AddScoped<ICampaignContentRepository, DapperCampaignContentRepository>()
            .AddScoped<IFieldsRepository, DapperFieldsRepository>();
}
