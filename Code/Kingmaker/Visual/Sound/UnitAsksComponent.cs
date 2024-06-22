using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Enums.Sound;
using Kingmaker.Localization;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Common;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.Sound;

[AllowedOn(typeof(BlueprintUnitAsksList))]
[TypeId("95968ed93cfb4dc45b8141526f932ba4")]
public class UnitAsksComponent : BlueprintComponent, IUnlockableFlagReference
{
	[Serializable]
	public class BarkEntry
	{
		public SharedStringAsset Text;

		[AkEventReference]
		public string AkEvent;

		public float RandomWeight = 1f;

		public int ExcludeTime = 1;

		[Tooltip("bark can only trigger when ALL these flags are UNLOCKED")]
		[SerializeField]
		[FormerlySerializedAs("RequiredFlags")]
		private BlueprintUnlockableFlagReference[] m_RequiredFlags;

		[Tooltip("bark can only trigger when ALL these flags are LOCKED")]
		[SerializeField]
		[FormerlySerializedAs("ExcludedFlags")]
		private BlueprintUnlockableFlagReference[] m_ExcludedFlags;

		[NonSerialized]
		public int ExclusionCounter;

		public ReferenceArrayProxy<BlueprintUnlockableFlag> RequiredFlags
		{
			get
			{
				BlueprintReference<BlueprintUnlockableFlag>[] requiredFlags = m_RequiredFlags;
				return requiredFlags;
			}
		}

		public ReferenceArrayProxy<BlueprintUnlockableFlag> ExcludedFlags
		{
			get
			{
				BlueprintReference<BlueprintUnlockableFlag>[] excludedFlags = m_ExcludedFlags;
				return excludedFlags;
			}
		}

		public bool Locked
		{
			get
			{
				if (!RequiredFlags.EmptyIfNull().Any((BlueprintUnlockableFlag f) => Game.Instance.Player.UnlockableFlags.IsLocked(f)))
				{
					return ExcludedFlags.EmptyIfNull().Any((BlueprintUnlockableFlag f) => Game.Instance.Player.UnlockableFlags.IsUnlocked(f));
				}
				return true;
			}
		}
	}

	[Serializable]
	public class Bark
	{
		public BarkEntry[] Entries;

		public float Cooldown;

		public bool OverrideCooldownOnGamepad;

		[ShowIf("OverrideCooldownOnGamepad")]
		public float CooldownGamepad;

		public bool InterruptOthers;

		public float DelayMin;

		public float DelayMax;

		[Range(0f, 1f)]
		public float Chance = 1f;

		public bool ShowOnScreen;

		public bool DoNotPlayWhileAlone;

		public bool EnablePrioritization;

		[ShowIf("EnablePrioritization")]
		[Range(0f, 10f)]
		public int PrioritizationGroup;

		[ShowIf("EnablePrioritization")]
		[Range(0f, 10f)]
		public int Priority;

		public float GetCooldown()
		{
			if (!OverrideCooldownOnGamepad || !Game.Instance.IsControllerGamepad)
			{
				return Cooldown;
			}
			return CooldownGamepad;
		}

		public virtual bool CheckBarkChance(float chanceValue)
		{
			return PFStatefulRandom.Visuals.Sounds.value <= chanceValue;
		}
	}

	[Serializable]
	[IKnowWhatImDoing]
	public class AnimationBark : Bark
	{
		public MappedAnimationEventType AnimationEvent;

		public override bool CheckBarkChance(float chanceValue)
		{
			if (TurnController.IsInTurnBasedCombat() && AnimationEvent == MappedAnimationEventType.IdleCombat)
			{
				return PFStatefulRandom.Visuals.Sounds.value <= chanceValue * BlueprintRoot.Instance.Sound.TBMIdleAudioOverride;
			}
			return PFStatefulRandom.Visuals.Sounds.value <= chanceValue;
		}
	}

	[Serializable]
	public class PersonalizedBark : Bark
	{
		[SerializeField]
		private BlueprintUnitReference[] m_UnitReferences = Array.Empty<BlueprintUnitReference>();

		public BlueprintUnitReference[] UnitReferences => m_UnitReferences;
	}

	[Serializable]
	public class SpaceCombatEntry
	{
		public Bark StartCombatReactionSpaceCombat = new Bark();

		public Bark WinCombatReactionSpaceCombat = new Bark();

		public Bark MoveOrderSpaceCombat = new Bark();

		public Bark FireMacroCannonsSpaceCombat = new Bark();

		public Bark LanceFireSpaceCombat = new Bark();

		public Bark LaunchTorpedosSpaceCombat = new Bark();

		public Bark ShieldSectionIsDownSpaceCombat = new Bark();

		public Bark EnemyShieldSectionIsDownSpaceCombat = new Bark();

		public Bark LowHealthSpaceCombat = new Bark();

		public Bark EnemyDeathSpaceCombat = new Bark();

		public Bark LowShieldSpaceCombat = new Bark();

		public IEnumerable<Bark> GetAll()
		{
			yield return StartCombatReactionSpaceCombat;
			yield return WinCombatReactionSpaceCombat;
			yield return MoveOrderSpaceCombat;
			yield return FireMacroCannonsSpaceCombat;
			yield return LanceFireSpaceCombat;
			yield return LaunchTorpedosSpaceCombat;
			yield return ShieldSectionIsDownSpaceCombat;
			yield return EnemyShieldSectionIsDownSpaceCombat;
			yield return LowHealthSpaceCombat;
			yield return EnemyDeathSpaceCombat;
			yield return LowShieldSpaceCombat;
		}
	}

	[AkBankReference]
	public string[] SoundBanks = new string[0];

	[AkEventReference]
	public string PreviewSound = "";

	public Bark Aggro = new Bark();

	public Bark Pain = new Bark();

	public Bark Death = new Bark();

	public Bark Unconscious = new Bark();

	public Bark LowHealth = new Bark();

	public Bark CriticalHit = new Bark();

	public Bark Order = new Bark();

	public Bark Selected = new Bark();

	public Bark CantDo = new Bark();

	public Bark CheckSuccess = new Bark();

	public Bark CheckFail = new Bark();

	public Bark Discovery = new Bark();

	public Bark OrderMove = new Bark();

	public Bark OrderMoveExploration = new Bark();

	public Bark MomentumAction = new Bark();

	public Bark EnemyDeath = new Bark();

	public Bark PartyMemberUnconscious = new Bark();

	public Bark PsychicPhenomena = new Bark();

	public Bark PerilsOfTheWarp = new Bark();

	public Bark HealingAlly = new Bark();

	public Bark BeingHealed = new Bark();

	public Bark EnemyMassDeath = new Bark();

	public Bark ThrowingGrenade = new Bark();

	public Bark FriendlyFire = new Bark();

	public Bark UsingCombatDrug = new Bark();

	public Bark AggroQuestionHuman = new Bark();

	public Bark AggroQuestionXenos = new Bark();

	public Bark AggroQuestionChaos = new Bark();

	public Bark AggroRaceAnswer = new Bark();

	public Bark OutOfAmmo = new Bark();

	public PersonalizedBark[] PartyMemberUnconsciousPersonalized;

	public AnimationBark[] AnimationBarks;

	public SpaceCombatEntry SpaceCombatBarks = new SpaceCombatEntry();

	public void PlayPreview()
	{
		GameObject gameObject = UIDollRooms.Instance.gameObject;
		SoundEventsManager.PostEvent(PreviewSound, gameObject);
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		List<Bark> list = new List<Bark>();
		list.Add(Aggro);
		list.Add(Pain);
		list.Add(Death);
		list.Add(Unconscious);
		list.Add(LowHealth);
		list.Add(CriticalHit);
		list.Add(Order);
		list.Add(Selected);
		list.Add(CantDo);
		list.Add(CheckSuccess);
		list.Add(CheckFail);
		list.Add(Discovery);
		list.Add(OrderMove);
		list.Add(OrderMoveExploration);
		list.Add(MomentumAction);
		list.Add(EnemyDeath);
		list.Add(PartyMemberUnconscious);
		list.Add(PsychicPhenomena);
		list.Add(PerilsOfTheWarp);
		list.Add(HealingAlly);
		list.Add(BeingHealed);
		list.Add(EnemyMassDeath);
		list.Add(ThrowingGrenade);
		list.Add(FriendlyFire);
		list.Add(UsingCombatDrug);
		list.Add(AggroQuestionHuman);
		list.Add(AggroQuestionXenos);
		list.Add(AggroQuestionChaos);
		list.Add(AggroRaceAnswer);
		list.Add(OutOfAmmo);
		list.AddRange(AnimationBarks);
		list.AddRange(PartyMemberUnconsciousPersonalized);
		list.AddRange(SpaceCombatBarks.GetAll());
		if ((from f in list.SelectMany((Bark b) => b.Entries).SelectMany((BarkEntry e) => e.ExcludedFlags.Concat(e.RequiredFlags))
			where f
			select f).Contains(flag))
		{
			return UnlockableFlagReferenceType.Check;
		}
		return UnlockableFlagReferenceType.None;
	}
}
