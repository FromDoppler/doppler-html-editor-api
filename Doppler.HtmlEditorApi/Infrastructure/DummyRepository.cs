using System;
using Doppler.HtmlEditorApi.Model;
using System.Threading.Tasks;
using System.Text.Json;

namespace Doppler.HtmlEditorApi.Infrastructure
{
    public class DummyRepository : IRepository
    {
        public async Task<string> GetCampaignModel(string accountName, int campaignId)
        {
            ContentModel campaign = new()
            {
                counters = new()
                {
                    u_row = 1,
                    u_column = 2,
                    u_content_text = 1,
                    u_content_heading = 1,
                    u_content_menu = 1
                },
                body = new()
                {
                    rows = Array.Empty<string>(),
                    values = new()
                    {
                        textColor = "#000000",
                        backgroundColor = "#e7e7e7",
                        backgroundImage = new()
                        {
                            url = "",
                            fullWidth = true,
                            repeat = false,
                            center = true,
                            cover = false
                        },
                        contentWidth = "500px",
                        contentAlign = "center",
                        fontFamily = new()
                        {
                            label = "Arial",
                            value = "arial,helvetica,sans-serif"
                        },
                        preheaderText = "",
                        linkStyle = new()
                        {
                            body = true,
                            linkColor = "#0000ee",
                            linkHoverColor = "#0000ee",
                            linkUnderline = true,
                            linkHoverUnderline = true
                        },
                        _meta = new()
                        {
                            htmlID = "u_body",
                            htmlClassNames = "u_body"
                        }
                    }
                },
                schemaVersion = 6
            };

            return JsonSerializer.Serialize(campaign); ;
        }

        public async Task<TemplateModel> GetTemplateModel(string accountName, int templateId)
        {
            TemplateModel template = new()
            {
                name = "Template Name",
                counters = new()
                {
                    u_row = 1,
                    u_column = 2,
                    u_content_text = 1,
                    u_content_heading = 1,
                    u_content_menu = 1
                },
                body = new()
                {
                    rows = Array.Empty<string>(),
                    values = new()
                    {
                        textColor = "#000000",
                        backgroundColor = "#e7e7e7",
                        backgroundImage = new()
                        {
                            url = "",
                            fullWidth = true,
                            repeat = false,
                            center = true,
                            cover = false
                        },
                        contentWidth = "500px",
                        contentAlign = "center",
                        fontFamily = new()
                        {
                            label = "Arial",
                            value = "arial,helvetica,sans-serif"
                        },
                        preheaderText = "",
                        linkStyle = new()
                        {
                            body = true,
                            linkColor = "#0000ee",
                            linkHoverColor = "#0000ee",
                            linkUnderline = true,
                            linkHoverUnderline = true
                        },
                        _meta = new()
                        {
                            htmlID = "u_body",
                            htmlClassNames = "u_body"
                        }
                    }
                },
                schemaVersion = 6
            };

            return template;
        }

        public async Task<TemplateModel> GetSharedTemplateModel(int templateId)
        {
            TemplateModel sharedTemplate = new()
            {
                name = "Shared Template Name",
                counters = new()
                {
                    u_row = 1,
                    u_column = 2,
                    u_content_text = 1,
                    u_content_heading = 1,
                    u_content_menu = 1
                },
                body = new()
                {
                    rows = Array.Empty<string>(),
                    values = new()
                    {
                        textColor = "#000000",
                        backgroundColor = "#e7e7e7",
                        backgroundImage = new()
                        {
                            url = "",
                            fullWidth = true,
                            repeat = false,
                            center = true,
                            cover = false
                        },
                        contentWidth = "500px",
                        contentAlign = "center",
                        fontFamily = new()
                        {
                            label = "Arial",
                            value = "arial,helvetica,sans-serif"
                        },
                        preheaderText = "",
                        linkStyle = new()
                        {
                            body = true,
                            linkColor = "#0000ee",
                            linkHoverColor = "#0000ee",
                            linkUnderline = true,
                            linkHoverUnderline = true
                        },
                        _meta = new()
                        {
                            htmlID = "u_body",
                            htmlClassNames = "u_body"
                        }
                    }
                },
                schemaVersion = 6
            };

            return sharedTemplate;
        }

        public Task SaveCampaignContent(string accountName, int campaignId, ContentModel campaignModel)
        {
            return Task.CompletedTask;
        }

        public Task SaveTemplateContent(string accountName, int templateId, TemplateModel templateModel)
        {
            return Task.CompletedTask;
        }

        public Task CreateTemplate(string accountName, TemplateModel templateModel)
        {
            return Task.CompletedTask;
        }
    }
}
