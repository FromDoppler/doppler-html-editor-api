namespace Doppler.HtmlEditorApi.Model
{
    public class Counters
    {
        public int u_row { get; set; }
        public int u_column { get; set; }
        public int u_content_text { get; set; }
        public int u_content_menu { get; set; }
        public int u_content_heading { get; set; }
    }

    public class BackgroundImage
    {
        public string url { get; set; }
        public bool fullWidth { get; set; }
        public bool repeat { get; set; }
        public bool center { get; set; }
        public bool cover { get; set; }
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

    public class Values
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
        public string[] rows { get; set; }
        public Values values { get; set; }
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
