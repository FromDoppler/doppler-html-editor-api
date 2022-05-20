using System.Collections.Generic;

namespace Doppler.HtmlEditorApi.Domain;

public enum TestABCondition
{
    TypeClassic = 0,
    TypeTestABSubject = 1,
    TypeTestABContent = 2
}

public abstract record CampaignState(
    bool OwnCampaignExists,
    bool ContentExists,
    int? EditorType,
    CampaignStatus? CampaignStatus,
    TestABCondition TestABCondition,
    int? IdCampaignA,
    int? IdCampaignB,
    int? IdCampaignResult)
{
    private static readonly HashSet<CampaignStatus> WritableStatus = new HashSet<CampaignStatus>(
        new[]
        {
            Domain.CampaignStatus.Draft,
            Domain.CampaignStatus.InWinnerInABSelectionProcess
        });
    public bool IsWritable => CampaignStatus.HasValue && WritableStatus.Contains(CampaignStatus.Value);
}
public record NoExistCampaignState() : CampaignState(false, false, null, null, TestABCondition.TypeClassic, null, null, null);
public record ClassicCampaignState(
    int IdCampaign,
    bool ContentExists,
    int? EditorType,
    CampaignStatus?
    CampaignStatus) : CampaignState(true, ContentExists, EditorType, CampaignStatus, TestABCondition.TypeClassic, IdCampaign, null, null);
public record TestABCampaignState(
    bool ContentExists,
    int? EditorType,
    CampaignStatus?
    CampaignStatus,
    TestABCondition TestABCondition,
    int? IdCampaignA,
    int? IdCampaignB,
    int? IdCampaignResult) : CampaignState(true, ContentExists, EditorType, CampaignStatus, TestABCondition, IdCampaignA, IdCampaignB, IdCampaignResult);
