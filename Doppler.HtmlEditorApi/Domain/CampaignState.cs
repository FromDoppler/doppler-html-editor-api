using System.Collections.Generic;

namespace Doppler.HtmlEditorApi.Domain;

public record CampaignState(bool OwnCampaignExists, bool ContentExists, int? EditorType, CampaignStatus? CampaignStatus)
{
    private static readonly HashSet<CampaignStatus> WritableStatus = new HashSet<CampaignStatus>(
        new[]
        {
            Domain.CampaignStatus.Draft,
            Domain.CampaignStatus.InWinnerInABSelectionProcess
        });
    public bool IsWritable => CampaignStatus.HasValue && WritableStatus.Contains(CampaignStatus.Value);
}
public record NoExistCampaignState() : CampaignState(false, false, null, null);
