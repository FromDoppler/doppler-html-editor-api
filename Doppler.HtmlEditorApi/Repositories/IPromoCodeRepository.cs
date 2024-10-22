using System.Threading.Tasks;
using Doppler.HtmlEditorApi.Domain;

namespace Doppler.HtmlEditorApi.Repositories;

public interface IPromoCodeRepository
{
    Task<int> CreatePromoCode(PromoCodeModel promoCodeModel);
    Task UpdatePromoCode(PromoCodeModel promoCodeModel);
}

