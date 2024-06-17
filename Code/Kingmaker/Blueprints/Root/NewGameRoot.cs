using System;
using System.Collections.Generic;
using Kingmaker.DLC;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class NewGameRoot
{
	[Serializable]
	public class StoryEntity
	{
		[SerializeField]
		private BlueprintDlcRewardCampaignReference m_DlcReward;

		public LocalizedString Title;

		public LocalizedString Description;

		public Sprite KeyArt;

		public bool ComingSoon;

		public BlueprintDlcRewardCampaign DlcReward => m_DlcReward;
	}

	public List<StoryEntity> StoryList;
}
