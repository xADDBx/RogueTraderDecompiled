using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.ActivatableAbilities;

[AllowedOn(typeof(BlueprintActivatableAbility))]
[TypeId("22c7a9a55d4f5724b89759d342784915")]
public class ActivatableAbilityResourceLogic : UnitFactComponentDelegate, IActivatableAbilitySpendResourceLogic, IHashable
{
	public enum ResourceSpendType
	{
		None,
		TurnOn,
		Start,
		NewRound,
		Attack,
		Judgment,
		AttackHit,
		AttackCrit,
		AttackSpecificWeapon,
		Never
	}

	public ResourceSpendType SpendType;

	public bool SpendActionPointsInstead;

	[ShowIf("SpendActionPointsInstead")]
	public int ActionPointCost;

	[SerializeField]
	[HideIf("SpendActionPointsInstead")]
	private BlueprintAbilityResourceReference m_RequiredResource;

	[SerializeField]
	private BlueprintUnitFactReference m_FreeBlueprint;

	[SerializeField]
	private WeaponCategory[] Categories;

	public BlueprintAbilityResource RequiredResource => m_RequiredResource?.Get();

	public BlueprintUnitFact FreeBlueprint => m_FreeBlueprint?.Get();

	[UsedImplicitly]
	public bool AttackSpecificWeapon => SpendType == ResourceSpendType.AttackSpecificWeapon;

	public bool IsAvailable(EntityFactComponent runtime)
	{
		using (runtime.RequestEventContext())
		{
			if (base.Owner.Facts.Contains(FreeBlueprint))
			{
				return true;
			}
			if (SpendActionPointsInstead)
			{
				return base.Owner.CombatState.ActionPointsYellow >= ActionPointCost;
			}
			if ((bool)RequiredResource)
			{
				if (SpendType != ResourceSpendType.Judgment)
				{
					return base.Owner.AbilityResources.HasEnoughResource(RequiredResource, (SpendType != 0) ? 1 : 0);
				}
				return base.Owner.AbilityResources.HasEnoughResource(RequiredResource, 1);
			}
			if (base.Fact.SourceItem != null)
			{
				return !base.Fact.SourceItem.IsSpendCharges || base.Fact.SourceItem.Charges > 0;
			}
			return false;
		}
	}

	void IActivatableAbilitySpendResourceLogic.OnAbilityTurnOn()
	{
		if (SpendType == ResourceSpendType.TurnOn)
		{
			SpendResource();
		}
	}

	void IActivatableAbilitySpendResourceLogic.OnStart()
	{
		if (SpendType == ResourceSpendType.Start)
		{
			SpendResource();
		}
	}

	void IActivatableAbilitySpendResourceLogic.OnNewRound()
	{
		if (SpendType == ResourceSpendType.NewRound)
		{
			SpendResource();
		}
	}

	void IActivatableAbilitySpendResourceLogic.OnAttack(BlueprintItemWeapon weapon)
	{
		if (SpendType == ResourceSpendType.Attack)
		{
			SpendResource();
		}
		if (SpendType == ResourceSpendType.AttackSpecificWeapon && Categories.Contains(weapon.Category))
		{
			SpendResource();
		}
	}

	void IActivatableAbilitySpendResourceLogic.OnHit()
	{
		if (SpendType == ResourceSpendType.AttackHit)
		{
			SpendResource();
		}
	}

	void IActivatableAbilitySpendResourceLogic.OnCrit()
	{
		if (SpendType == ResourceSpendType.AttackCrit)
		{
			SpendResource();
		}
	}

	void IActivatableAbilitySpendResourceLogic.ManualSpendResource()
	{
		SpendResource(manual: true);
	}

	private void SpendResource(bool manual = false)
	{
		if (base.Owner.Facts.Contains(FreeBlueprint))
		{
			return;
		}
		if (SpendActionPointsInstead)
		{
			base.Owner.CombatState.SpendActionPoints(ActionPointCost);
			EventBus.RaiseEvent(delegate(IUnitActionPointsHandler h)
			{
				h.HandleActionPointsSpent(base.Owner);
			});
		}
		if ((bool)RequiredResource && !SpendActionPointsInstead)
		{
			base.Owner.AbilityResources.Spend(RequiredResource, 1);
		}
		else
		{
			base.Fact.SourceItem?.SpendCharges();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
