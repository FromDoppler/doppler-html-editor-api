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

    public class HrefValues
    {
        public string href { get; set; }
        public string target { get; set; }
    }

    public class Href
    {
        public string name { get; set; }
        public HrefValues values { get; set; }
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
    }

    public class Meta
    {
        public string htmlID { get; set; }
        public string htmlClassNames { get; set; }
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
