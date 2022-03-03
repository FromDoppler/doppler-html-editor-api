using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Doppler.HtmlEditorApi;

public struct PlaceholderMatch
{
    private Match _match;
    public PlaceholderMatch(Match match)
    {
        _match = match;
    }

    public string Outher => _match.Value;
    public string Inner => _match.Groups[1].Value;
}

/// <summary>
/// This class deals with the conversion between a standard HTML content
/// and the set of strings that represents the content in Doppler DB.
/// It is named Doppler because it should replicate current Doppler logic.
/// </summary>
public class DopplerHtmlDocument
{
    public const string FIELD_START_DELIMITER = "[[[";
    public const string FIELD_END_DELIMITER = "]]]";
    public const string FIELD_START_DELIMITER_BACK_END = "|*|";
    public const string FIELD_END_DELIMITER_BACK_END = "*|*";
    public static readonly Regex BACKEND_FIELD_REGEX = new Regex($@"{Regex.Escape(FIELD_START_DELIMITER_BACK_END)}(\d+){Regex.Escape(FIELD_END_DELIMITER_BACK_END)}");

    // % is here to accept %20
    public static readonly Regex FIELD_REGEX = new Regex($@"{Regex.Escape(FIELD_START_DELIMITER)}([a-zA-Z0-9 \-_ñÑáéíóúÁÉÍÓÚ%]+){Regex.Escape(FIELD_END_DELIMITER)}");

    // Old Doppler code:
    // https://github.com/MakingSense/Doppler/blob/ed24e901c990b7fb2eaeaed557c62c1adfa80215/Doppler.HypermediaAPI/ApiMappers/ToDoppler/CampaignContent_To_DtoContent.cs#L27-L29
    private readonly HtmlNode _headNode;
    private readonly HtmlNode _contentNode;

    public DopplerHtmlDocument(string inputHtml)
    {
        var htmlDocument = LoadHtml(inputHtml);

        _headNode = htmlDocument.DocumentNode.SelectSingleNode("//head");

        _contentNode = _headNode == null ? htmlDocument.DocumentNode
            : htmlDocument.DocumentNode.SelectSingleNode("//body")
            ?? LoadHtml(inputHtml.Replace(_headNode.OuterHtml, "")).DocumentNode;
    }

    public string GetDopplerContent()
        => EnsureContent(_contentNode.InnerHtml);

    public string GetHeadContent()
        => _headNode?.InnerHtml;

    public void ReplaceFieldNamesToFieldIds(Func<string, int?> getFieldIdOrNull)
    {
        // TODO: optimize it to do many replacements while traversing the HTML document
        _contentNode.InnerHtml = FIELD_REGEX.Replace(
            _contentNode.InnerHtml,
            // TODO: take into account %20 and that kind of things
            match =>
            {
                var fieldName = match.Groups[1].Value;
                var fieldId = getFieldIdOrNull(fieldName);
                return fieldId.HasValue
                    ? CreateFieldIdPlaceholder(fieldId.GetValueOrDefault())
                    : match.Value;
            });
    }

    public void ClearInexistentFieldIds(Func<int, bool> fieldIdExist)
    {
        // TODO: optimize it to do many replacements while traversing the HTML document
        _contentNode.InnerHtml = BACKEND_FIELD_REGEX.Replace(
            _contentNode.InnerHtml,
            match => fieldIdExist(int.Parse(match.Groups[1].ValueSpan))
                ? match.Value
                : string.Empty);
    }

    private static string CreateFieldIdPlaceholder(int? fieldId)
        => $"{FIELD_START_DELIMITER_BACK_END}{fieldId}{FIELD_END_DELIMITER_BACK_END}";

    private static string EnsureContent(string htmlContent)
        => string.IsNullOrWhiteSpace(htmlContent) ? "<BR>" : htmlContent;

    private static HtmlDocument LoadHtml(string inputHtml)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(inputHtml);
        return htmlDocument;
    }
}
