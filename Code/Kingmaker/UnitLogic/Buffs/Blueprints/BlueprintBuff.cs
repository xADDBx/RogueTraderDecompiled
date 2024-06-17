using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.Sound;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Blueprints;

[Serializable]
[TypeId("618a7e0d54149064ab3ffa5d9057362c")]
public class BlueprintBuff : BlueprintUnitFact, IResourcesHolder, IResourceIdsHolder, IBlueprintScanner, IBlueprintFactWithRanks
{
	[Flags]
	private enum Flags
	{
		IsFromSpell = 1,
		HiddenInUi = 2,
		ShowInLogOnlyOnYourself = 4,
		StayOnDeath = 8,
		RemoveOnRest = 0x10,
		RemoveOnResurrect = 0x20,
		Harmful = 0x40,
		NeedsNoVisual = 0x80,
		DynamicDamage = 0x100,
		ShowInDialogue = 0x200,
		PriorityInUI = 0x400
	}

	public enum InitiativeType
	{
		ByCaster,
		ByOwner
	}

	public bool IsClassFeature;

	[SerializeField]
	[EnumFlagsAsButtons]
	private Flags m_Flags;

	[SerializeField]
	private AkSwitchReference m_SoundTypeSwitch;

	[SerializeField]
	private AkSwitchReference m_MuffledTypeSwitch;

	public StackingType Stacking;

	[ShowIf("IsHighestByPriority")]
	public ContextPropertyName PriorityProperty;

	public InitiativeType Initiative;

	[SerializeField]
	[ShowIf("HasRanks")]
	public int Ranks;

	public bool TickEachSecond;

	[HideIf("TickEachSecond")]
	public DurationRate Frequency;

	[Tooltip("Use VisualFXSettings with Start event instead.")]
	[Obsolete("Use VisualFXSettings with Start instead.")]
	public PrefabLink FxOnStart;

	[Tooltip("Use VisualFXSettings with End event instead.")]
	[Obsolete("Use VisualFXSettings with End event instead.")]
	public PrefabLink FxOnRemove;

	[SerializeField]
	private BlueprintAbilityFXSettings.Reference m_FXSettings;

	public string[] ResourceAssetIds;

	public bool PlayOnlyFirstHitSound;

	[SerializeField]
	private BlueprintAbilityGroupReference[] m_AbilityGroups;

	[SerializeField]
	private bool m_Cyclical;

	public int MaxRank
	{
		get
		{
			if (!HasRanks)
			{
				return 1;
			}
			return Math.Max(1, Ranks);
		}
	}

	public AkSwitchReference SoundTypeSwitch => m_SoundTypeSwitch;

	public AkSwitchReference MuffledTypeSwitch => m_MuffledTypeSwitch;

	public bool HasRanks
	{
		get
		{
			StackingType stacking = Stacking;
			return stacking == StackingType.Rank || stacking == StackingType.Stack;
		}
	}

	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettings => m_FXSettings;

	public bool IsFromSpell => HasFlag(Flags.IsFromSpell);

	public bool IsHiddenInUI => HasFlag(Flags.HiddenInUi);

	public bool NeedsNoVisual => HasFlag(Flags.NeedsNoVisual);

	public bool StayOnDeath => HasFlag(Flags.StayOnDeath);

	public bool RemoveOnRest => HasFlag(Flags.RemoveOnRest);

	public bool RemoveOnResurrect => HasFlag(Flags.RemoveOnResurrect);

	public bool Harmful => HasFlag(Flags.Harmful);

	public bool ShowInLogOnlyOnYourself => HasFlag(Flags.ShowInLogOnlyOnYourself);

	public bool DynamicDamage => HasFlag(Flags.DynamicDamage);

	public bool ShowInDialogue => HasFlag(Flags.ShowInDialogue);

	public bool PriorityInUI => HasFlag(Flags.PriorityInUI);

	public TimeSpan TickTime
	{
		get
		{
			if (this.GetComponent<ITickEachRound>() == null)
			{
				return TimeSpan.MaxValue;
			}
			if (!TickEachSecond)
			{
				return Frequency.ToRounds().Seconds;
			}
			return 1.Seconds();
		}
	}

	public bool IsHardCrowdControl => this.GetComponent<HardCrowdControlBuff>() != null;

	private bool IsHighestByPriority => Stacking == StackingType.HighestByProperty;

	public ReferenceArrayProxy<BlueprintAbilityGroup> AbilityGroups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] abilityGroups = m_AbilityGroups;
			return abilityGroups;
		}
	}

	public bool Cyclical => m_Cyclical;

	protected override Type GetFactType()
	{
		return typeof(Buff);
	}

	public override MechanicEntityFact CreateFact(MechanicsContext parentContext, MechanicEntity owner, BuffDuration duration)
	{
		MechanicsContext context = parentContext?.CloneFor(this, owner) ?? new MechanicsContext(owner, owner, this);
		return new Buff(this, context, duration);
	}

	private bool HasFlag(Flags flag)
	{
		return (m_Flags & flag) != 0;
	}

	public string[] GetResourceIds()
	{
		return ResourceAssetIds;
	}

	public IEnumerable<WeakResourceLink> GetResources()
	{
		return null;
	}

	public void Scan()
	{
	}
}
