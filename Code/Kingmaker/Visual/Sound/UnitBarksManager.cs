using System;
using System.Collections;
using System.Linq;
using Core.Cheats;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums.Sound;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class UnitBarksManager
{
	private BarkWrapper m_CurrentlyActiveBark;

	private uint m_CurrentlyPlayingId;

	public readonly AbstractUnitEntity Unit;

	private readonly string[] m_SoundBanks;

	public readonly BarkWrapper Aggro;

	public readonly BarkWrapper Pain;

	public readonly BarkWrapper Death;

	public readonly BarkWrapper Unconscious;

	public readonly BarkWrapper LowHealth;

	public readonly BarkWrapper CriticalHit;

	public readonly BarkWrapper Order;

	public readonly BarkWrapper Selected;

	public readonly BarkWrapper CantDo;

	public readonly BarkWrapper CheckSuccess;

	public readonly BarkWrapper CheckFail;

	public readonly BarkWrapper Discovery;

	public readonly BarkWrapper OrderMove;

	public readonly BarkWrapper OrderMoveExploration;

	public readonly BarkWrapper MomentumAction;

	public readonly BarkWrapper EnemyDeath;

	public readonly BarkWrapper PartyMemberUnconscious;

	public readonly BarkWrapper PsychicPhenomena;

	public readonly BarkWrapper PerilsOfTheWarp;

	public readonly BarkWrapper HealingAlly;

	public readonly BarkWrapper BeingHealed;

	public readonly BarkWrapper EnemyMassDeath;

	public readonly BarkWrapper ThrowingGrenade;

	public readonly BarkWrapper FriendlyFire;

	public readonly BarkWrapper UsingCombatDrug;

	public readonly BarkWrapper AggroQuestionHuman;

	public readonly BarkWrapper AggroQuestionXenos;

	public readonly BarkWrapper AggroQuestionChaos;

	public readonly BarkWrapper AggroRaceAnswer;

	public readonly BarkWrapper OutOfAmmo;

	public readonly BarkWrapper[] PartyMemberUnconsciousPersonalized;

	public readonly BarkWrapper[] AnimationBarks;

	public readonly BarkWrapper StartCombatReactionSpaceCombat;

	public readonly BarkWrapper WinCombatReactionSpaceCombat;

	public readonly BarkWrapper MoveOrderSpaceCombat;

	public readonly BarkWrapper FireMacroCannonsSpaceCombat;

	public readonly BarkWrapper LanceFireSpaceCombat;

	public readonly BarkWrapper LaunchTorpedosSpaceCombat;

	public readonly BarkWrapper ShieldSectionIsDownSpaceCombat;

	public readonly BarkWrapper EnemyShieldSectionIsDownSpaceCombat;

	public readonly BarkWrapper LowHealthSpaceCombat;

	public readonly BarkWrapper EnemyDeathSpaceCombat;

	public readonly BarkWrapper LowShieldSpaceCombat;

	public BarkWrapper SelectAnimationBark(MappedAnimationEventType evt)
	{
		return (from b in AnimationBarks.EmptyIfNull()
			where ((UnitAsksComponent.AnimationBark)b.Bark).AnimationEvent == evt
			select b).Random(PFStatefulRandom.Visuals.Sounds);
	}

	public UnitBarksManager(AbstractUnitEntity unit, UnitAsksComponent component)
	{
		m_SoundBanks = component.SoundBanks;
		Unit = unit;
		Aggro = Wrap(component.Aggro);
		Pain = Wrap(component.Pain);
		Death = Wrap(component.Death);
		Unconscious = Wrap(component.Unconscious);
		LowHealth = Wrap(component.LowHealth);
		CriticalHit = Wrap(component.CriticalHit);
		Order = Wrap(component.Order);
		Selected = Wrap(component.Selected);
		CantDo = Wrap(component.CantDo);
		CheckSuccess = Wrap(component.CheckSuccess);
		CheckFail = Wrap(component.CheckFail);
		Discovery = Wrap(component.Discovery);
		OrderMove = Wrap(component.OrderMove);
		OrderMoveExploration = Wrap(component.OrderMoveExploration);
		MomentumAction = Wrap(component.MomentumAction);
		EnemyDeath = Wrap(component.EnemyDeath);
		PartyMemberUnconscious = Wrap(component.PartyMemberUnconscious);
		PsychicPhenomena = Wrap(component.PsychicPhenomena);
		PerilsOfTheWarp = Wrap(component.PerilsOfTheWarp);
		HealingAlly = Wrap(component.HealingAlly);
		BeingHealed = Wrap(component.BeingHealed);
		EnemyMassDeath = Wrap(component.EnemyMassDeath);
		ThrowingGrenade = Wrap(component.ThrowingGrenade);
		FriendlyFire = Wrap(component.FriendlyFire);
		UsingCombatDrug = Wrap(component.UsingCombatDrug);
		AggroQuestionHuman = Wrap(component.AggroQuestionHuman);
		AggroQuestionXenos = Wrap(component.AggroQuestionXenos);
		AggroQuestionChaos = Wrap(component.AggroQuestionChaos);
		AggroRaceAnswer = Wrap(component.AggroRaceAnswer);
		OutOfAmmo = Wrap(component.OutOfAmmo);
		PartyMemberUnconsciousPersonalized = component.PartyMemberUnconsciousPersonalized.EmptyIfNull().Select(Wrap).ToArray();
		AnimationBarks = component.AnimationBarks.EmptyIfNull().Select(Wrap).ToArray();
		StartCombatReactionSpaceCombat = Wrap(component.SpaceCombatBarks.StartCombatReactionSpaceCombat);
		WinCombatReactionSpaceCombat = Wrap(component.SpaceCombatBarks.WinCombatReactionSpaceCombat);
		MoveOrderSpaceCombat = Wrap(component.SpaceCombatBarks.MoveOrderSpaceCombat);
		FireMacroCannonsSpaceCombat = Wrap(component.SpaceCombatBarks.FireMacroCannonsSpaceCombat);
		LanceFireSpaceCombat = Wrap(component.SpaceCombatBarks.LanceFireSpaceCombat);
		LaunchTorpedosSpaceCombat = Wrap(component.SpaceCombatBarks.LaunchTorpedosSpaceCombat);
		ShieldSectionIsDownSpaceCombat = Wrap(component.SpaceCombatBarks.ShieldSectionIsDownSpaceCombat);
		EnemyShieldSectionIsDownSpaceCombat = Wrap(component.SpaceCombatBarks.EnemyShieldSectionIsDownSpaceCombat);
		LowHealthSpaceCombat = Wrap(component.SpaceCombatBarks.LowHealthSpaceCombat);
		EnemyDeathSpaceCombat = Wrap(component.SpaceCombatBarks.EnemyDeathSpaceCombat);
		LowShieldSpaceCombat = Wrap(component.SpaceCombatBarks.LowShieldSpaceCombat);
	}

	private BarkWrapper Wrap(UnitAsksComponent.Bark bark)
	{
		return new BarkWrapper(bark, this);
	}

	public void LoadBanks()
	{
		string[] soundBanks = m_SoundBanks;
		for (int i = 0; i < soundBanks.Length; i++)
		{
			SoundBanksManager.LoadBank(soundBanks[i]);
		}
		OnLoadBanks();
	}

	protected virtual void OnLoadBanks()
	{
	}

	public void UnloadBanks()
	{
		string[] soundBanks = m_SoundBanks;
		for (int i = 0; i < soundBanks.Length; i++)
		{
			SoundBanksManager.UnloadBank(soundBanks[i]);
		}
		OnUnloadBanks();
	}

	protected virtual void OnUnloadBanks()
	{
	}

	public bool Schedule(BarkWrapper wrapper, bool is2D = false, AkCallbackManager.EventCallback callback = null)
	{
		UnitAsksComponent.Bark bark = wrapper?.Bark;
		if (bark == null)
		{
			return false;
		}
		if (bark.DoNotPlayWhileAlone && Game.Instance.Player.CapitalPartyMode)
		{
			return false;
		}
		if (bark.Entries == null || bark.Entries.Length == 0 || wrapper.IsOnCooldown)
		{
			return false;
		}
		if (wrapper.Bark.EnablePrioritization)
		{
			UnitAsksPriorityHelper.RegisterBark(wrapper);
			BarkWrapper currentHighestPriorityBark = UnitAsksPriorityHelper.GetCurrentHighestPriorityBark(wrapper.Bark.PrioritizationGroup);
			if (wrapper != currentHighestPriorityBark && wrapper.Bark.Priority >= currentHighestPriorityBark.Bark.Priority)
			{
				return false;
			}
		}
		if (!bark.InterruptOthers && m_CurrentlyActiveBark != null)
		{
			return false;
		}
		float num = 1f;
		AbstractUnitEntity unit = Unit;
		if (unit != null && unit.IsPlayerFaction)
		{
			num = SettingsRoot.Sound.VoicedAskFrequency.GetValue() switch
			{
				VoiceAskFrequency.Never => 0f, 
				VoiceAskFrequency.Occasionally => num / 6f, 
				VoiceAskFrequency.Frequently => num / 2f, 
				VoiceAskFrequency.Constantly => 1f, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		num = bark.Chance * num;
		if (!bark.CheckBarkChance(num))
		{
			return false;
		}
		UnitAsksComponent.BarkEntry entry = ((bark.Entries.Length == 1) ? bark.Entries[0] : SelectRandomEntry(bark));
		float num2 = PFStatefulRandom.Visuals.Sounds.Range(bark.DelayMin, bark.DelayMax);
		if (num2 < 0.01f || Unit == null)
		{
			Play(wrapper, entry, is2D, callback);
		}
		else
		{
			Unit.View.StartCoroutine(PlayAfter(wrapper, num2, entry, is2D, callback, synced: false));
		}
		return true;
	}

	public void DiscardCurrentActiveBark()
	{
		SoundEventsManager.StopPlayingById(m_CurrentlyPlayingId);
		m_CurrentlyActiveBark = null;
	}

	private static UnitAsksComponent.BarkEntry SelectRandomEntry(UnitAsksComponent.Bark bark)
	{
		UnitAsksComponent.BarkEntry barkEntry = null;
		float num = 0f;
		for (int i = 0; i < bark.Entries.Length; i++)
		{
			UnitAsksComponent.BarkEntry barkEntry2 = bark.Entries[i];
			if (barkEntry2.Locked)
			{
				continue;
			}
			if (barkEntry2.ExclusionCounter > 0)
			{
				barkEntry2.ExclusionCounter--;
				continue;
			}
			float randomWeight = barkEntry2.RandomWeight;
			if (PFStatefulRandom.Visuals.Sounds.Range(0f, num + randomWeight) >= num)
			{
				barkEntry = barkEntry2;
			}
			num += randomWeight;
		}
		if (barkEntry == null)
		{
			barkEntry = bark.Entries[0];
		}
		barkEntry.ExclusionCounter = barkEntry.ExcludeTime;
		return barkEntry;
	}

	private IEnumerator PlayAfter(BarkWrapper wrapper, float delay, UnitAsksComponent.BarkEntry entry, bool is2D, AkCallbackManager.EventCallback callback = null, bool synced = true)
	{
		wrapper.IsPlaying = true;
		m_CurrentlyActiveBark = wrapper;
		yield return new WaitForSeconds(delay);
		Play(wrapper, entry, is2D, callback, synced);
	}

	private void Play(BarkWrapper wrapper, UnitAsksComponent.BarkEntry entry, bool is2D, AkCallbackManager.EventCallback callback = null, bool synced = false)
	{
		wrapper.IsPlaying = true;
		m_CurrentlyActiveBark = wrapper;
		if (!string.IsNullOrEmpty(entry.AkEvent))
		{
			if (!is2D && Unit == null)
			{
				PFLog.Default.Warning("Can not play " + entry.AkEvent + " in 3D cause no unit entity. Will play in 2D");
				is2D = true;
			}
			m_CurrentlyPlayingId = SoundEventsManager.PostEvent(entry.AkEvent, is2D ? SoundState.Get2DSoundObject() : Unit.View.gameObject, 1u, delegate(object cookie, AkCallbackType type, AkCallbackInfo info)
			{
				wrapper.IsPlaying = false;
				wrapper.LastPlayTime = (float)Game.Instance.TimeController.RealTime.TotalSeconds;
				m_CurrentlyActiveBark = null;
				callback?.Invoke(cookie, type, info);
			}, null);
			if (m_CurrentlyPlayingId == 0)
			{
				wrapper.IsPlaying = false;
				m_CurrentlyActiveBark = null;
			}
		}
		else
		{
			wrapper.IsPlaying = false;
			m_CurrentlyActiveBark = null;
		}
		if (!(entry.Text != null) || Unit == null)
		{
			return;
		}
		if (wrapper.Bark.ShowOnScreen)
		{
			BarkPlayer.Bark(Unit, entry.Text.String, -1f, playVoiceOver: false, null, synced);
		}
		else
		{
			EventBus.RaiseEvent((IEntity)Unit, (Action<ICombatLogBarkHandler>)delegate(ICombatLogBarkHandler h)
			{
				h.HandleOnShowBark(entry.Text.String);
			}, isCheckRuntime: true);
		}
		if (!wrapper.IsPlaying)
		{
			wrapper.LastPlayTime = (float)Game.Instance.TimeController.RealTime.TotalSeconds;
		}
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void OverridePlayerAsks(BlueprintUnitAsksList asksList)
	{
		Game.Instance.Player.MainCharacterEntity.Asks.SetOverride(asksList);
		Game.Instance.Player.MainCharacterEntity.View.UpdateAsks();
	}
}
