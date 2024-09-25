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
		List<Buff> source = unit.Buffs.RawFacts;
		if (!(unit.Blueprint is BlueprintStarship))
		{
			source = source.Where((Buff b) => !b.Blueprint.IsStarshipBuff).ToList();
		}
		return source.Where((Buff b) => !b.Blueprint.IsHiddenInUI).Select(delegate(Buff buff)
		{
			BuffUIGroup group = ((!buff.Blueprint.IsDOTVisual) ? (unit.IsEnemy(buff.Context.MaybeCaster) ? BuffUIGroup.Enemy : BuffUIGroup.Ally) : BuffUIGroup.DOT);
			return new TooltipBrickBuff(buff, group);
		}).ToList();
	}

	public static ReactiveCollection<ITooltipBrick> GetBuffsTooltipBricks(BaseUnitEntity unit)
	{
		List<Buff> source = unit.Buffs.RawFacts;
		if (!unit.IsStarship())
		{
			source = source.Where((Buff b) => !b.Blueprint.IsStarshipBuff).ToList();
		}
		return source.Where((Buff b) => !b.Blueprint.IsHiddenInUI).Select((Func<Buff, ITooltipBrick>)delegate(Buff buff)
		{
			BuffUIGroup group = ((!buff.Blueprint.IsDOTVisual) ? (unit.IsEnemy(buff.Context.MaybeCaster) ? BuffUIGroup.Enemy : BuffUIGroup.Ally) : BuffUIGroup.DOT);
			return new TooltipBrickBuff(buff, group);
		}).ToReactiveCollection();
	}
}
