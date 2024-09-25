using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;

namespace Kingmaker.AI;

public static class AiBrainHelper
{
	public class ThreatsInfo
	{
		public HashSet<BaseUnitEntity> aooUnits = new HashSet<BaseUnitEntity>();

		public HashSet<BaseUnitEntity> overwatchUnits = new HashSet<BaseUnitEntity>();

		public HashSet<AreaEffectEntity> aes = new HashSet<AreaEffectEntity>();

		public HashSet<AreaEffectEntity> dmgOnMoveAes = new HashSet<AreaEffectEntity>();
	}

	private delegate bool ThreatCheck(AbilityAreaEffectLogic aeLogic, BaseUnitEntity unit);

	private static readonly ThreatCheck[] ThreatChecks = new ThreatCheck[2] { CheckDealDamage, CheckApplyBuffWithDamage };

	private static readonly ThreatCheck[] DamageOnMoveChecks = new ThreatCheck[1] { CheckDamageOnMove };

	public static Dictionary<GraphNode, ThreatsInfo> GatherThreatsData(BaseUnitEntity unit)
	{
		Dictionary<GraphNode, ThreatsInfo> dictionary = new Dictionary<GraphNode, ThreatsInfo>();
		foreach (UnitGroupMemory.UnitInfo enemy in unit.CombatGroup.Memory.Enemies)
		{
			BaseUnitEntity unit2 = enemy.Unit;
			if (unit2.CanMakeAttackOfOpportunity(unit))
			{
				foreach (GraphNode item in unit2.GetThreateningArea())
				{
					if (!dictionary.TryGetValue(item, out var value))
					{
						value = new ThreatsInfo();
						dictionary.Add(item, value);
					}
					value.aooUnits.Add(unit2);
				}
			}
			PartOverwatch optional = unit2.GetOptional<PartOverwatch>();
			if (optional == null)
			{
				continue;
			}
			foreach (CustomGridNodeBase item2 in optional.OverwatchArea)
			{
				if (!dictionary.TryGetValue(item2, out var value2))
				{
					value2 = new ThreatsInfo();
					dictionary.Add(item2, value2);
				}
				value2.overwatchUnits.Add(unit2);
			}
		}
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (!IsThreatningArea(areaEffect, unit))
			{
				continue;
			}
			foreach (CustomGridNodeBase coveredNode in areaEffect.CoveredNodes)
			{
				if (!dictionary.TryGetValue(coveredNode, out var value3))
				{
					value3 = new ThreatsInfo();
					dictionary.Add(coveredNode, value3);
				}
				value3.aes.Add(areaEffect);
				if (CheckThreats(areaEffect, unit, DamageOnMoveChecks))
				{
					value3.dmgOnMoveAes.Add(areaEffect);
				}
			}
		}
		return dictionary;
	}

	public static ThreatsInfo TryFindThreats(BaseUnitEntity unit, CustomGridNodeBase node)
	{
		HashSet<BaseUnitEntity> aooUnits = new HashSet<BaseUnitEntity>();
		HashSet<AreaEffectEntity> aes = new HashSet<AreaEffectEntity>();
		unit.CombatGroup.Memory.Enemies.ForEach(delegate(UnitGroupMemory.UnitInfo u)
		{
			if (u.Unit.IsThreat(node))
			{
				aooUnits.Add(u.Unit);
			}
		});
		Game.Instance.State.AreaEffects.ForEach(delegate(AreaEffectEntity ae)
		{
			if (ae.Contains(node) && IsThreatningArea(ae, unit))
			{
				aes.Add(ae);
			}
		});
		return new ThreatsInfo
		{
			aooUnits = aooUnits,
			aes = aes
		};
	}

	public static bool IsThreatningArea(AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		return CheckThreats(areaEffect, unit, ThreatChecks);
	}

	private static bool CheckThreats(AreaEffectEntity areaEffect, BaseUnitEntity unit, IEnumerable<ThreatCheck> checkList)
	{
		if (!areaEffect.IsSuitableTargetType(unit))
		{
			return false;
		}
		foreach (AbilityAreaEffectLogic item in from c in areaEffect.Blueprint.ComponentsArray
			where c is AbilityAreaEffectLogic
			select c as AbilityAreaEffectLogic)
		{
			foreach (ThreatCheck check in checkList)
			{
				if (check(item, unit))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool CheckDamageOnMove(AbilityAreaEffectLogic aeLogic, BaseUnitEntity unit)
	{
		if (!(aeLogic is AbilityAreaEffectRunAction abilityAreaEffectRunAction))
		{
			return false;
		}
		return IsDealDamage(abilityAreaEffectRunAction.UnitMove);
	}

	private static bool CheckDealDamage(AbilityAreaEffectLogic aeLogic, BaseUnitEntity unit)
	{
		if (!(aeLogic is AbilityAreaEffectRunAction abilityAreaEffectRunAction))
		{
			return false;
		}
		if (!IsDealDamage(abilityAreaEffectRunAction.UnitEnter) && !IsDealDamage(abilityAreaEffectRunAction.UnitExit) && !IsDealDamage(abilityAreaEffectRunAction.UnitMove))
		{
			return IsDealDamage(abilityAreaEffectRunAction.Round);
		}
		return true;
	}

	private static bool CheckApplyBuffWithDamage(AbilityAreaEffectLogic aeLogic, BaseUnitEntity unit)
	{
		if (!(aeLogic is AbilityAreaEffectBuff abilityAreaEffectBuff))
		{
			return false;
		}
		BlueprintComponent[] componentsArray = abilityAreaEffectBuff.Buff.ComponentsArray;
		for (int i = 0; i < componentsArray.Length; i++)
		{
			if (componentsArray[i] is AddFactContextActions addFactContextActions && (IsDealDamage(addFactContextActions.Activated) || IsDealDamage(addFactContextActions.Deactivated) || IsDealDamage(addFactContextActions.NewRound)))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsDealDamage(ActionList actionList)
	{
		return actionList.Actions.Any((GameAction p1) => IsDealDamage(p1));
	}

	private static bool IsDealDamage(GameAction a)
	{
		if (!(a is ContextActionDealDamage))
		{
			DodgeActions obj = a as DodgeActions;
			if (obj == null || !obj.ActionsOnDodge.Actions.Contains((GameAction a2) => a2 is ContextActionDealDamage))
			{
				return (a as DodgeActions)?.ActionsOnHit.Actions.Contains((GameAction a2) => a2 is ContextActionDealDamage) ?? false;
			}
		}
		return true;
	}
}
