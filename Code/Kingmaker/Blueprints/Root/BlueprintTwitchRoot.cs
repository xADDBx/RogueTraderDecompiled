using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[TypeId("af2ccb13511948f2bf65bc4924c84c10")]
public class BlueprintTwitchRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintTwitchRoot>
	{
	}

	[Serializable]
	public class DropsReward
	{
		[SerializeField]
		private string m_TwitchId;

		[SerializeField]
		private List<DropsRewardItem> m_RewardItems;

		public string TwitchId => m_TwitchId;

		public IReadOnlyList<DropsRewardItem> RewardItems => m_RewardItems;
	}

	[Serializable]
	public class DropsRewardItem
	{
		[SerializeField]
		private BlueprintItemReference m_Item;

		[SerializeField]
		private int m_Count;

		public BlueprintItem Item => m_Item.Get();

		public int Count => m_Count;
	}

	[SerializeField]
	private List<DropsReward> m_DropsRewards;

	[SerializeField]
	private string m_DropsServiceUrl;

	[SerializeField]
	private float m_DropsServiceTimeoutSeconds = 30f;

	public IReadOnlyList<DropsReward> DropsRewards => m_DropsRewards;

	public string DropsServiceUrl => m_DropsServiceUrl;

	public float DropsServiceTimeoutSeconds => m_DropsServiceTimeoutSeconds;
}
