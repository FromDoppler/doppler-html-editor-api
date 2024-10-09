using Doppler.HtmlEditorApi.Domain;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Repositories;

public interface IPromoCodeRepository
{
    Task<int> CreatePromoCode(PromoCodeModel promoCodeModel);
    Task UpdatePromoCode(PromoCodeModel promoCodeModel);
}

