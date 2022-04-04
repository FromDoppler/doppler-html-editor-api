using System.Text.Json.Serialization;

namespace Doppler.HtmlEditorApi.ApiModels;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContentType
{
    unset = 0,
    html,
    unlayer,
}
