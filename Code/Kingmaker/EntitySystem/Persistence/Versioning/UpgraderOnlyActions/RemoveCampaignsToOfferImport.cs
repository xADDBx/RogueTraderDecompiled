using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UpgraderOnlyActions;

[TypeId("e5cc8997257a7264ea5c181d6ce7377f")]
public class RemoveCampaignsToOfferImport : PlayerUpgraderOnlyAction
{
	[SerializeField]
	private BlueprintCampaignReference m_Campaign;

	protected override void RunActionOverride()
	{
		if (Game.Instance.Player.CampaignsToOfferImport.ContainsKey(m_Campaign))
		{
			Game.Instance.Player.CampaignsToOfferImport.Remove(m_Campaign);
		}
		else if (m_Campaign == null)
		{
			Game.Instance.Player.CampaignsToOfferImport.Clear();
		}
	}

	public override string GetCaption()
	{
		if (m_Campaign != null)
		{
			return "Remove all " + m_Campaign.Get()?.name + " to offer import";
		}
		return "Remove all campaigns to offer import";
	}
}
