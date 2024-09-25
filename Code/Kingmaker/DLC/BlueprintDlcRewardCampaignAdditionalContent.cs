using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("2bc7dae885d94711bdd5c77f38a3c4ca")]
public class BlueprintDlcRewardCampaignAdditionalContent : BlueprintDlcReward
{
	[SerializeField]
	private BlueprintCampaignReference m_Campaign;

	public BlueprintCampaign Campaign => m_Campaign;

	public override void RecheckAvailability()
	{
		base.RecheckAvailability();
		Campaign.RecheckAvailability();
	}
}
