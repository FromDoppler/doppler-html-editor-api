using System;
using Doppler.HtmlEditorApi.Model;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

namespace Doppler.HtmlEditorApi.Infrastructure
{
    public class DummyRepository : IRepository
    {
        public Task<ContentModel> GetCampaignModel(string accountName, int campaignId)
        {
            return Task.FromResult(new ContentModel()
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
                    rows = new List<Row>(),
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
            });
        }

        public Task<TemplateModel> GetTemplateModel(string accountName, int templateId)
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
                    rows = new List<Row>(),
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

            return Task.FromResult(template);
        }

        public Task<TemplateModel> GetSharedTemplateModel(int templateId)
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
                    rows = new List<Row>(),
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

            return Task.FromResult(sharedTemplate);
        }

        public Task SaveCampaignContent(string accountName, int campaignId, CampaignContentRequest campaignModel)
        {
            return null;
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
