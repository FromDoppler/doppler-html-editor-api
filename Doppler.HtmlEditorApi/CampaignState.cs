using System.Linq;
using System;
using System.Collections.Generic;

namespace Doppler.HtmlEditorApi;

public record CampaignState(bool OwnCampaignExists, bool ContentExists, int? EditorType, CampaignStatus? CampaignStatus)
{
    private static readonly HashSet<CampaignStatus> WritableStatus = new HashSet<CampaignStatus>
    (new[]
        {
            HtmlEditorApi.CampaignStatus.DRAFT,
            HtmlEditorApi.CampaignStatus.IN_WINNER_IN_AB_SELECTION_PROCESS
        });
    public bool IsWritable => CampaignStatus.HasValue && WritableStatus.Contains(CampaignStatus.Value);
}
public record NoExistCampaignState() : CampaignState(false, false, null, null);
