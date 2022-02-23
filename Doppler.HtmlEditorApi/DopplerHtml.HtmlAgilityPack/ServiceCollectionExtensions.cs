using Doppler.HtmlEditorApi.DopplerHtml;
using Doppler.HtmlEditorApi.DopplerHtml.HtmlAgilityPack;

namespace Microsoft.Extensions.DependencyInjection;

public static class AgilityPackDopplerHtmlServiceCollectionExtensions
{
    static public IServiceCollection AddAgilityPackDopplerHtml(this IServiceCollection services)
        => services
            .AddSingleton<IDopplerHtmlProcessor, AgilityPackDopplerHtmlProcessor>();
}
