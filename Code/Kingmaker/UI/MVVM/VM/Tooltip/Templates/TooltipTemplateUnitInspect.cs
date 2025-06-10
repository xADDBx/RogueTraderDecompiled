using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Inspect;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Block;
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
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_Unit == null)
		{
			list.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.UnitIsNotInspected));
			return list;
		}
		string unitTypeTitle = GetUnitTypeTitle(m_Unit);
		TooltipBrickPortraitAndName item = new TooltipBrickPortraitAndName(UIUtilityUnit.GetSurfaceCombatStandardPortrait(m_Unit, UIUtilityUnit.PortraitCombatSize.Small), m_Unit.CharacterName, new TooltipBrickTitle(unitTypeTitle, TooltipTitleType.H6, TextAlignmentOptions.Left), (!m_Unit.IsInPlayerParty) ? UIUtilityUnit.GetSurfaceEnemyDifficulty(m_Unit) : 0, UIUtilityUnit.UsedSubtypeIcon(m_Unit), m_Unit.IsPlayerEnemy, !m_Unit.IsInPlayerParty && !m_Unit.IsPlayerEnemy);
		list.Add(item);
		UnitPartPetOwner unitPartPetOwner = ((!m_Unit.IsPet) ? m_Unit.GetOptional<UnitPartPetOwner>() : null);
		if (m_Unit.IsPet)
		{
			TooltipBrickOverseerPaper item2 = new TooltipBrickOverseerPaper(UIUtilityUnit.GetSurfaceCombatStandardPortrait(m_Unit.Master, UIUtilityUnit.PortraitCombatSize.Small), m_Unit.Master.CharacterName, GetUnitTypeTitle(m_Unit.Master), m_Unit.Master);
			list.Add(item2);
			return list;
		}
		if (unitPartPetOwner != null)
		{
			TooltipBrickOverseerPaper item3 = new TooltipBrickOverseerPaper(UIUtilityUnit.GetSurfaceCombatStandardPortrait(unitPartPetOwner.PetUnit, UIUtilityUnit.PortraitCombatSize.Small), unitPartPetOwner.PetUnit.CharacterName, GetUnitTypeTitle(unitPartPetOwner.PetUnit), unitPartPetOwner.PetUnit);
			list.Add(item3);
			return list;
		}
		return list;
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
		AddBlockChance(result);
		result.Add(new TooltipBrickSpace(2f));
		result.Add(new TooltipBrickAbilityScoresBlock(m_UnitReactiveProperty));
		result.Add(new TooltipBrickSpace(2f));
		AddBuffsAndStatusEffects(result);
		result.Add(new TooltipBrickSpace(2f));
		AddWeapon(result);
		result.Add(new TooltipBrickSpace(2f));
		AddProtocols(result);
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

	private void AddProtocols(List<ITooltipBrick> result)
	{
		if (m_Unit.IsPet && m_Unit.Body.PetProtocol != null && m_Unit.Body.PetProtocol.HasItem)
		{
			result.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.ProtocolTitle, TooltipTitleType.H1));
			result.Add(new TooltipBrickProtocolPet(m_Unit.Body.PetProtocol));
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

	private void AddBlockChance(List<ITooltipBrick> bricks)
	{
		if (m_Unit == null || m_Unit.Items == null)
		{
			return;
		}
		ItemEntityShield maybeShield = m_Unit.Body.CurrentHandsEquipmentSet.PrimaryHand.MaybeShield;
		ItemEntityShield maybeShield2 = m_Unit.Body.CurrentHandsEquipmentSet.SecondaryHand.MaybeShield;
		if (maybeShield != null || maybeShield2 != null)
		{
			if (maybeShield != null)
			{
				int result = Rulebook.Trigger(new RuleCalculateBlockChance((UnitEntity)m_Unit, maybeShield.Blueprint.BlockChance)).Result;
				bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.BlockStrings.BaseBlockChance, $"{result}%", null, tooltip: new TooltipTemplateGlossary("BlockChance"), icon: BlueprintRoot.Instance.UIConfig.UIIcons.TooltipInspectIcons.CoverMagnitude));
			}
			if (maybeShield2 != null)
			{
				int result2 = Rulebook.Trigger(new RuleCalculateBlockChance((UnitEntity)m_Unit, maybeShield2.Blueprint.BlockChance)).Result;
				bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.BlockStrings.BaseBlockChance, $"{result2}%", null, tooltip: new TooltipTemplateGlossary("BlockChance"), icon: BlueprintRoot.Instance.UIConfig.UIIcons.TooltipInspectIcons.CoverMagnitude));
			}
		}
	}

	private void AddBuffsAndStatusEffects(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.StatusEffectsTitle.Text));
		ReactiveCollection<ITooltipBrick> source = ((m_InspectReactiveData != null) ? m_InspectReactiveData.TooltipBrickBuffs : InspectExtensions.GetBuffsTooltipBricks(m_Unit));
		ReactiveCollection<ITooltipBrick> buffs = source.Where((ITooltipBrick b) => ((TooltipBrickBuff)b).Group == BuffUIGroup.DOT).ToReactiveCollection();
		ReactiveCollection<ITooltipBrick> buffs2 = source.Where((ITooltipBrick b) => ((TooltipBrickBuff)b).Group == BuffUIGroup.Ally).ToReactiveCollection();
		ReactiveCollection<ITooltipBrick> buffs3 = source.Where((ITooltipBrick b) => ((TooltipBrickBuff)b).Group == BuffUIGroup.Enemy).ToReactiveCollection();
		AddBuffsAndStatusEffectsGroup(bricks, UIStrings.Instance.Inspect.EffectsDOT.Text, buffs);
		UpdateUnitWrapper();
		if (m_UnitUIWrapper.IsPlayerFaction)
		{
			AddBuffsAndStatusEffectsGroup(bricks, UIStrings.Instance.Inspect.EffectsEnemy.Text, buffs3);
			AddBuffsAndStatusEffectsGroup(bricks, UIStrings.Instance.Inspect.EffectsAlly.Text, buffs2);
		}
		else
		{
			AddBuffsAndStatusEffectsGroup(bricks, UIStrings.Instance.Inspect.EffectsAlly.Text, buffs2);
			AddBuffsAndStatusEffectsGroup(bricks, UIStrings.Instance.Inspect.EffectsEnemy.Text, buffs3);
		}
	}

	private void AddBuffsAndStatusEffectsGroup(List<ITooltipBrick> bricks, string title, ReactiveCollection<ITooltipBrick> buffs)
	{
		bricks.Add(new TooltipBrickTitle(title, TooltipTitleType.H2));
		bricks.Add(new TooltipBrickWidget(buffs, new TooltipBrickText(UIStrings.Instance.Inspect.NoStatusEffects.Text, TooltipTextType.Simple | TooltipTextType.BrightColor, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true, 16)));
	}

	protected void AddWeapon(List<ITooltipBrick> bricks)
	{
		IList<HandsEquipmentSet> handsEquipmentSets = m_Unit.Body.HandsEquipmentSets;
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (!handsEquipmentSets.Empty())
		{
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
		}
		List<WeaponSlot> additionalLimbs = m_Unit.Body.AdditionalLimbs;
		if (additionalLimbs != null)
		{
			foreach (WeaponSlot item2 in additionalLimbs)
			{
				if (item2 != null && item2.HasWeapon)
				{
					if (list.Count > 0)
					{
						list.Add(new TooltipBrickSpace(2f));
					}
					list.Add(new TooltipBrickWeaponSet(item2));
				}
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
				bool isHidden = blueprintAbility.CultAmbushVisibility(m_Unit) == UnitPartCultAmbush.VisibilityStatuses.NotVisible;
				bricks.Add(new TooltipBrickFeature(blueprintAbility, isHeader: false, m_Unit, isHidden));
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
				if (!string.IsNullOrEmpty(feature.Name) && !feature.HideInUI)
				{
					bool isHidden = feature.CultAmbushVisibility(m_Unit) == UnitPartCultAmbush.VisibilityStatuses.NotVisible;
					bricks.Add(new TooltipBrickFeature(feature, isHeader: false, available: true, showIcon: true, m_Unit, forceSetName: false, isHidden));
				}
			}
			break;
		}
		}
	}

	private string GetUnitTypeTitle(BaseUnitEntity unit)
	{
		string result = string.Empty;
		if (!unit.IsPet)
		{
			BlueprintArmyDescription army = unit.Blueprint.Army;
			if (army != null && army.IsDaemon)
			{
				result = UIStrings.Instance.CharacterSheet.Chaos;
			}
			if (army != null && army.IsXenos)
			{
				result = UIStrings.Instance.CharacterSheet.Xenos;
			}
			if (army != null && army.IsHuman)
			{
				result = UIStrings.Instance.CharacterSheet.Human;
			}
		}
		else
		{
			UnitPartPetOwner unitPartPetOwner = (unit.IsPet ? unit.Master.GetOptional<UnitPartPetOwner>() : null);
			if (unitPartPetOwner != null)
			{
				switch (unitPartPetOwner.PetType)
				{
				case PetType.Mastiff:
					result = UIStrings.Instance.Pets.MastiffPetType;
					break;
				case PetType.Raven:
					result = UIStrings.Instance.Pets.RavenPetType;
					break;
				case PetType.ServoskullSwarm:
					result = UIStrings.Instance.Pets.SkullPetType;
					break;
				case PetType.Eagle:
					result = UIStrings.Instance.Pets.EaglePetType;
					break;
				}
			}
		}
		return result;
	}
}
