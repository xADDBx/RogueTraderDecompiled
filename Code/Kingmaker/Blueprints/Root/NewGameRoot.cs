using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class NewGameRoot
{
	[SerializeField]
	private BlueprintCampaignReference[] m_StoryCampaigns;

	private BlueprintCampaignReference m_MainCampaignReference;

	public IEnumerable<BlueprintCampaign> StoryCampaigns => m_StoryCampaigns?.Dereference();

	public BlueprintCampaign MainCampaign
	{
		get
		{
			if (m_MainCampaignReference == null || m_MainCampaignReference.IsEmpty())
			{
				m_MainCampaignReference = StoryCampaigns.FirstOrDefault((BlueprintCampaign bp) => bp.IsMainGameContent)?.ToReference<BlueprintCampaignReference>();
			}
			if (m_MainCampaignReference != null && !m_MainCampaignReference.IsEmpty())
			{
				return m_MainCampaignReference?.Get();
			}
			return null;
		}
	}
}
