using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("18beb8746b848f6448d3ee6969a32467")]
public class WarhammerOverrideAbilityApCost : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityActionPointCost>, IRulebookHandler<RuleCalculateAbilityActionPointCost>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public enum WeaponAPParameter
	{
		Single,
		Burst,
		Special,
		StaticConstant,
		Reload,
		CostBonus,
		Zero,
		DefaultCost
	}

	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	private WeaponAPParameter m_overrideMode;

	[SerializeField]
	[ShowIf("m_NewCost")]
	private int m_newCost;

	[SerializeField]
	[ShowIf("CostBonus")]
	private int m_costBonus;

	[SerializeField]
	[ShowIf("CostBonus")]
	[InfoBox("Оставьте -1, если переопределять минимальный Cost не нужно")]
	private int m_costMinimum = -1;

	[InfoBox(Text = "Ability или группа, к которым нужно применять изменение стоимости AP. Если оба не заданы, применяется к текущей")]
	[SerializeField]
	[CanBeNull]
	private BlueprintAbilityReference m_Ability;

	[SerializeField]
	private BlueprintAbilityGroupReference m_AffectedGroup;

	[SerializeField]
	private BlueprintAbilityGroupReference[] m_AbilityGroupList;

	public bool NotSelectedGroup;

	public bool AffectOnlyMelee;

	public bool AffectOnlyRanged;

	public bool AffectOnlyAoE;

	public bool AffectOnlyBurst;

	public bool AffectOnlyHeavy;

	public BlueprintAbility Ability => m_Ability?.Get();

	public BlueprintAbilityGroup AffectedGroup => m_AffectedGroup?.Get();

	public ReferenceArrayProxy<BlueprintAbilityGroup> AbilityGroupList
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] abilityGroupList = m_AbilityGroupList;
			return abilityGroupList;
		}
	}

	public bool StaticConst => m_overrideMode == WeaponAPParameter.StaticConstant;

	public bool CostBonus => m_overrideMode == WeaponAPParameter.CostBonus;

	private bool DefaultCost => m_overrideMode == WeaponAPParameter.DefaultCost;

	private bool m_NewCost
	{
		get
		{
			if (!StaticConst)
			{
				return DefaultCost;
			}
			return true;
		}
	}

	public void OnEventAboutToTrigger(RuleCalculateAbilityActionPointCost evt)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt, evt.AbilityData))
			{
				return;
			}
		}
		if ((AffectedGroup == null && Ability == null && AbilityGroupList.Length == 0 && evt.Blueprint != base.OwnerBlueprint) || (AffectedGroup != null && ((NotSelectedGroup && evt.Blueprint.AbilityGroups.Contains(AffectedGroup)) || (!NotSelectedGroup && !evt.Blueprint.AbilityGroups.Contains(AffectedGroup)))) || (AbilityGroupList.Length != 0 && !evt.Blueprint.AbilityGroups.Intersect(AbilityGroupList).Any()) || (Ability != null && evt.Blueprint != Ability))
		{
			return;
		}
		if (AffectOnlyMelee)
		{
			ItemEntityWeapon weapon = evt.Weapon;
			if (weapon == null || !weapon.Blueprint.IsMelee)
			{
				return;
			}
		}
		if (AffectOnlyRanged)
		{
			ItemEntityWeapon weapon2 = evt.Weapon;
			if (weapon2 == null || !weapon2.Blueprint.IsRanged)
			{
				return;
			}
		}
		if ((AffectOnlyAoE && !evt.Blueprint.IsAoE) || (AffectOnlyBurst && !evt.Blueprint.IsBurst))
		{
			return;
		}
		if (AffectOnlyHeavy)
		{
			ItemEntityWeapon weapon3 = evt.Weapon;
			if (weapon3 == null || weapon3.Blueprint.Heaviness != WeaponHeaviness.Heavy)
			{
				return;
			}
		}
		if (CostBonus)
		{
			evt.CostBonus += m_costBonus;
			evt.CostMinimum = m_costMinimum;
		}
		if (StaticConst && m_newCost >= 0)
		{
			evt.CostOverride = m_newCost;
		}
		if (DefaultCost && m_newCost >= 0)
		{
			evt.DefaultCostOverride = m_newCost;
		}
		if (m_overrideMode == WeaponAPParameter.Zero)
		{
			evt.CostOverride = 0;
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityActionPointCost evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
