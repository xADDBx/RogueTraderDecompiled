using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateBuff : TooltipBaseTemplate
{
	public readonly Buff Buff;

	public readonly BlueprintBuff BlueprintBuff;

	private EntityRef m_OverrideCaster;

	private string m_Name;

	private string m_Desc;

	private Sprite m_Icon;

	private readonly string m_Stacking;

	public override void Prepare(TooltipTemplateType type)
	{
		if (Buff != null)
		{
			FillAbilityDataInfo(Buff);
		}
		else if (BlueprintBuff != null)
		{
			FillBlueprintAbilityData(BlueprintBuff);
		}
	}

	private void FillBlueprintAbilityData(BlueprintBuff blueprintBuff)
	{
		try
		{
			if (blueprintBuff != null)
			{
				m_Name = blueprintBuff.Name;
				m_Desc = blueprintBuff.Description;
				m_Icon = blueprintBuff.Icon;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {blueprintBuff?.name}: {arg}");
		}
	}

	private void FillAbilityDataInfo(Buff buff)
	{
		try
		{
			if (buff == null)
			{
				return;
			}
			using (GameLogContext.Scope)
			{
				GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)((IBuff)Buff).Caster;
				m_Name = buff.Name;
				m_Desc = buff.Description;
				m_Icon = buff.Icon;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {buff?.Blueprint?.name}: {arg}");
		}
	}

	public TooltipTemplateBuff(Buff buff, IEntity overrideCaster = null)
	{
		Buff = buff;
		m_OverrideCaster = new EntityRef(overrideCaster);
	}

	public TooltipTemplateBuff(BlueprintBuff blueprintBuff)
	{
		BlueprintBuff = blueprintBuff;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = m_Name,
			TextParams = new TextFieldParams
			{
				FontStyles = TMPro.FontStyles.Bold
			}
		};
		TooltipBrickIconPattern.TextFieldValues secondaryValues = null;
		if (Buff != null && Buff.Blueprint.HasRanks && Buff.Rank > 0)
		{
			secondaryValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = string.Format(UIStrings.Instance.CommonTexts.BuffStacks, Buff.Rank, Buff.Blueprint.MaxRank),
				TextParams = new TextFieldParams()
			};
		}
		yield return new TooltipBrickIconPattern(m_Icon, null, titleValues, secondaryValues);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDOT(list);
		AddSource(list);
		AddDuration(list);
		AddStacking(list);
		AddDescription(list);
		AddNonStackBonus(list);
		return list;
	}

	private void AddDuration(List<ITooltipBrick> bricks)
	{
		if (Buff != null)
		{
			string duration = BuffTooltipUtils.GetDuration(Buff);
			if (!string.IsNullOrEmpty(duration))
			{
				TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
				{
					Text = UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Duration)
				};
				TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
				{
					Text = UIUtilityTexts.WrapWithWeight(duration, TextFontWeight.SemiBold)
				};
				bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Duration, null, titleValues, secondaryValues));
			}
		}
	}

	private void AddStacking(List<ITooltipBrick> bricks)
	{
		if (Buff != null && m_Stacking != null)
		{
			bricks.Add(new TooltipBrickText(m_Stacking));
		}
	}

	private void AddDescription(List<ITooltipBrick> bricks)
	{
		if (Buff == null)
		{
			bricks.Add(new TooltipBrickText(m_Desc, TooltipTextType.Paragraph));
		}
		else
		{
			bricks.Add(new TooltipBrickText(UIUtilityTexts.UpdateDescriptionWithUIProperties(m_Desc, ((IBuff)Buff).Caster), TooltipTextType.Paragraph));
		}
	}

	private void AddNonStackBonus(List<ITooltipBrick> bricks)
	{
		UnitPartNonStackBonuses unitPartNonStackBonuses = Buff?.Owner?.GetOptional<UnitPartNonStackBonuses>();
		if (unitPartNonStackBonuses != null && unitPartNonStackBonuses.ShouldShowWarning(Buff))
		{
			bricks.Add(new TooltipBrickNonStack(unitPartNonStackBonuses));
		}
	}

	private void AddSource(List<ITooltipBrick> bricks)
	{
		if (Buff == null)
		{
			return;
		}
		ITooltipBrick tooltipBrick = null;
		if (Buff?.SourceAbilityBlueprint != null)
		{
			tooltipBrick = new TooltipBrickIconPattern(Buff.SourceAbilityBlueprint.Icon, null, UIStrings.Instance.Tooltips.Source, Buff.SourceAbilityBlueprint.Name);
		}
		if (Buff?.SourceFact != null)
		{
			BlueprintBuff blueprintBuff = (BlueprintBuff)Buff.SourceFact.Blueprint;
			if (blueprintBuff == null || !blueprintBuff.IsHiddenInUI)
			{
				tooltipBrick = new TooltipBrickIconPattern(Buff.SourceFact.Icon, null, UIStrings.Instance.Tooltips.Source, Buff.SourceFact.Name);
			}
		}
		if (Buff?.SourceItem != null)
		{
			tooltipBrick = new TooltipBrickIconPattern(Buff.SourceItem.ToItemEntity().Icon, null, UIStrings.Instance.Tooltips.Source, Buff.SourceItem.ToItemEntity().Name);
		}
		if (tooltipBrick == null && m_OverrideCaster.Entity is BaseUnitEntity baseUnitEntity)
		{
			tooltipBrick = new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Source, null, UIStrings.Instance.Tooltips.Source, baseUnitEntity.CharacterName);
		}
		if (tooltipBrick == null && Buff?.Context?.MaybeCaster is BaseUnitEntity baseUnitEntity2)
		{
			tooltipBrick = new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Source, null, UIStrings.Instance.Tooltips.Source, baseUnitEntity2.CharacterName);
		}
		if (tooltipBrick != null)
		{
			bricks.Add(tooltipBrick);
		}
	}

	private void AddDOT(List<ITooltipBrick> bricks)
	{
		if (Buff?.Blueprint?.GetComponent<DOTLogicVisual>() == null)
		{
			return;
		}
		TooltipBrickStrings tooltipBrickStrings = GameLogStrings.Instance.TooltipBrickStrings;
		DamageData damageData = DOTLogicUIExtensions.CalculateDOTDamage(Buff);
		bricks.Add(new TooltipBrickDamageRange(tooltipBrickStrings.Damage.Text, damageData.AverageValue, damageData.MinValue, damageData.MaxValue, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: false, isBeigeBackground: false, isRedBackground: true));
		string value = damageData.MinValueBase + " — " + damageData.MaxValueBase;
		TooltipBrickTextValue item = new TooltipBrickTextValue(tooltipBrickStrings.BaseModifier.Text, value, 2, isResultValue: true);
		bricks.Add(item);
		foreach (ITooltipBrick damageModifier in LogThreadBase.GetDamageModifiers(damageData, 2, minMax: true, common: true))
		{
			bricks.Add(damageModifier);
		}
	}
}
