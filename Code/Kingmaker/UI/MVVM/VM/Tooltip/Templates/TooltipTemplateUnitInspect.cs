using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Inspect;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Inspect;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateUnitInspect : TooltipBaseTemplate
{
	private readonly IReadOnlyReactiveProperty<BaseUnitEntity> m_UnitReactiveProperty;

	private readonly BaseUnitEntity m_Unit;

	private readonly UnitInspectInfoByPart m_InspectInfo;

	private readonly RuleCalculateStatsArmor m_StatsArmor;

	private MechanicEntityUIWrapper m_UnitUIWrapper;

	private readonly InspectReactiveData m_InspectReactiveData;

	public TooltipTemplateUnitInspect(BaseUnitEntity unit)
	{
		try
		{
			if (unit != null)
			{
				m_Unit = unit;
				m_StatsArmor = Rulebook.Trigger(new RuleCalculateStatsArmor(m_Unit));
				Game.Instance.Player.InspectUnitsManager.ForceRevealUnitInfo(m_Unit);
				m_InspectInfo = InspectUnitsHelper.GetInfo(m_Unit.BlueprintForInspection, force: true);
				m_Unit.GetOptional<UnitPartInspectedBuffs>()?.GetBuffs(m_InspectInfo);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {unit?.Blueprint?.name}: {arg}");
		}
	}

	public TooltipTemplateUnitInspect(IReadOnlyReactiveProperty<BaseUnitEntity> unit, InspectReactiveData inspectReactiveData)
		: this(unit.Value)
	{
		m_UnitReactiveProperty = unit;
		m_InspectReactiveData = inspectReactiveData;
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
		if (type == TooltipTemplateType.Tooltip)
		{
			GetTooltipBody(result);
		}
		else
		{
			GetInfoBody(result);
		}
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

	private void GetInfoBody(List<ITooltipBrick> result)
	{
		AddWounds(result);
		AddDamageDeflection(result);
		AddArmorAbsorption(result);
		AddDodge(result);
		AddMovePoints(result);
		result.Add(new TooltipBrickSpace(2f));
		result.Add(new TooltipBrickAbilityScoresBlock(m_UnitReactiveProperty));
		result.Add(new TooltipBrickSpace(2f));
		AddBuffsAndStatusEffects(result);
		result.Add(new TooltipBrickSpace(2f));
		AddWeapon(result);
		result.Add(new TooltipBrickSpace(2f));
		result.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.AbilitiesTitle));
		bool flag = false;
		BlueprintAbility[] array = (from a in UIUtilityUnit.CollectAbilities(m_Unit)
			where !a.Blueprint.IsMomentum
			select a.Blueprint).ToArray();
		if (array.Any())
		{
			flag = true;
			result.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.ActiveAbilitiesTitle, TooltipTitleType.H5));
			AddAbilities(result, array, TooltipTemplateType.Info);
		}
		if (m_Unit.IsInPlayerParty)
		{
			List<BlueprintAbility> list = new List<BlueprintAbility>();
			foreach (Ability ability in m_Unit.Abilities)
			{
				if ((ability.Blueprint.IsMomentum && ability.Blueprint.IsHeroicAct && !ability.Blueprint.Hidden) || (ability.Blueprint.IsDesperateMeasure && !ability.Blueprint.Hidden))
				{
					list.Add(ability.Blueprint);
				}
			}
			if (list.Any())
			{
				flag = true;
				result.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.UltimateAbilitiesTitle, TooltipTitleType.H5));
				AddAbilities(result, list.ToArray(), TooltipTemplateType.Info);
			}
		}
		UIFeature[] array2 = UIUtilityUnit.CollectFeats(m_Unit).ToArray();
		if (array2.Any())
		{
			flag = true;
			result.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.PassiveAbilitiesTitle, TooltipTitleType.H5));
			FeatureUIData[] features = array2;
			AddFeatures(result, features, TooltipTemplateType.Info);
		}
		if (!flag)
		{
			result.Add(new TooltipBrickText(UIStrings.Instance.Inspect.NoAbilities.Text, TooltipTextType.Simple | TooltipTextType.BrightColor, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true, 16));
		}
	}

	private void UpdateUnitWrapper()
	{
		if (m_UnitUIWrapper.MechanicEntity != m_Unit)
		{
			m_UnitUIWrapper = new MechanicEntityUIWrapper(m_Unit);
		}
	}

	private void AddWounds(List<ITooltipBrick> bricks)
	{
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Wounds.Text, m_InspectReactiveData.WoundsValue.Value, m_InspectReactiveData.WoundsAddValue.Value, UIConfig.Instance.UIIcons.TooltipInspectIcons.Wounds, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Normal, null, new TooltipTemplateGlossary("HitPoints"), m_InspectReactiveData.WoundsValue, m_InspectReactiveData.WoundsAddValue));
			return;
		}
		UpdateUnitWrapper();
		if (InspectExtensions.TryGetWoundsText(m_UnitUIWrapper, out var woundsValue, out var woundsAddValue))
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Wounds.Text, woundsValue, woundsAddValue, UIConfig.Instance.UIIcons.TooltipInspectIcons.Wounds, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Normal, null, new TooltipTemplateGlossary("HitPoints")));
		}
	}

	private void AddDamageDeflection(List<ITooltipBrick> bricks)
	{
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.DamageDeflection.Text, m_InspectReactiveData.DeflectionValue.Value, null, tooltip: new TooltipTemplateDeflection(m_StatsArmor), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.DamageDeflection, type: TooltipBrickIconStatValueType.Normal, backgroundType: TooltipBrickIconStatValueType.Normal, style: TooltipBrickIconStatValueStyle.Normal, valueHint: null, reactiveValue: m_InspectReactiveData.DeflectionValue));
		}
		else
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.DamageDeflection.Text, InspectExtensions.GetDeflection(m_Unit), null, tooltip: new TooltipTemplateDeflection(m_StatsArmor), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.DamageDeflection));
		}
	}

	private void AddArmorAbsorption(List<ITooltipBrick> bricks)
	{
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Armor.Text, m_InspectReactiveData.ArmorValue.Value, null, tooltip: new TooltipTemplateAbsorption(m_StatsArmor), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Armor, type: TooltipBrickIconStatValueType.Normal, backgroundType: TooltipBrickIconStatValueType.Normal, style: TooltipBrickIconStatValueStyle.Normal, valueHint: null, reactiveValue: m_InspectReactiveData.ArmorValue));
		}
		else
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Armor.Text, InspectExtensions.GetArmor(m_Unit), null, tooltip: new TooltipTemplateAbsorption(m_StatsArmor), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Armor));
		}
	}

	private void AddDodge(List<ITooltipBrick> bricks)
	{
		RuleCalculateDodgeChance dodgeRule = Rulebook.Trigger(new RuleCalculateDodgeChance((UnitEntity)m_Unit));
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Dodge.Text, m_InspectReactiveData.DodgeValue.Value, null, tooltip: new TooltipTemplateDodge(dodgeRule), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Dodge, type: TooltipBrickIconStatValueType.Normal, backgroundType: TooltipBrickIconStatValueType.Normal, style: TooltipBrickIconStatValueStyle.Normal, valueHint: null, reactiveValue: m_InspectReactiveData.DodgeValue));
		}
		else
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.Dodge.Text, InspectExtensions.GetDodge(m_Unit), null, tooltip: new TooltipTemplateDodge(dodgeRule), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Dodge));
		}
	}

	private void AddMovePoints(List<ITooltipBrick> bricks)
	{
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.MovePoints.Text, m_InspectReactiveData.MovementPointsValue.Value, null, tooltip: new TooltipTemplateGlossary("MovementPoints"), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.MovePoints, type: TooltipBrickIconStatValueType.Normal, backgroundType: TooltipBrickIconStatValueType.Normal, style: TooltipBrickIconStatValueStyle.Normal, valueHint: null, reactiveValue: m_InspectReactiveData.MovementPointsValue));
		}
		else
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Inspect.MovePoints.Text, InspectExtensions.GetMovementPoints(m_Unit), null, tooltip: new TooltipTemplateGlossary("MovementPoints"), icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.MovePoints));
		}
	}

	private void AddBuffsAndStatusEffects(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.StatusEffectsTitle.Text));
		if (m_InspectReactiveData != null)
		{
			ReactiveCollection<ITooltipBrick> tooltipBrickBuffs = m_InspectReactiveData.TooltipBrickBuffs;
			bricks.Add(new TooltipBrickWidget(tooltipBrickBuffs, new TooltipBrickText(UIStrings.Instance.Inspect.NoStatusEffects.Text, TooltipTextType.Simple | TooltipTextType.BrightColor, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true, 16)));
		}
		else
		{
			ReactiveCollection<ITooltipBrick> buffsTooltipBricks = InspectExtensions.GetBuffsTooltipBricks(m_Unit);
			bricks.Add(new TooltipBrickWidget(buffsTooltipBricks, new TooltipBrickText(UIStrings.Instance.Inspect.NoStatusEffects.Text, TooltipTextType.Simple | TooltipTextType.BrightColor, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true, 16)));
		}
	}

	protected void AddWeapon(List<ITooltipBrick> bricks)
	{
		IList<HandsEquipmentSet> handsEquipmentSets = m_Unit.Body.HandsEquipmentSets;
		if (handsEquipmentSets.Empty())
		{
			return;
		}
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		foreach (HandsEquipmentSet item in handsEquipmentSets)
		{
			HandSlot primaryHand = item.PrimaryHand;
			if (primaryHand != null && primaryHand.HasWeapon)
			{
				list.Add(new TooltipBrickWeaponSet(item.PrimaryHand, isPrimary: true));
			}
			primaryHand = item.SecondaryHand;
			if (primaryHand != null && primaryHand.HasWeapon)
			{
				if (list.Count > 0)
				{
					list.Add(new TooltipBrickSpace(2f));
				}
				list.Add(new TooltipBrickWeaponSet(item.SecondaryHand, isPrimary: false));
			}
		}
		if (list.Count > 0)
		{
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.WeaponsTitle));
			bricks.AddRange(list);
		}
	}

	protected void AddAbilities(List<ITooltipBrick> bricks, BlueprintAbility[] abilities, TooltipTemplateType type)
	{
		if (abilities.Empty())
		{
			return;
		}
		if (!(m_Unit.Blueprint is BlueprintStarship))
		{
			abilities = abilities.Where((BlueprintAbility a) => !a.IsStarshipAbility).ToArray();
		}
		switch (type)
		{
		case TooltipTemplateType.Tooltip:
		{
			StringBuilder stringBuilder = new StringBuilder();
			BlueprintAbility[] array = abilities;
			foreach (BlueprintAbility blueprintAbility2 in array)
			{
				if (!string.IsNullOrEmpty(blueprintAbility2.Name))
				{
					UIUtilityTexts.TryAddWordSeparator(stringBuilder, ", ");
					stringBuilder.Append(blueprintAbility2.Name);
				}
			}
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Replace(" ,", ",");
				bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
				bricks.Add(new TooltipBrickText(stringBuilder.ToString(), TooltipTextType.Bold));
			}
			break;
		}
		case TooltipTemplateType.Info:
		{
			BlueprintAbility[] array = abilities;
			foreach (BlueprintAbility blueprintAbility in array)
			{
				if (blueprintAbility.CultAmbushVisibility(m_Unit) != 0)
				{
					bricks.Add(new TooltipBrickFeature(blueprintAbility, isHeader: false, m_Unit));
				}
			}
			break;
		}
		}
	}

	protected void AddFeatures(List<ITooltipBrick> bricks, FeatureUIData[] features, TooltipTemplateType type)
	{
		if (features.Empty())
		{
			return;
		}
		if (!(m_Unit.Blueprint is BlueprintStarship))
		{
			features = features.Where((FeatureUIData f) => !f.Feature.IsStarshipFeature).ToArray();
		}
		switch (type)
		{
		case TooltipTemplateType.Tooltip:
		{
			StringBuilder stringBuilder = new StringBuilder();
			FeatureUIData[] array = features;
			foreach (FeatureUIData featureUIData in array)
			{
				if (!string.IsNullOrEmpty(featureUIData.Name) && !featureUIData.Feature.HideInUI)
				{
					UIUtilityTexts.TryAddWordSeparator(stringBuilder, ", ");
					stringBuilder.Append(featureUIData.Name);
				}
			}
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Replace(" ,", ",");
				bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
				bricks.Add(new TooltipBrickText(stringBuilder.ToString(), TooltipTextType.Bold));
			}
			break;
		}
		case TooltipTemplateType.Info:
		{
			FeatureUIData[] array = features;
			for (int i = 0; i < array.Length; i++)
			{
				BlueprintFeature feature = array[i].Feature;
				if (!string.IsNullOrEmpty(feature.Name) && !feature.HideInUI && feature.CultAmbushVisibility(m_Unit) != 0)
				{
					bricks.Add(new TooltipBrickFeature(feature, isHeader: false, available: true, showIcon: true, m_Unit));
				}
			}
			break;
		}
		}
	}
}
