using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.DLC;
using Owlcat.Runtime.UI.SelectionGroup;

namespace Kingmaker.Code.UI.MVVM.VM.NewGame.Story;

public class NewGamePhaseStoryScenarioEntityIntegralDlcVM : SelectionGroupEntityVM
{
	public readonly string Title;

	private readonly BlueprintCampaignReference m_CampaignReference;

	public readonly BlueprintDlc BlueprintDlc;

	public BlueprintCampaign Campaign
	{
		get
		{
			if (!m_CampaignReference.IsEmpty())
			{
				return m_CampaignReference.Get();
			}
			return null;
		}
	}

	public NewGamePhaseStoryScenarioEntityIntegralDlcVM(BlueprintCampaign campaign, BlueprintDlc blueprintDlc)
		: base(allowSwitchOff: false)
	{
		Title = blueprintDlc.GetDlcName();
		m_CampaignReference = campaign.ToReference<BlueprintCampaignReference>();
		BlueprintDlc = blueprintDlc;
	}

	protected override void DisposeImplementation()
	{
	}

	public void SelectMe()
	{
		DoSelectMe();
	}

	protected override void DoSelectMe()
	{
	}
}
