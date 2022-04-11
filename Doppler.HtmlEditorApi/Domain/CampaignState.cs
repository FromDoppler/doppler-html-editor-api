using System.Collections.Generic;

namespace Doppler.HtmlEditorApi.Domain;

public record CampaignState(bool OwnCampaignExists, bool ContentExists, int? EditorType, CampaignStatus? CampaignStatus)
{
    private static readonly HashSet<CampaignStatus> _writableStatus = new HashSet<CampaignStatus>(
        new[]
        {
            Domain.CampaignStatus.DRAFT,
            Domain.CampaignStatus.IN_WINNER_IN_AB_SELECTION_PROCESS
        });
    public bool IsWritable => CampaignStatus.HasValue && _writableStatus.Contains(CampaignStatus.Value);
}
public record NoExistCampaignState() : CampaignState(false, false, null, null);
