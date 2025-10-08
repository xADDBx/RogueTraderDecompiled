using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.UI.MVVM.VM.Inspect;

public class InspectExtensions
{
	public static bool TryGetWoundsText(MechanicEntityUIWrapper unitUIWrapper, out string woundsValue, out string woundsAddValue)
	{
		PartHealth health = unitUIWrapper.Health;
		if (health == null)
		{
			woundsValue = string.Empty;
			woundsAddValue = string.Empty;
			return false;
		}
		if (unitUIWrapper.MechanicEntity.HasMechanicFeature(MechanicsFeatureType.HideRealHealthInUI))
		{
			woundsValue = "???";
			woundsAddValue = string.Empty;
			return true;
		}
		woundsValue = UIUtility.GetHpText(unitUIWrapper, unitUIWrapper.IsDead);
		string text = ((health.TemporaryHitPoints > 0) ? "+" : "-") + health.TemporaryHitPoints + " " + UIStrings.Instance.CharacterSheet.TemporaryHP.Text;
		woundsAddValue = ((health.TemporaryHitPoints == 0) ? "" : text);
		return true;
	}

	public static string GetDeflection(BaseUnitEntity unit)
	{
		return Rulebook.Trigger(new RuleCalculateStatsArmor(unit)).ResultDeflection.ToString();
	}

	public static string GetArmor(BaseUnitEntity unit)
	{
		return Rulebook.Trigger(new RuleCalculateStatsArmor(unit)).ResultAbsorption + "%";
	}

	public static string GetDodge(BaseUnitEntity unit)
	{
		return Rulebook.Trigger(new RuleCalculateDodgeChance((UnitEntity)unit)).Result + "%";
	}

	public static string GetMovementPoints(BaseUnitEntity unit)
	{
		return unit.CombatState.ActionPointsBlueMax.ToString();
	}

	public static List<TooltipBrickBuff> GetBuffs(BaseUnitEntity unit)
	{
		List<Buff> list = unit.Buffs.RawFacts;
		Dictionary<BlueprintBuff, List<Buff>> dictionary = new Dictionary<BlueprintBuff, List<Buff>>();
		if (!(unit.Blueprint is BlueprintStarship))
		{
			list = list.Where((Buff b) => !b.Blueprint.IsStarshipBuff).ToList();
		}
		foreach (Buff item in list)
		{
			if (item.Blueprint.NeedCollapseStack)
			{
				dictionary.TryAdd(item.Blueprint, new List<Buff>());
			}
		}
		foreach (KeyValuePair<BlueprintBuff, List<Buff>> kvp in dictionary)
		{
			List<Buff> collection = list.Where((Buff b) => b.Blueprint == kvp.Key && !b.Blueprint.IsHiddenInUI).ToList();
			if (dictionary.TryGetValue(kvp.Key, out var value))
			{
				value.AddRange(collection);
			}
		}
		List<TooltipBrickBuff> list2 = list.Where((Buff b) => !b.Blueprint.IsHiddenInUI && !b.Blueprint.NeedCollapseStack).Select(delegate(Buff buff)
		{
			BuffUIGroup group2 = ((!buff.Blueprint.IsDOTVisual) ? (unit.IsEnemy(buff.Context.MaybeCaster) ? BuffUIGroup.Enemy : BuffUIGroup.Ally) : BuffUIGroup.DOT);
			return new TooltipBrickBuff(buff, group2);
		}).ToList();
		foreach (KeyValuePair<BlueprintBuff, List<Buff>> item2 in dictionary)
		{
			Buff buff2 = item2.Value.FirstOrDefault();
			if (buff2 != null)
			{
				BuffUIGroup group = ((!buff2.Blueprint.IsDOTVisual) ? (unit.IsEnemy(buff2.Context.MaybeCaster) ? BuffUIGroup.Enemy : BuffUIGroup.Ally) : BuffUIGroup.DOT);
				list2.Add(new TooltipBrickBuff(buff2, group, item2.Value));
			}
		}
		return list2;
	}

	public static ReactiveCollection<ITooltipBrick> GetBuffsTooltipBricks(BaseUnitEntity unit)
	{
		List<Buff> list = unit.Buffs.RawFacts;
		Dictionary<BlueprintBuff, List<Buff>> dictionary = new Dictionary<BlueprintBuff, List<Buff>>();
		if (!unit.IsStarship())
		{
			list = list.Where((Buff b) => !b.Blueprint.IsStarshipBuff).ToList();
		}
		foreach (Buff item in list)
		{
			if (item.Blueprint.NeedCollapseStack)
			{
				dictionary.TryAdd(item.Blueprint, new List<Buff>());
			}
		}
		foreach (KeyValuePair<BlueprintBuff, List<Buff>> kvp in dictionary)
		{
			List<Buff> collection = list.Where((Buff b) => b.Blueprint == kvp.Key && !b.Blueprint.IsHiddenInUI).ToList();
			if (dictionary.TryGetValue(kvp.Key, out var value))
			{
				value.AddRange(collection);
			}
		}
		ReactiveCollection<ITooltipBrick> reactiveCollection = list.Where((Buff b) => !b.Blueprint.IsHiddenInUI && !b.Blueprint.NeedCollapseStack).Select((Func<Buff, ITooltipBrick>)delegate(Buff buff)
		{
			BuffUIGroup group2 = ((!buff.Blueprint.IsDOTVisual) ? (unit.IsEnemy(buff.Context.MaybeCaster) ? BuffUIGroup.Enemy : BuffUIGroup.Ally) : BuffUIGroup.DOT);
			return new TooltipBrickBuff(buff, group2);
		}).ToReactiveCollection();
		foreach (KeyValuePair<BlueprintBuff, List<Buff>> item2 in dictionary)
		{
			Buff buff2 = item2.Value.FirstOrDefault();
			if (buff2 != null)
			{
				BuffUIGroup group = ((!buff2.Blueprint.IsDOTVisual) ? (unit.IsEnemy(buff2.Context.MaybeCaster) ? BuffUIGroup.Enemy : BuffUIGroup.Ally) : BuffUIGroup.DOT);
				reactiveCollection.Add(new TooltipBrickBuff(buff2, group, item2.Value));
			}
		}
		return reactiveCollection;
	}
}
