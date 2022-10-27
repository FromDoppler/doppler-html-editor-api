using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.ApiModels;
using Doppler.HtmlEditorApi.Configuration;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.DopplerSecurity;
using Doppler.HtmlEditorApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Doppler.HtmlEditorApi.Controllers
{
    [Authorize]
    [ApiController]
    public class CampaignsController
    {
        private const string EmptyUnlayerContentJson = @"{
    ""body"": {
        ""rows"": [
            {
                ""cells"": [1],
                ""columns"": [
                    {
                        ""contents"": []
                    }
                ]
            }
        ],
        ""values"": {
            ""contentWidth"": ""600px""
        }
    }
}";
        private const string EmptyUnlayerContentHtml = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional //EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\"><head> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"> <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"> <meta name=\"x-apple-disable-message-reformatting\"> <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"> <title></title></head><body></body></html>";
        private readonly ICampaignContentRepository _campaignContentRepository;
        private readonly IFieldsRepository _fieldsRepository;
        private readonly IOptions<FieldsOptions> _fieldsOptions;
        private readonly ITemplateRepository _templateRepository;

        // We are using HTMLSourceType = 2 (Template) because Editor seems to be tied to the old HTML Editor
        // See https://github.com/MakingSense/Doppler/blob/48cf637bb1f8b4d81837fff904d8736fe889ff1c/Doppler.Transversal/Classes/CampaignHTMLContentTypeEnum.cs#L12-L17
        private const int TemplateHtmlSourceType = 2;

        public CampaignsController(ICampaignContentRepository repository, IFieldsRepository fieldsRepository, IOptions<FieldsOptions> fieldsOptions, ITemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
            _campaignContentRepository = repository;
            _fieldsRepository = fieldsRepository;
            _fieldsOptions = fieldsOptions;
        }

        [Authorize(Policies.OwnResourceOrSuperUser)]
        [HttpGet("/accounts/{accountName}/campaigns/{campaignId}/content")]
        public async Task<ActionResult<CampaignContent>> GetCampaignContent(string accountName, int campaignId)
        {
            // TODO: Considere refactoring accountName validation
            var contentRow = await _campaignContentRepository.GetCampaignModel(accountName, campaignId);

            ActionResult<CampaignContent> result = contentRow switch
            {
                null => new NotFoundObjectResult("Campaign not found or belongs to a different account"),
                EmptyCampaignContentData => new CampaignContent(
                    type: ContentType.unlayer,
                    meta: Utils.ParseAsJsonElement(EmptyUnlayerContentJson),
                    htmlContent: EmptyUnlayerContentHtml,
                    previewImage: null,
                    campaignName: null), // TODO: Take the name from campaign information
                UnlayerCampaignContentData unlayerContent => new CampaignContent(
                    type: ContentType.unlayer,
                    meta: Utils.ParseAsJsonElement(unlayerContent.Meta),
                    htmlContent: GenerateHtmlContent(unlayerContent),
                    previewImage: unlayerContent.PreviewImage,
                    campaignName: unlayerContent.CampaignName),
                BaseHtmlCampaignContentData htmlContent => new CampaignContent(
                    type: ContentType.html,
                    meta: null,
                    htmlContent: GenerateHtmlContent(htmlContent),
                    previewImage: htmlContent.PreviewImage,
                    campaignName: htmlContent.CampaignName),
                _ => throw new NotImplementedException($"Unsupported campaign content type {contentRow.GetType()}")
            };

            return result;
        }

        [Authorize(Policies.OwnResourceOrSuperUser)]
        [HttpGet("/accounts/{accountName}/campaigns/{campaignId}/content/thumbnail")]
        public Task<ActionResult> GetCampaignThumbnail(string accountName, int campaignId)
        {
            var uriCampaignThumbnail = "https://via.placeholder.com/200x200";
            return Task.FromResult<ActionResult>(new RedirectResult(uriCampaignThumbnail));
        }

        [Authorize(Policies.OwnResourceOrSuperUser)]
        [HttpPut("/accounts/{accountName}/campaigns/{campaignId}/content")]
        public async Task<IActionResult> SaveCampaignContent(string accountName, int campaignId, CampaignContent campaignContent)
        {
            var campaignState = await _campaignContentRepository.GetCampaignState(accountName, campaignId);
            if (!ValidateCampaignStateToUpdate(campaignState, out var error))
            {
                return error;
            }

            var htmlDocument = await ExtractHtmlDomFromCampaignContent(accountName, campaignContent.htmlContent);
            var head = htmlDocument.GetHeadContent();
            var content = htmlDocument.GetDopplerContent();
            var fieldIds = htmlDocument.GetFieldIds();
            var trackableUrls = htmlDocument.GetTrackableUrls();

            // TODO: Validate if it's possible to delete PreviewImage property from BaseHtmlContentData,
            // because it's already in campaignContent
            // See it on: https://github.com/FromDoppler/doppler-html-editor-api/pull/111#discussion_r870681998
            BaseHtmlCampaignContentData baseHtmlContent = campaignContent.type switch
            {
                ContentType.unlayer => new UnlayerCampaignContentData(
                    HtmlContent: content,
                    HtmlHead: head,
                    Meta: campaignContent.meta.ToString(),
                    PreviewImage: campaignContent.previewImage,
                    CampaignName: campaignContent.campaignName,
                    IdTemplate: null),
                ContentType.html => new HtmlCampaignContentData(
                    HtmlContent: content,
                    HtmlHead: head,
                    PreviewImage: campaignContent.previewImage,
                    CampaignName: campaignContent.campaignName,
                    IdTemplate: null),
                _ => throw new NotImplementedException($"Unsupported campaign content type {campaignContent.type:G}")
            };

            await SaveCampaignContent(baseHtmlContent, fieldIds, trackableUrls, campaignState);

            return new OkObjectResult($"La campaña '{campaignId}' del usuario '{accountName}' se guardó exitosamente ");
        }

        [Authorize(Policies.OwnResourceOrSuperUser)]
        [HttpPost("/accounts/{accountName}/campaigns/{campaignId}/content/from-template/{templateId}")]
        public async Task<IActionResult> CreateCampaignContentFromTemplate(string accountName, int campaignId, int templateId)
        {
            var campaignState = await _campaignContentRepository.GetCampaignState(accountName, campaignId);
            if (!ValidateCampaignStateToUpdate(campaignState, out var error))
            {
                return error;
            }

            var templateData = await _templateRepository.GetTemplate(accountName, templateId);
            if (templateData == null)
            {
                return new NotFoundObjectResult(new ProblemDetails()
                {
                    Title = $@"The template was no found",
                    Detail = $@"The template not exists or Inactive"
                });
            }
            if (templateData is not UnlayerTemplateContentData unlayerTemplateData)
            {
                return new BadRequestObjectResult(new ProblemDetails()
                {
                    Title = $@"The template cannot open",
                    Detail = $@"The template exist but is not unlayer template"
                });
            }

            var htmlDocument = await ExtractHtmlDomFromCampaignContent(accountName, unlayerTemplateData.HtmlComplete);
            var head = htmlDocument.GetHeadContent();
            var content = htmlDocument.GetDopplerContent();
            var fieldIds = htmlDocument.GetFieldIds();
            var trackableUrls = htmlDocument.GetTrackableUrls();

            // TODO: Validate if it's possible to delete PreviewImage property from BaseHtmlContentData,
            // because it's already in campaignContent
            // See it on: https://github.com/FromDoppler/doppler-html-editor-api/pull/111#discussion_r870681998
            BaseHtmlCampaignContentData baseHtmlContent = new UnlayerCampaignContentData(
                    HtmlContent: content,
                    HtmlHead: head,
                    Meta: unlayerTemplateData.Meta,
                    PreviewImage: unlayerTemplateData.PreviewImage,
                    CampaignName: unlayerTemplateData.Name,
                    IdTemplate: templateId);

            // TODO: Save templateId reference with the content
            await SaveCampaignContent(baseHtmlContent, fieldIds, trackableUrls, campaignState);

            return new OkObjectResult($"La campaña '{campaignId}' del usuario '{accountName}' se guardó exitosamente ");
        }

        private async Task SaveCampaignContent(BaseHtmlCampaignContentData content, IEnumerable<int> fieldIds, IEnumerable<string> trackableUrls, CampaignState campaignState)
        {

            var campaignIds = new[] { campaignState.IdCampaignA, campaignState.IdCampaignB }
                .Where(x => x != null)
                .Select(x => x.Value);

            foreach (var campaign in campaignIds)
            {
                if (campaignState.ContentExists)
                {
                    await _campaignContentRepository.UpdateCampaignContent(campaign, content);
                }
                else
                {
                    await _campaignContentRepository.CreateCampaignContent(campaign, content);
                    await _campaignContentRepository.UpdateCampaignStatus(
                        setCurrentStep: 2,
                        setHtmlSourceType: TemplateHtmlSourceType,
                        whenIdCampaignIs: campaign,
                        whenCurrentStepIs: 1);
                }
                await _campaignContentRepository.UpdateCampaignPreviewImage(campaign, content.PreviewImage);
                await _campaignContentRepository.SaveNewFieldIds(campaign, fieldIds);
                await _campaignContentRepository.SaveLinks(campaign, trackableUrls);
            }

            if (campaignState.IdCampaignResult != null && !campaignState.ContentExists)
            {
                await _campaignContentRepository.UpdateCampaignStatus(
                    setCurrentStep: 2,
                    setHtmlSourceType: TemplateHtmlSourceType,
                    whenIdCampaignIs: campaignState.IdCampaignResult.Value,
                    whenCurrentStepIs: 1);
            }
        }

        private async Task<DopplerHtmlDocument> ExtractHtmlDomFromCampaignContent(string accountName, string htmlContent)
        {
            var fieldAliases = _fieldsOptions.Value.Aliases;

            var basicFields = await _fieldsRepository.GetActiveBasicFields();
            var customFields = await _fieldsRepository.GetCustomFields(accountName);
            var fields = basicFields.Union(customFields);

            var dopplerFieldsProcessor = new DopplerFieldsProcessor(fields, fieldAliases);

            var htmlDocument = new DopplerHtmlDocument(htmlContent);
            htmlDocument.RemoveHarmfulTags();
            htmlDocument.RemoveEventAttributes();
            htmlDocument.ReplaceFieldNameTagsByFieldIdTags(dopplerFieldsProcessor.GetFieldIdOrNull);
            htmlDocument.RemoveUnknownFieldIdTags(dopplerFieldsProcessor.FieldIdExist);
            htmlDocument.SanitizeTrackableLinks();
            return htmlDocument;
        }

        private static string GenerateHtmlContent(BaseHtmlCampaignContentData content)
            // Notice that it is not symmetric with ExtractDopplerHtmlData.
            // The head is being lossed here. It is not good if we try to edit an imported content.
            // Old Doppler code:
            // https://github.com/MakingSense/Doppler/blob/ed24e901c990b7fb2eaeaed557c62c1adfa80215/Doppler.HypermediaAPI/ApiMappers/FromDoppler/DtoContent_To_CampaignContent.cs#L23
            => content.HtmlContent;

        private static bool ValidateCampaignStateToUpdate(CampaignState campaignState, out ObjectResult error)
        {
            error = !campaignState.OwnCampaignExists ? new NotFoundObjectResult(
                    new ProblemDetails()
                    {
                        Title = $@"Not found campaign",
                        Detail = $@"The campaign does not exists or belongs to another user"
                    })
                : !campaignState.IsWritable ? new BadRequestObjectResult(
                    new ProblemDetails()
                    {
                        Title = "The campaign content is read only",
                        Detail = $@"The content cannot be edited because status campaign is {campaignState.CampaignStatus}"
                    })
                : campaignState.TestABCondition == TestABCondition.TypeTestABContent ? new BadRequestObjectResult(
                    new ProblemDetails()
                    {
                        Title = "The campaign is AB Test by content",
                        Detail = $@"The type of campaign is AB Test by content and it's unsupported"
                    }) : null;
            return error == null;
        }
    }
}
