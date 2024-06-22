using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateMomentum : TooltipBaseTemplate
{
	private readonly int m_MomentumValue;

	private readonly int m_MaxMomentum;

	public TooltipTemplateMomentum(int momentumValue)
	{
		m_MomentumValue = momentumValue;
		m_MaxMomentum = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot.MaximalMomentum;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string text = UIStrings.Instance.ActionBar.MomentumHeader.Text;
		int momentumValue = m_MomentumValue;
		list.Add(new TooltipBrickIconValueStat(text, momentumValue.ToString(), BlueprintRoot.Instance.UIConfig.UIIcons.TooltipIcons.Momentum, TooltipIconValueStatType.Normal, isWhite: true, needChangeSize: true, 26, 28));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (!(Game.Instance.TurnController.CurrentUnit is BaseUnitEntity baseUnitEntity))
		{
			return list;
		}
		int heroicActThreshold = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot.HeroicActThreshold;
		int desperateMeasureThreshold = baseUnitEntity.GetDesperateMeasureThreshold();
		BlueprintMomentumRoot momentumRoot = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
		list.Add(new TooltipBrickText(UIStrings.Instance.ActionBar.MomentumDescription.Text, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
		list.Add(new TooltipBrickSlider(m_MaxMomentum, m_MomentumValue, new List<BrickSliderValueVM>
		{
			new BrickSliderValueVM(m_MaxMomentum, desperateMeasureThreshold, null, needColor: true, UIConfig.Instance.TooltipColors.ProgressbarPenalty),
			new BrickSliderValueVM(m_MaxMomentum, heroicActThreshold, null, needColor: true, UIConfig.Instance.TooltipColors.ProgressbarNeutral)
		}, showValue: false, 50, UIConfig.Instance.TooltipColors.ProgressbarBonus));
		list.Add(new TooltipBrickIconValueStat(UIStrings.Instance.Tooltips.DesperateMeasureAbility.Text, $"<={desperateMeasureThreshold}", null, TooltipIconValueStatType.Normal, isWhite: false, needChangeSize: true));
		AddFeaturesBricks(baseUnitEntity, list, needMomentum: false);
		list.Add(new TooltipBrickIconValueStat(UIStrings.Instance.Tooltips.HeroicActAbility.Text, $">={heroicActThreshold}", null, TooltipIconValueStatType.Normal, isWhite: false, needChangeSize: true));
		AddFeaturesBricks(baseUnitEntity, list, needMomentum: true);
		list.Add(new TooltipBrickSeparator(TooltipBrickElementType.Medium));
		switch (type)
		{
		case TooltipTemplateType.Tooltip:
			AddTooltipMomentumPortrait(list, momentumRoot);
			break;
		case TooltipTemplateType.Info:
			AddInfoMomentumPortrait(list, momentumRoot);
			break;
		}
		return list;
	}

	private void AddTooltipMomentumPortrait(List<ITooltipBrick> bricks, BlueprintMomentumRoot root)
	{
		List<TooltipBrickMomentumPortrait> list = new List<TooltipBrickMomentumPortrait>();
		foreach (UnitReference unit in Game.Instance.Player.Group.Units)
		{
			bool enable = CanUseMomentumAbility(root, unit);
			list.Add(new TooltipBrickMomentumPortrait(unit.Entity.ToBaseUnitEntity().Portrait.SmallPortrait, enable));
		}
		bricks.Add(new TooltipsBrickMomentumPortraits(list));
	}

	private void AddInfoMomentumPortrait(List<ITooltipBrick> bricks, BlueprintMomentumRoot root)
	{
		foreach (UnitReference unit in Game.Instance.Player.Group.Units)
		{
			List<Ability> list = new List<Ability>();
			List<Ability> list2 = new List<Ability>();
			foreach (Ability ability in unit.Entity.ToBaseUnitEntity().Abilities)
			{
				if (!ability.Blueprint.Hidden)
				{
					if (ability.Blueprint.IsMomentum && ability.Blueprint.IsHeroicAct)
					{
						list2.Add(ability);
					}
					else if (ability.Blueprint.IsMomentum && ability.Blueprint.IsDesperateMeasure)
					{
						list.Add(ability);
					}
				}
			}
			bricks.Add(new TooltipBrickPortraitFeatures(unit.Entity.ToBaseUnitEntity().Name, CanUseMomentumAbility(root, unit), CanUseMomentumAbilityInInfoWindow(root, unit), unit.Entity.ToBaseUnitEntity().Portrait.SmallPortrait, list, list2));
		}
	}

	private bool CanUseMomentumAbility(BlueprintMomentumRoot root, UnitReference unit)
	{
		if (unit.Entity.ToBaseUnitEntity().Facts.Contains(root.DesperateMeasureBuff))
		{
			return false;
		}
		if (unit.Entity.ToBaseUnitEntity().Facts.Contains(root.HeroicActBuff))
		{
			return false;
		}
		return true;
	}

	private string CanUseMomentumAbilityInInfoWindow(BlueprintMomentumRoot root, UnitReference unit)
	{
		if (unit.Entity.ToBaseUnitEntity().Facts.Contains(root.DesperateMeasureBuff))
		{
			return UIStrings.Instance.Tooltips.MomentumNotAvailable;
		}
		if (unit.Entity.ToBaseUnitEntity().Facts.Contains(root.HeroicActBuff))
		{
			return UIStrings.Instance.Tooltips.MomentumNotAvailable;
		}
		return UIStrings.Instance.Tooltips.MomentumAvailable;
	}

	private void AddFeaturesBricks(BaseUnitEntity currentUnit, List<ITooltipBrick> bricks, bool needMomentum)
	{
		foreach (Ability ability in currentUnit.Abilities)
		{
			if ((ability.Blueprint.IsMomentum && ability.Blueprint.IsHeroicAct && needMomentum && !ability.Blueprint.Hidden) || (ability.Blueprint.IsDesperateMeasure && !needMomentum && !ability.Blueprint.Hidden))
			{
				UIUtilityItem.UIAbilityData uIAbilityData = UIUtilityItem.GetUIAbilityData(ability.Blueprint, ability.Data.Weapon);
				TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
				{
					Text = ability.Name,
					TextParams = new TextFieldParams
					{
						FontStyles = FontStyles.Bold
					}
				};
				AbilityMomentumLogic abilityMomentumLogic = ability.Blueprint.CasterRestrictions.FirstOrDefault((IAbilityCasterRestriction x) => x is AbilityMomentumLogic) as AbilityMomentumLogic;
				TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
				{
					Text = UIStrings.Instance.Tooltips.CostAP.Text + " " + ((abilityMomentumLogic != null) ? abilityMomentumLogic.Cost.ToString() : "0"),
					TextParams = new TextFieldParams
					{
						FontStyles = FontStyles.Bold
					}
				};
				bricks.Add(new TooltipBrickIconPattern(ability.Icon, uIAbilityData.PatternData, titleValues, secondaryValues));
				break;
			}
		}
	}
}
