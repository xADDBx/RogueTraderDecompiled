using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.Settings;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial;

public class Tutorial : EntityFact<TutorialSystem>, IHashable
{
	private bool m_Enabled;

	private readonly CountableFlag m_EnabledByEtude = new CountableFlag();

	[JsonProperty]
	public int TriggeredTimes { get; set; }

	[JsonProperty]
	public int ShowedTimes { get; set; }

	[JsonProperty]
	public int LastShowIndex { get; set; }

	[JsonProperty]
	public int TriggerLogicCounter { get; set; }

	[JsonProperty]
	public bool Banned { get; set; }

	public new BlueprintTutorial Blueprint => (BlueprintTutorial)base.Blueprint;

	public bool HasTrigger => Blueprint.GetComponent<TutorialTrigger>() != null;

	public override bool IsEnabled => m_Enabled;

	public bool IsLimitReached
	{
		get
		{
			if (ShowedTimes >= Blueprint.Limit)
			{
				return Blueprint.Limit == 0;
			}
			return true;
		}
	}

	public Tutorial(BlueprintTutorial fact)
		: base((BlueprintFact)fact)
	{
	}

	[JsonConstructor]
	private Tutorial()
	{
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		UpdateIsEnabled();
	}

	public bool IsBanned(bool fromTrigger)
	{
		if (!(HasTrigger && fromTrigger))
		{
			return base.Owner.IsTagBanned(Blueprint.Tag);
		}
		if (!Banned)
		{
			return !SettingsRoot.Game.Tutorial.ShowContextTutorial;
		}
		return true;
	}

	public void UpdateIsEnabled()
	{
		m_Enabled = (bool)m_EnabledByEtude && IsLimitReached && !IsBanned(fromTrigger: true);
		UpdateIsActive();
	}

	public void EnableByEtude()
	{
		m_EnabledByEtude.Retain();
		UpdateIsEnabled();
	}

	public void DisableByEtude()
	{
		m_EnabledByEtude.Release();
		UpdateIsEnabled();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		int val2 = TriggeredTimes;
		result.Append(ref val2);
		int val3 = ShowedTimes;
		result.Append(ref val3);
		int val4 = LastShowIndex;
		result.Append(ref val4);
		int val5 = TriggerLogicCounter;
		result.Append(ref val5);
		bool val6 = Banned;
		result.Append(ref val6);
		return result;
	}
}
