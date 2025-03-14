using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb;

public class DopplerPromoCodeRepository : IPromoCodeRepository
{
    private readonly IDbContext _dbContext;
    public DopplerPromoCodeRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CreatePromoCode(PromoCodeModel promoCodeModel)
    {
        var insertPromoCodeDbQuery = new InsertPromoCodeDbQuery(
            Type: promoCodeModel.Type,
            Value: promoCodeModel.Value,
            IncludeShipping: promoCodeModel.IncludeShipping,
            FirstPurchase: promoCodeModel.FirstPurchase,
            AllowCombines: promoCodeModel.CombineDiscounts,
            MinPrice: promoCodeModel.MinPrice,
            ExpireDays: promoCodeModel.ExpireDays,
            MaxUses: promoCodeModel.MaxUses,
            Categories: promoCodeModel.Categories,
            IdCampaign: promoCodeModel.CampaignId,
            Prefix: promoCodeModel.Prefix
        );

        var result = await _dbContext.ExecuteAsync(insertPromoCodeDbQuery);

        return result.IdDynamicContentPromoCode;
    }

    public async Task<bool> UpdatePromoCode(PromoCodeModel promoCodeModel)
    {
        var updatePromoCodeDbQuery = new UpdatePromoCodeDbQuery(
            Id: promoCodeModel.Id,
            IdCampaign: promoCodeModel.CampaignId,
            Type: promoCodeModel.Type,
            Value: promoCodeModel.Value,
            IncludeShipping: promoCodeModel.IncludeShipping,
            FirstPurchase: promoCodeModel.FirstPurchase,
            AllowCombines: promoCodeModel.CombineDiscounts,
            MinPrice: promoCodeModel.MinPrice,
            ExpireDays: promoCodeModel.ExpireDays,
            MaxUses: promoCodeModel.MaxUses,
            Categories: promoCodeModel.Categories,
            Prefix: promoCodeModel.Prefix
        );

        var result = await _dbContext.ExecuteAsync(updatePromoCodeDbQuery);
        return result > 0;
    }
}

