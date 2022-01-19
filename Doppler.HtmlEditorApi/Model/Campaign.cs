using System.Collections.Generic;
using System.Text.Json;

namespace Doppler.HtmlEditorApi.Model
{
    public class Counters
    {
        public int u_row { get; set; }
        public int u_column { get; set; }
        public int u_content_text { get; set; }
        public int u_content_button { get; set; }
        public int u_content_menu { get; set; }
        public int u_content_heading { get; set; }
        public int u_content_divider { get; set; }
        public int u_content_image { get; set; }
        public int u_content_html { get; set; }
        public int u_content_social { get; set; }
        public int u_content_video { get; set; }
    }

    public class ContentValues
    {
        public Href href { get; set; }
        public string target { get; set; }
        public string containerPadding { get; set; }
        public ButtonColors buttonColors { get; set; }
        public Size size { get; set; }
        public string textAlign { get; set; }
        public string lineHeight { get; set; }
        public string padding { get; set; }
        public Border border { get; set; }
        public string borderRadius { get; set; }
        public bool hideDesktop { get; set; }
        public Meta _meta { get; set; }
        public bool selectable { get; set; }
        public bool draggable { get; set; }
        public bool duplicatable { get; set; }
        public bool deletable { get; set; }
        public bool hideable { get; set; }
        public string text { get; set; }
        public string backgroundColor { get; set; }
        public object displayCondition { get; set; }
        public bool columns { get; set; }
        public string columnsBackgroundColor { get; set; }
        public BackgroundImage backgroundImage { get; set; }
        public string textColor { get; set; }
        public string contentWidth { get; set; }
        public string contentAlign { get; set; }
        public FontFamily fontFamily { get; set; }
        public string preheaderText { get; set; }
        public LinkStyle linkStyle { get; set; }
        public int calculatedWidth { get; set; }
        public int calculatedHeight { get; set; }
        public string width { get; set; }
        public string headingType { get; set; }
        public string fontSize { get; set; }
        public string html { get; set; }
        public SrcImage src { get; set; }
        public string altText { get; set; }
        public Action action { get; set; }
        public Menu menu { get; set; }
        public string align { get; set; }
        public string layout { get; set; }
        public string separator { get; set; }
        public IconsSocial icons { get; set; }
        public Editor editor { get; set; }
        public int spacing { get; set; }
        public Video video { get; set; }

    }

    public class Video
    {
        public string url { get; set; }
        public string thumbnail { get; set; }
        public string videoId { get; set; }
        public string type { get; set; }
        public string playIconType { get; set; }
        public string playIconSize { get; set; }
        public string playIconColor { get; set; }
    }

    public class dataEditor
    {
        public bool showDefaultIcons { get; set; }
        public bool showDefaultOptions { get; set; }
        public List<string> customIcons { get; set; }
        public List<string> customOptions { get; set; }
    }

    public class Editor
    {
        public dataEditor data { get; set; }
    }

    public class Icons
    {
        public string name { get; set; }
        public string url { get; set; }
    }
    public class IconsSocial
    {
        public string iconType { get; set; }
        public List<Icons> icons { get; set; }
    }

    public class RowValues
    {
        public object displayCondition { get; set; }
        public bool columns { get; set; }
        public string backgroundColor { get; set; }
        public string columnsBackgroundColor { get; set; }
        public BackgroundImage backgroundImage { get; set; }
        public string padding { get; set; }
        public bool hideDesktop { get; set; }
        public Meta _meta { get; set; }
        public bool selectable { get; set; }
        public bool draggable { get; set; }
        public bool duplicatable { get; set; }
        public bool deletable { get; set; }
        public bool hideable { get; set; }
    }

    public class ColumnValues
    {
        public string backgroundColor { get; set; }
        public string padding { get; set; }
        public Border border { get; set; }
        public string borderRadius { get; set; }
        public Meta _meta { get; set; }
    }

    public class ActionValues
    {
        public string href { get; set; }
        public string target { get; set; }
        public string email { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public string phone { get; set; }
    }

    public class HrefTargetValues
    {
        public string href { get; set; }
        public string target { get; set; }
    }

    public class Href
    {
        public string name { get; set; }
        public ActionValues values { get; set; }
        public HrefTargetValues attrs { get; set; }
    }

    public class ButtonColors
    {
        public string color { get; set; }
        public string backgroundColor { get; set; }
        public string hoverColor { get; set; }
        public string hoverBackgroundColor { get; set; }
    }

    public class Size
    {
        public bool autoWidth { get; set; }
        public string width { get; set; }
    }

    public class Border
    {
        public string borderTopWidth { get; set; }
        public string borderTopStyle { get; set; }
        public string borderTopColor { get; set; }
    }

    public class Content
    {
        public string type { get; set; }
        public ContentValues values { get; set; }
    }

    public class Column
    {
        public List<Content> contents { get; set; }
        public ColumnValues values { get; set; }
    }

    public class BackgroundImage
    {
        public string url { get; set; }
        public bool fullWidth { get; set; }
        public bool repeat { get; set; }
        public bool center { get; set; }
        public bool cover { get; set; }
    }

    public class Row
    {
        public List<int> cells { get; set; }
        public List<Column> columns { get; set; }
        public RowValues values { get; set; }
    }

    public class FontFamily
    {
        public string label { get; set; }
        public string value { get; set; }
    }

    public class LinkStyle
    {
        public bool body { get; set; }
        public string linkColor { get; set; }
        public string linkHoverColor { get; set; }
        public bool linkUnderline { get; set; }
        public bool linkHoverUnderline { get; set; }
        public bool inherit { get; set; }
    }

    public class SrcImage
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public bool autoWidth { get; set; }
        public string maxWidth { get; set; }
    }

    public class Action
    {
        public string name { get; set; }
        public ActionValues values { get; set; }
        public HrefTargetValues attrs { get; set; }
    }

    public class Meta
    {
        public string htmlID { get; set; }
        public string htmlClassNames { get; set; }
    }

    public class ItemsMenu
    {
        public string key { get; set; }
        public string text { get; set; }
        public Action link { get; set; }
    }

    public class Menu
    {
        public List<ItemsMenu> items { get; set; }
    }

    public class BodyValues
    {
        public string textColor { get; set; }
        public string backgroundColor { get; set; }
        public BackgroundImage backgroundImage { get; set; }
        public string contentWidth { get; set; }
        public string contentAlign { get; set; }
        public FontFamily fontFamily { get; set; }
        public string preheaderText { get; set; }
        public LinkStyle linkStyle { get; set; }
        public Meta _meta { get; set; }
    }

    public class Body
    {
        public List<Row> rows { get; set; }
        public BodyValues values { get; set; }
    }

    public class ContentModel
    {
        public Counters counters { get; set; }
        public Body body { get; set; }
        public int schemaVersion { get; set; }
    }

    public class TemplateModel : ContentModel
    {
        public string name { get; set; }
    }

}
