using System;
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
        private const string EmptyUnlayerContentJson = "{\"body\":{\"rows\":[]}}";
        private const string EmptyUnlayerContentHtml = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional //EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\"><head> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"> <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"> <meta name=\"x-apple-disable-message-reformatting\"> <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"> <title></title></head><body></body></html>";
        private readonly ICampaignContentRepository _campaignContentRepository;
        private readonly IFieldsRepository _fieldsRepository;
        private readonly IOptions<FieldsOptions> _fieldsOptions;

        public CampaignsController(ICampaignContentRepository repository, IFieldsRepository fieldsRepository, IOptions<FieldsOptions> fieldsOptions)
        {
            _campaignContentRepository = repository;
            _fieldsRepository = fieldsRepository;
            _fieldsOptions = fieldsOptions;
        }

        [Authorize(Policies.OwnResourceOrSuperUser)]
        [HttpGet("/accounts/{accountName}/campaigns/{campaignId}/content")]
        public async Task<ActionResult<CampaignContent>> GetCampaign(string accountName, int campaignId)
        {
            // TODO: Considere refactoring accountName validation
            var contentRow = await _campaignContentRepository.GetCampaignModel(accountName, campaignId);

            ActionResult<CampaignContent> result = contentRow switch
            {
                null => new NotFoundObjectResult("Campaign not found or belongs to a different account"),
                EmptyContentData => new CampaignContent(
                    type: ContentType.unlayer,
                    meta: Utils.ParseAsJsonElement(EmptyUnlayerContentJson),
                    htmlContent: EmptyUnlayerContentHtml),
                UnlayerContentData unlayerContent => new CampaignContent(
                    type: ContentType.unlayer,
                    meta: Utils.ParseAsJsonElement(unlayerContent.Meta),
                    htmlContent: GenerateHtmlContent(unlayerContent)),
                BaseHtmlContentData htmlContent => new CampaignContent(
                    type: ContentType.html,
                    meta: null,
                    htmlContent: GenerateHtmlContent(htmlContent)),
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
        public async Task<IActionResult> SaveCampaign(string accountName, int campaignId, CampaignContent campaignContent)
        {
            var campaignState = await _campaignContentRepository.GetCampaignState(accountName, campaignId);
            if (!campaignState.OwnCampaignExists)
            {
                return new NotFoundObjectResult(new ProblemDetails()
                {
                    Title = $@"The campaign was no found",
                    Detail = $@"The campaign with id {campaignId} does not exists or belongs to another user than {accountName}"
                });
            }

            if (!campaignState.IsWritable)
            {
                return new BadRequestObjectResult(new ProblemDetails()
                {
                    Title = "The campaign content is read only",
                    Detail = $@"The content cannot be edited because status campaign is {campaignState.CampaignStatus}"
                });
            }
            var fieldAliases = _fieldsOptions.Value.Aliases;

            var basicFields = await _fieldsRepository.GetActiveBasicFields();
            var customFields = await _fieldsRepository.GetCustomFields(accountName);
            var fields = basicFields.Union(customFields);

            var dopplerFieldsProcessor = new DopplerFieldsProcessor(fields, fieldAliases);

            var htmlDocument = new DopplerHtmlDocument(campaignContent.htmlContent);
            htmlDocument.RemoveHarmfulTags();
            htmlDocument.RemoveEventAttributes();
            htmlDocument.ReplaceFieldNameTagsByFieldIdTags(dopplerFieldsProcessor.GetFieldIdOrNull);
            htmlDocument.RemoveUnknownFieldIdTags(dopplerFieldsProcessor.FieldIdExist);
            htmlDocument.SanitizeTrackableLinks();

            var head = htmlDocument.GetHeadContent();
            var content = htmlDocument.GetDopplerContent();
            var fieldIds = htmlDocument.GetFieldIds();
            var trackableUrls = htmlDocument.GetTrackableUrls();

            BaseHtmlContentData contentRow = campaignContent.type switch
            {
                ContentType.unlayer => new UnlayerContentData(
                    HtmlContent: content,
                    HtmlHead: head,
                    Meta: campaignContent.meta.ToString(),
                    CampaignId: campaignId,
                    PreviewImage: campaignContent.previewImage),
                ContentType.html => new HtmlContentData(
                    HtmlContent: content,
                    HtmlHead: head,
                    CampaignId: campaignId,
                    PreviewImage: campaignContent.previewImage),
                _ => throw new NotImplementedException($"Unsupported campaign content type {campaignContent.type:G}")
            };

            if (campaignState.ContentExists)
            {
                await _campaignContentRepository.UpdateCampaignContent(accountName, contentRow);
            }
            else
            {
                await _campaignContentRepository.CreateCampaignContent(accountName, contentRow);
            }
            await _campaignContentRepository.SaveNewFieldIds(campaignId, fieldIds);
            await _campaignContentRepository.SaveLinks(campaignId, trackableUrls);

            return new OkObjectResult($"La campaña '{campaignId}' del usuario '{accountName}' se guardó exitosamente ");
        }

        private static string GenerateHtmlContent(BaseHtmlContentData content)
            // Notice that it is not symmetric with ExtractDopplerHtmlData.
            // The head is being lossed here. It is not good if we try to edit an imported content.
            // Old Doppler code:
            // https://github.com/MakingSense/Doppler/blob/ed24e901c990b7fb2eaeaed557c62c1adfa80215/Doppler.HypermediaAPI/ApiMappers/FromDoppler/DtoContent_To_CampaignContent.cs#L23
            => content.HtmlContent;
    }
}
