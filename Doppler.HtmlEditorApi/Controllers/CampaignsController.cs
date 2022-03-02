using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DopplerSecurity;
using Microsoft.AspNetCore.Authorization;
using Doppler.HtmlEditorApi.ApiModels;
using Doppler.HtmlEditorApi.Storage;
using System.Text.Json;

namespace Doppler.HtmlEditorApi.Controllers
{
    [Authorize]
    [ApiController]
    public class CampaignsController
    {
        private const string EMPTY_UNLAYER_CONTENT_JSON = "{\"body\":{\"rows\":[]}}";
        private const string EMPTY_UNLAYER_CONTENT_HTML = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional //EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\"><head> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"> <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"> <meta name=\"x-apple-disable-message-reformatting\"> <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"> <title></title></head><body></body></html>";
        private readonly IRepository _repository;
        private readonly IFieldsRepository _fieldsRepository;

        public CampaignsController(IRepository Repository, IFieldsRepository fieldsRepository)
        {
            _repository = Repository;
            _fieldsRepository = fieldsRepository;
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/campaigns/{campaignId}/content")]
        public async Task<ActionResult<CampaignContent>> GetCampaign(string accountName, int campaignId)
        {
            // TODO: Considere refactoring accountName validation
            var contentRow = await _repository.GetCampaignModel(accountName, campaignId);

            ActionResult<CampaignContent> result = contentRow switch
            {
                null => new NotFoundObjectResult("Campaign not found or belongs to a different account"),
                EmptyContentData emptyContentData => new CampaignContent(
                    type: ContentType.unlayer,
                    meta: Utils.ParseAsJsonElement(EMPTY_UNLAYER_CONTENT_JSON),
                    htmlContent: EMPTY_UNLAYER_CONTENT_HTML),
                UnlayerContentData unlayerContent => new CampaignContent(
                    type: ContentType.unlayer,
                    meta: Utils.ParseAsJsonElement(unlayerContent.meta),
                    htmlContent: GenerateHtmlContent(unlayerContent)),
                BaseHtmlContentData htmlContent => new CampaignContent(
                    type: ContentType.html,
                    meta: null,
                    htmlContent: GenerateHtmlContent(htmlContent)),
                _ => throw new NotImplementedException($"Unsupported campaign content type {contentRow.GetType()}")
            };

            return result;
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpGet("/accounts/{accountName}/campaigns/{campaignId}/content/thumbnail")]
        public Task<ActionResult> GetCampaignThumbnail(string accountName, int campaignId)
        {
            var uriCampaignThumbnail = "https://via.placeholder.com/200x200";
            return Task.FromResult<ActionResult>(new RedirectResult(uriCampaignThumbnail));
        }

        [Authorize(Policies.OWN_RESOURCE_OR_SUPERUSER)]
        [HttpPut("/accounts/{accountName}/campaigns/{campaignId}/content")]
        public async Task<IActionResult> SaveCampaign(string accountName, int campaignId, CampaignContent campaignContent)
        {
            var basicFields = await _fieldsRepository.GetActiveBasicFields();
            // var customFields = await _fieldsRepository.GetCustomFields(accountName);
            // var fields = basicFields.Union(customFields);
            // TODO: include custom fields also
            var fields = basicFields;

            // TODO: get this information from the configuration
            var fieldAliases = new[]
            {
                new FieldAliasesDef("BIRTHDAY", new[] { "CUMPLEANOS", "CUMPLEAÑOS", "DATE OF BIRTH", "DOB", "FECHA DE NACIMIENTO", "NACIMIENTO" }),
                new FieldAliasesDef("COUNTRY", new[] { "PAIS", "PAÍS" }),
                new FieldAliasesDef("EMAIL", new[] { "CORREO", "CORREO ELECTRONICO", "CORREO ELECTRÓNICO", "CORREO_ELECTRONICO", "CORREO_ELECTRÓNICO", "E-MAIL", "MAIL" }),
                new FieldAliasesDef("FIRST_NAME", new[] { "FIRST NAME", "FIRST-NAME", "FIRSTNAME", "NAME", "NOMBRE" }),
                new FieldAliasesDef("GENDER", new[] { "GENERO", "GÉNERO", "SEXO" }),
                new FieldAliasesDef("LAST_NAME", new[] { "LAST NAME", "LAST-NAME", "LASTNAME", "SURNAME", "APELLIDO" }),
            };

            var dopplerFieldsProcessor = new DopplerFieldsProcessor(fields, fieldAliases);

            var htmlDocument = new DopplerHtmlDocument(campaignContent.htmlContent);
            htmlDocument.TraverseAndReplace(dopplerFieldsProcessor.ReplaceFieldNamesToFieldIdsInHtmlContent);
            htmlDocument.TraverseAndReplace(dopplerFieldsProcessor.ClearInexistentFieldIds);

            var head = htmlDocument.GetHeadContent();
            var content = htmlDocument.GetDopplerContent();

            BaseHtmlContentData contentRow = campaignContent.type switch
            {
                ContentType.unlayer => new UnlayerContentData(
                    htmlContent: content,
                    htmlHead: head,
                    meta: campaignContent.meta.ToString(),
                    campaignId: campaignId),
                ContentType.html => new HtmlContentData(
                    htmlContent: content,
                    htmlHead: head,
                    campaignId: campaignId),
                _ => throw new NotImplementedException($"Unsupported campaign content type {campaignContent.type:G}")
            };

            await _repository.SaveCampaignContent(accountName, contentRow);
            return new OkObjectResult($"La campaña '{campaignId}' del usuario '{accountName}' se guardó exitosamente ");
        }

        private string GenerateHtmlContent(BaseHtmlContentData content)
            // Notice that it is not symmetric with ExtractDopplerHtmlData.
            // The head is being lossed here. It is not good if we try to edit an imported content.
            // Old Doppler code:
            // https://github.com/MakingSense/Doppler/blob/ed24e901c990b7fb2eaeaed557c62c1adfa80215/Doppler.HypermediaAPI/ApiMappers/FromDoppler/DtoContent_To_CampaignContent.cs#L23
            => content.htmlContent;
    }
}
