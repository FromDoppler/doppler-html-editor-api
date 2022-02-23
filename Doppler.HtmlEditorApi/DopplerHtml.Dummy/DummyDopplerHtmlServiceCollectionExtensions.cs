using Doppler.HtmlEditorApi.DopplerHtml;
using Doppler.HtmlEditorApi.DopplerHtml.Dummy;

namespace Microsoft.Extensions.DependencyInjection;

public static class DummyDopplerHtmlServiceCollectionExtensions
{
    static public IServiceCollection AddDummyDopplerHtml(this IServiceCollection services)
        => services
            .AddSingleton<IDopplerHtmlProcessor, DummyDopplerHtmlProcessor>();
}
