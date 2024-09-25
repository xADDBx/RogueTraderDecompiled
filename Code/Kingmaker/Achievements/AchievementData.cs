using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Achievements;

[HashRoot]
[TypeId("e6ab11140a57cf54d92e5809d232b220")]
public class AchievementData : BlueprintScriptableObject, IUnlockableFlagReference
{
	[Serializable]
	private class UnlockableFlagData
	{
		[UsedImplicitly]
		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("Flag")]
		private BlueprintUnlockableFlagReference m_Flag;

		[UsedImplicitly]
		[HideInInspector]
		public int Value;

		public BlueprintUnlockableFlag Flag => m_Flag?.Get();
	}

	[Serializable]
	private class UnlockableFlagsPack
	{
		[UsedImplicitly]
		public UnlockableFlagData[] Flags;
	}

	public string SteamId;

	public string GogId;

	public string EGSId;

	public string PS5TrophyId;

	public string XboxLiveId;

	public AchievementType Type;

	public int EventsCountForUnlock;

	public bool OnlyMainCampaign;

	[HideIf("OnlyMainCampaign")]
	public BlueprintCampaignReference SpecificCampaign;

	[SerializeField]
	[ShowIf("ShowFlags")]
	private UnlockableFlagsPack[] m_Flags = new UnlockableFlagsPack[0];

	[UsedImplicitly]
	private bool ShowFlags => Type == AchievementType.Flags;

	public bool IsFlagsUnlocked
	{
		get
		{
			if (Type != AchievementType.Flags)
			{
				return false;
			}
			UnlockableFlagsPack[] flags = m_Flags;
			foreach (UnlockableFlagsPack obj in flags)
			{
				bool flag = true;
				UnlockableFlagData[] flags2 = obj.Flags;
				foreach (UnlockableFlagData unlockableFlagData in flags2)
				{
					flag &= unlockableFlagData.Flag.IsUnlocked && unlockableFlagData.Flag.Value >= unlockableFlagData.Value;
				}
				if (flag)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool FlagsAreNotSpecified()
	{
		if (m_Flags != null && m_Flags.Length != 0)
		{
			return m_Flags.All((UnlockableFlagsPack p) => p.Flags.Empty() || p.Flags.All((UnlockableFlagData f) => !f.Flag));
		}
		return true;
	}

	public bool AnyNullInFlags()
	{
		if (m_Flags != null)
		{
			return m_Flags.Any((UnlockableFlagsPack p) => p.Flags.Any((UnlockableFlagData f) => !f.Flag));
		}
		return false;
	}

	public bool AnyFlagWithValueLessThanZero()
	{
		if (m_Flags != null)
		{
			return m_Flags.Any((UnlockableFlagsPack p) => p.Flags.Any((UnlockableFlagData f) => f.Value < 0));
		}
		return false;
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (m_Flags.SelectMany((UnlockableFlagsPack f) => f.Flags).Any((UnlockableFlagData f) => f.Flag == flag))
		{
			return UnlockableFlagReferenceType.Check | UnlockableFlagReferenceType.CheckValue;
		}
		return UnlockableFlagReferenceType.None;
	}
}
