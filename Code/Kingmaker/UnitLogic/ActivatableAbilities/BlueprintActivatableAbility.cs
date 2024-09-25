using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.ActivatableAbilities;

[TypeId("f7bbaafe7a7cbad4693107291c548dcc")]
public class BlueprintActivatableAbility : BlueprintUnitFact, IBlueprintScanner, IResourceIdsHolder
{
	[SerializeField]
	[FormerlySerializedAs("Buff")]
	private BlueprintBuffReference m_Buff;

	public ActivatableAbilityGroup Group;

	[ShowIf("HasGroup")]
	public int WeightInGroup = 1;

	public bool IsOnByDefault;

	public bool DeactivateIfCombatEnded;

	public bool DeactivateAfterFirstRound;

	public bool DeactivateImmediately;

	public bool IsTargeted;

	public bool DeactivateIfOwnerDisabled;

	public bool DeactivateIfOwnerUnconscious;

	public bool OnlyInCombat;

	public bool DoNotTurnOffOnRest;

	public AbilityActivationType ActivationType;

	[SerializeField]
	[ShowIf("ActivateOnUnitAction")]
	private AbilityActivateOnUnitActionType m_ActivateOnUnitAction;

	[SerializeField]
	[ShowIf("IsTargeted")]
	private BlueprintAbilityReference m_SelectTargetAbility;

	public string[] ResourceAssetIds;

	public BlueprintBuff Buff => m_Buff?.Get();

	public bool HasGroup => Group != ActivatableAbilityGroup.None;

	public bool ActivateOnCombatStarts
	{
		get
		{
			if (DeactivateIfCombatEnded)
			{
				return !ActivateOnUnitAction;
			}
			return false;
		}
	}

	public bool ActivateImmediately => ActivationType == AbilityActivationType.Immediately;

	public bool ActivateOnUnitAction => ActivationType == AbilityActivationType.OnUnitAction;

	public AbilityActivateOnUnitActionType ActivateOnUnitActionType => m_ActivateOnUnitAction;

	public BlueprintAbility SelectTargetAbility => m_SelectTargetAbility?.Get();

	protected override Type GetFactType()
	{
		return typeof(ActivatableAbility);
	}

	public override MechanicEntityFact CreateFact(MechanicsContext parentContext, MechanicEntity owner, BuffDuration duration)
	{
		return new ActivatableAbility(this);
	}

	public string[] GetResourceIds()
	{
		return ResourceAssetIds;
	}

	public void Scan()
	{
	}
}
