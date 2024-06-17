using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Inspect;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateUnitInspectShort : TooltipBaseTemplate
{
	private readonly RuleCalculateStatsArmor m_StatsArmor;

	private readonly MechanicEntityUIWrapper m_UnitUIWrapper;

	private readonly BaseUnitEntity m_Unit;

	public TooltipTemplateUnitInspectShort(BaseUnitEntity unit)
	{
		try
		{
			if (unit != null)
			{
				m_Unit = unit;
				m_UnitUIWrapper = new MechanicEntityUIWrapper(m_Unit);
				m_StatsArmor = Rulebook.Trigger(new RuleCalculateStatsArmor(m_Unit));
				Game.Instance.Player.InspectUnitsManager.ForceRevealUnitInfo(m_Unit);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {unit?.Blueprint?.name}: {arg}");
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (m_Unit == null)
		{
			yield return new TooltipBrickText(UIStrings.Instance.Tooltips.UnitIsNotInspected);
			yield break;
		}
		BlueprintArmyDescription army = m_Unit.Blueprint.Army;
		string title = string.Empty;
		if (army?.IsDaemon ?? false)
		{
			title = UIStrings.Instance.CharacterSheet.Chaos;
		}
		if (army?.IsXenos ?? false)
		{
			title = UIStrings.Instance.CharacterSheet.Xenos;
		}
		if (army?.IsHuman ?? false)
		{
			title = UIStrings.Instance.CharacterSheet.Human;
		}
		Sprite surfaceCombatStandardPortrait = UIUtilityUnit.GetSurfaceCombatStandardPortrait(m_Unit, UIUtilityUnit.PortraitCombatSize.Small);
		yield return new TooltipBrickPortraitAndName(surfaceCombatStandardPortrait, m_Unit.CharacterName, new TooltipBrickTitle(title, TooltipTitleType.H6, TextAlignmentOptions.Left), (!m_Unit.IsInPlayerParty) ? UIUtilityUnit.GetSurfaceEnemyDifficulty(m_Unit) : 0, UIUtilityUnit.UsedSubtypeIcon(m_Unit), m_Unit.IsPlayerEnemy, !m_Unit.IsInPlayerParty && !m_Unit.IsPlayerEnemy);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> result = new List<ITooltipBrick>();
		if (m_Unit == null)
		{
			return result;
		}
		GetTooltipBody(result);
		return result;
	}

	private void GetTooltipBody(List<ITooltipBrick> result)
	{
		AddWounds(result);
		AddDamageDeflection(result);
		AddArmorAbsorption(result);
		AddDodge(result);
		AddMovePoints(result);
		AddBuffsAndStatusEffects(result);
	}

	private void AddWounds(List<ITooltipBrick> bricks)
	{
		if (InspectExtensions.TryGetWoundsText(m_UnitUIWrapper, out var woundsValue, out var woundsAddValue))
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Wounds.Text, woundsValue, woundsAddValue, UIConfig.Instance.UIIcons.TooltipInspectIcons.Wounds, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Normal, null, new TooltipTemplateGlossary("HitPoints")));
		}
	}

	private void AddDamageDeflection(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.DamageDeflection.Text, InspectExtensions.GetDeflection(m_Unit), null, tooltip: new TooltipTemplateDeflection(m_StatsArmor), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.DamageDeflection));
	}

	private void AddArmorAbsorption(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Armor.Text, InspectExtensions.GetArmor(m_Unit), null, tooltip: new TooltipTemplateAbsorption(m_StatsArmor), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Armor));
	}

	private void AddDodge(List<ITooltipBrick> bricks)
	{
		RuleCalculateDodgeChance dodgeRule = Rulebook.Trigger(new RuleCalculateDodgeChance((UnitEntity)m_Unit));
		bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Dodge.Text, InspectExtensions.GetDodge(m_Unit), null, tooltip: new TooltipTemplateDodge(dodgeRule), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Dodge));
	}

	private void AddMovePoints(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.MovePoints.Text, InspectExtensions.GetMovementPoints(m_Unit), null, tooltip: new TooltipTemplateGlossary("MovementPoints"), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.MovePoints));
	}

	private void AddBuffsAndStatusEffects(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.StatusEffectsTitle.Text));
		ReactiveCollection<ITooltipBrick> buffsTooltipBricks = InspectExtensions.GetBuffsTooltipBricks(m_Unit);
		bricks.Add(new TooltipBrickWidget(buffsTooltipBricks, new TooltipBrickText(UIStrings.Instance.Inspect.NoStatusEffects.Text, TooltipTextType.Simple | TooltipTextType.BrightColor)));
	}
}
