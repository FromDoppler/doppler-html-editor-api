using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Doppler.HtmlEditorApi.Domain;

/// <summary>
/// This class deals with the conversion between a standard HTML content
/// and the set of strings that represents the content in Doppler DB.
/// It is named Doppler because it should replicate current Doppler logic.
/// </summary>
public class DopplerHtmlDocument
{
    // Old Doppler code:
    // https://github.com/MakingSense/Doppler/blob/ed24e901c990b7fb2eaeaed557c62c1adfa80215/Doppler.HypermediaAPI/ApiMappers/ToDoppler/CampaignContent_To_DtoContent.cs#L27-L29

    // TODO: consider remove the linebreaks in hrefs:
    // * https://github.com/MakingSense/Doppler/blob/ed24e901c990b7fb2eaeaed557c62c1adfa80215/Doppler.Domain.Core/Classes/Utils.cs#L147-L151
    // TODO: consider sanitize field names
    // Searching the canonical name in FieldsRes.resx (replacing " " and "%20" by "_" in the key)
    // * https://github.com/MakingSense/Doppler/blob/e0bf2aa982ac8b9902430395fd293fdcd3c231a8/Doppler.Application.CampaignsModule/Services/Classes/CampaignContentService.cs#L3428
    // * https://github.com/MakingSense/Doppler/blob/e0bf2aa982ac8b9902430395fd293fdcd3c231a8/Doppler.Application.ListsModule/Utils/FieldHelper.cs#L65-L81
    // * https://github.com/MakingSense/Doppler/blob/e0bf2aa982ac8b9902430395fd293fdcd3c231a8/Doppler.Application.ListsModule/Utils/FieldHelper.cs#L113-L134
    // * https://github.com/MakingSense/Doppler/blob/develop/Doppler.Recursos/FieldsRes.resx

    private const string FieldNameTagStartDelimiter = "[[[";
    private const string FieldNameTagEndDelimiter = "]]]";
    private const string FieldIdTagStartDelimiter = "|*|";
    private const string FieldIdTagEndDelimiter = "*|*";
    // &, # and ; are here to accept HTML Entities
    private static readonly Regex FieldNameTagRegex = new Regex($@"{Regex.Escape(FieldNameTagStartDelimiter)}([a-zA-Z0-9 \-_ñÑáéíóúÁÉÍÓÚ%&;#]+){Regex.Escape(FieldNameTagEndDelimiter)}");
    private static readonly Regex FieldIdTagRegex = new Regex($@"{Regex.Escape(FieldIdTagStartDelimiter)}(\d+){Regex.Escape(FieldIdTagEndDelimiter)}");
    private static readonly Regex CleanupUrlRegex = new Regex(@"^(?:\s?(?:%20)?)*|(?:\s?(?:%20)?)*$|\s");
    private static readonly Regex TrackableUrlAcceptanceRegex = new Regex(@"^(?:\s?(?:%20)?)*(?:(?:https?|ftp):\/\/|www\.)", RegexOptions.IgnoreCase);
    private static readonly Regex TrackableUrlPartsRegex = new Regex(@"^(?:(?<scheme>(?:https?|ftp):\/\/)(?<domain>[^\/]+)|(?<domainWithoutScheme>www\.[^\/]+))(?<rest>\/.*)?$", RegexOptions.IgnoreCase);

    private readonly HtmlNode _headNode;
    private readonly HtmlNode _contentNode;
    private readonly HtmlNode _rootNode;

    public DopplerHtmlDocument(string inputHtml)
    {
        var htmlDocument = HtmlAgilityPackUtils.LoadHtml(inputHtml);

        _headNode = htmlDocument.DocumentNode.SelectSingleNode("//head");

        _contentNode = _headNode == null ? htmlDocument.DocumentNode
            : htmlDocument.DocumentNode.SelectSingleNode("//body")
            ?? HtmlAgilityPackUtils.LoadHtml(inputHtml.Replace(_headNode.OuterHtml, string.Empty)).DocumentNode;

        _rootNode = _contentNode.OwnerDocument.DocumentNode;
    }

    public string GetDopplerContent()
        => EnsureContent(_contentNode.InnerHtml);

    public string GetHeadContent()
        => _headNode?.InnerHtml;

    public IEnumerable<int> GetFieldIds()
        => FieldIdTagRegex.Matches(_contentNode.InnerHtml)
            .Select(x => int.Parse(x.Groups[1].ValueSpan))
            .Distinct();

    public IEnumerable<string> GetTrackableUrls()
        => GetTrackableLinkNodes()
            .Select(x => x.Attributes["href"].Value)
            .Distinct();

    public void SanitizeTrackableLinks()
    {
        var trackableLinksNodes = GetTrackableLinkNodes();

        foreach (var node in trackableLinksNodes)
        {
            var originalHref = node.Attributes["href"].Value;
            var sanitizedUrl = SanitizedUrl(originalHref);
            if (sanitizedUrl != originalHref)
            {
                node.Attributes["href"].Value = sanitizedUrl;
            }
        }
    }

    public void RemoveHarmfulTags()
    {
        var harmfulTags = _rootNode.SelectNodes(@"//script|//embed|//iframe|//meta[contains(@http-equiv,'refresh')]").EmptyIfNull();
        foreach (var tag in harmfulTags)
        {
            tag.Remove();
        }
    }

    public void RemoveEventAttributes()
    {
        var eventAttributes = _rootNode.Descendants()
            .SelectMany(x => x.Attributes.Where(x => x.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var attribute in eventAttributes)
        {
            attribute.Remove();
        }
    }

    public void ReplaceFieldNameTagsByFieldIdTags(Func<string, int?> getFieldIdOrNullFunc)
    {
        _contentNode.TraverseAndReplaceTextsAndAttributeValues(text => FieldNameTagRegex.Replace(
            text,
            match =>
            {
                var fieldName = HtmlEntity.DeEntitize(match.Groups[1].Value.Replace("%20", " "));
                var fieldId = getFieldIdOrNullFunc(fieldName);
                return fieldId.HasValue
                    ? CreateFieldIdTag(fieldId.GetValueOrDefault())
                    // keep the name when field doesn't exist
                    : match.Value;
            }));
    }

    public void RemoveUnknownFieldIdTags(Func<int, bool> fieldIdExistFunc)
    {
        _contentNode.TraverseAndReplaceTextsAndAttributeValues(text => FieldIdTagRegex.Replace(
            text,
            match => fieldIdExistFunc(int.Parse(match.Groups[1].ValueSpan))
                ? match.Value
                : string.Empty));
    }

    private static string CreateFieldIdTag(int? fieldId)
        => $"{FieldIdTagStartDelimiter}{fieldId}{FieldIdTagEndDelimiter}";

    private static string EnsureContent(string htmlContent)
        => string.IsNullOrWhiteSpace(htmlContent) ? "<BR>" : htmlContent;

    private static string SanitizedUrl(string url)
    {
        var withoutSpaces = CleanupUrlRegex.Replace(url, string.Empty);

        var match = TrackableUrlPartsRegex.Match(withoutSpaces);
        var scheme = match.Groups["scheme"].Value
            .FallbackIfNullOrEmpty("http://")
            .ToLowerInvariant();
        var domain = match.Groups["domain"].Value
            .FallbackIfNullOrEmpty(match.Groups["domainWithoutScheme"].Value)
            .ToLowerInvariant();
        var rest = match.Groups["rest"].Value;
        var sanitizedUrl = $"{scheme}{domain}{rest}";

        return sanitizedUrl;
    }

    private IEnumerable<HtmlNode> GetTrackableLinkNodes()
        => _contentNode
            .GetLinkNodes()
            .Where(x => !string.IsNullOrWhiteSpace(x.Attributes["href"]?.Value))
            .Where(x => !x.Attributes.Contains("socialshare"))
            .Where(x => TrackableUrlAcceptanceRegex.IsMatch(x.Attributes["href"].Value));
}
