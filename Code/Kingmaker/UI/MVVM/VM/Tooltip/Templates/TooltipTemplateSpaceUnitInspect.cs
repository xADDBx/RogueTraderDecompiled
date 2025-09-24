using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Inspect;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Inspect;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateSpaceUnitInspect : TooltipBaseTemplate
{
	protected readonly IReadOnlyReactiveProperty<StarshipEntity> UnitReactiveProperty;

	private readonly StarshipEntity m_Ship;

	private readonly UnitInspectInfoByPart m_InspectInfo;

	private InspectReactiveData m_InspectReactiveData;

	public TooltipTemplateSpaceUnitInspect(BaseUnitEntity unit)
	{
		m_Ship = (StarshipEntity)unit;
		if (m_Ship != null)
		{
			Game.Instance.Player.InspectUnitsManager.ForceRevealUnitInfo(m_Ship);
			m_InspectInfo = InspectUnitsHelper.GetInfo(m_Ship.BlueprintForInspection, force: true);
			m_Ship.GetOptional<UnitPartInspectedBuffs>()?.GetBuffs(m_InspectInfo);
		}
	}

	public TooltipTemplateSpaceUnitInspect(IReadOnlyReactiveProperty<StarshipEntity> unit, InspectReactiveData inspectReactiveData)
		: this(unit.Value)
	{
		m_Ship = (StarshipEntity)unit;
		if (m_Ship != null)
		{
			UnitReactiveProperty = unit;
			m_InspectReactiveData = inspectReactiveData;
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (m_Ship == null)
		{
			yield return new TooltipBrickText(UIStrings.Instance.Tooltips.UnitIsNotInspected);
			yield break;
		}
		BlueprintArmyDescription army = m_Ship.Blueprint.Army;
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
		Sprite surfaceCombatStandardPortrait = UIUtilityUnit.GetSurfaceCombatStandardPortrait(m_Ship, UIUtilityUnit.PortraitCombatSize.Small);
		Experience component = m_Ship.Blueprint.GetComponent<Experience>();
		int num = ((component == null) ? 1 : (component.CR + 1));
		yield return new TooltipBrickPortraitAndName(surfaceCombatStandardPortrait, m_Ship.CharacterName, new TooltipBrickTitle(title, TooltipTitleType.H6, TextAlignmentOptions.Left), (!m_Ship.IsInPlayerParty) ? num : 0, UIUtilityUnit.UsedSubtypeIcon(m_Ship), m_Ship.IsPlayerEnemy, !m_Ship.IsInPlayerParty && !m_Ship.IsPlayerEnemy);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> result = new List<ITooltipBrick>();
		if (m_Ship == null)
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
		AddHP(result);
		AddEvasion(result);
		HitAndCritChance(result);
		AddArmoursAndShields(result);
		AddBuffsAndStatusEffects(result);
	}

	private void GetInfoBody(List<ITooltipBrick> result)
	{
		AddHP(result);
		AddEvasion(result);
		HitAndCritChance(result);
		AddArmoursAndShields(result);
		AddBuffsAndStatusEffects(result);
		result.Add(new TooltipBrickSpace(2f));
		AddWeapon(result);
		result.Add(new TooltipBrickSpace(2f));
		result.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.AbilitiesTitle));
		bool flag = false;
		BlueprintAbility[] array = (from a in UIUtilityUnit.CollectAbilities(m_Ship)
			where !a.Blueprint.IsMomentum
			select a.Blueprint).ToArray();
		if (array.Any())
		{
			flag = true;
			result.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.ActiveAbilitiesTitle, TooltipTitleType.H5));
			AddAbilities(result, array, TooltipTemplateType.Info);
		}
		UIFeature[] array2 = UIUtilityUnit.CollectFeats(m_Ship).ToArray();
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

	private void AddHP(List<ITooltipBrick> bricks)
	{
		string name = (m_Ship.IsSoftUnit ? UIStrings.Instance.Inspect.Number.Text : UIStrings.Instance.Inspect.ShipHP.Text);
		Sprite icon = (m_Ship.IsSoftUnit ? UIConfig.Instance.UIIcons.TooltipInspectIcons.Number : UIConfig.Instance.UIIcons.TooltipInspectIcons.HP);
		string value = m_Ship.Health.HitPointsLeft + "/" + m_Ship.Health.MaxHitPoints;
		TooltipBaseTemplate tooltip = new TooltipTemplateGlossary("HullIntegritySpace");
		bricks.Add(new TooltipBrickIconStatValue(name, value, null, icon, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Normal, null, tooltip));
	}

	private void AddEvasion(List<ITooltipBrick> bricks)
	{
		string text = UIStrings.Instance.Inspect.Evasion.Text;
		int value = m_Ship.GetStatBaseValue(StatType.Evasion).Value;
		bricks.Add(new TooltipBrickIconStatValue(value: value + "%", addValue: null, tooltip: new TooltipTemplateGlossary("EvasionSpace"), name: text, icon: UIConfig.Instance.UIIcons.TooltipInspectIcons.Evasion));
	}

	private void HitAndCritChance(List<ITooltipBrick> bricks)
	{
		RuleStarshipCalculateHitChances ruleStarshipCalculateHitChances = Rulebook.Trigger(new RuleStarshipCalculateHitChances(m_Ship, Game.Instance.Player.PlayerShip, null));
		TooltipTemplateSimple tooltipTemplateSimple = new TooltipTemplateSimple(UIStrings.Instance.Inspect.HitChance.Text, UIStrings.Instance.Inspect.HitChanceDescription.Text);
		string text = UIStrings.Instance.Inspect.HitChance.Text;
		string value = ruleStarshipCalculateHitChances.ResultHitChance + "%";
		TooltipBaseTemplate tooltip = tooltipTemplateSimple;
		bricks.Add(new TooltipBrickIconStatValue(text, value, null, UIConfig.Instance.UIIcons.TooltipInspectIcons.HitChance, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Normal, null, tooltip));
		TooltipTemplateSimple tooltipTemplateSimple2 = new TooltipTemplateSimple(UIStrings.Instance.Inspect.CriticalChance.Text, UIStrings.Instance.Inspect.CriticalChanceDescription.Text);
		string text2 = UIStrings.Instance.Inspect.CriticalChance.Text;
		string value2 = ruleStarshipCalculateHitChances.ResultCritChance + "%";
		tooltip = tooltipTemplateSimple2;
		bricks.Add(new TooltipBrickIconStatValue(text2, value2, null, UIConfig.Instance.UIIcons.TooltipInspectIcons.CriticalChance, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Normal, null, tooltip));
	}

	private void AddArmoursAndShields(List<ITooltipBrick> bricks)
	{
		TooltipBrickShipInspectScheme tooltipBrickShipInspectScheme = new TooltipBrickShipInspectScheme(UIStrings.Instance.Inspect.ArmoursAndShields, UIStrings.Instance.Inspect.Armours, UIStrings.Instance.Inspect.Shields, UIStrings.Instance.Inspect.Port, UIStrings.Instance.Inspect.Fore, UIStrings.Instance.Inspect.Starboard, UIStrings.Instance.Inspect.Aft);
		TooltipTemplateSimple tooltip = new TooltipTemplateSimple(UIStrings.Instance.ShipCustomization.ArmorPlating, UIStrings.Instance.ShipCustomization.ArmorPlatingDescription);
		tooltipBrickShipInspectScheme.SetArmourValue(ArmourAndShieldValueType.ArmourPort, m_Ship.Hull.GetLocationDeflection(StarshipHitLocation.Port).ToString(), tooltip);
		tooltipBrickShipInspectScheme.SetArmourValue(ArmourAndShieldValueType.ArmourFore, m_Ship.Hull.GetLocationDeflection(StarshipHitLocation.Fore).ToString(), tooltip);
		tooltipBrickShipInspectScheme.SetArmourValue(ArmourAndShieldValueType.ArmourStarboard, m_Ship.Hull.GetLocationDeflection(StarshipHitLocation.Starboard).ToString(), tooltip);
		tooltipBrickShipInspectScheme.SetArmourValue(ArmourAndShieldValueType.ArmourAft, m_Ship.Hull.GetLocationDeflection(StarshipHitLocation.Aft).ToString(), tooltip);
		TooltipTemplateGlossary tooltip2 = new TooltipTemplateGlossary("VoidshipShields");
		tooltipBrickShipInspectScheme.SetArmourValue(ArmourAndShieldValueType.ShieldPort, m_Ship.Shields.GetShields(StarshipSectorShieldsType.Port).Current.ToString(), tooltip2);
		tooltipBrickShipInspectScheme.SetArmourValue(ArmourAndShieldValueType.ShieldFore, m_Ship.Shields.GetShields(StarshipSectorShieldsType.Fore).Current.ToString(), tooltip2);
		tooltipBrickShipInspectScheme.SetArmourValue(ArmourAndShieldValueType.ShieldStarboard, m_Ship.Shields.GetShields(StarshipSectorShieldsType.Starboard).Current.ToString(), tooltip2);
		tooltipBrickShipInspectScheme.SetArmourValue(ArmourAndShieldValueType.ShieldAft, m_Ship.Shields.GetShields(StarshipSectorShieldsType.Aft).Current.ToString(), tooltip2);
		bricks.Add(tooltipBrickShipInspectScheme);
	}

	private void AddBuffsAndStatusEffects(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.StatusEffectsTitle.Text));
		if (m_InspectReactiveData != null)
		{
			bricks.Add(new TooltipBrickWidget(m_InspectReactiveData.TooltipBrickBuffs, new TooltipBrickText(UIStrings.Instance.Inspect.NoStatusEffects.Text, TooltipTextType.Simple | TooltipTextType.BrightColor, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true, 16)));
			return;
		}
		ReactiveCollection<ITooltipBrick> buffsTooltipBricks = InspectExtensions.GetBuffsTooltipBricks(m_Ship);
		bricks.Add(new TooltipBrickWidget(buffsTooltipBricks, new TooltipBrickText(UIStrings.Instance.Inspect.NoStatusEffects.Text, TooltipTextType.Simple | TooltipTextType.BrightColor, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: true, 16)));
	}

	private void AddWeapon(List<ITooltipBrick> bricks)
	{
		List<WeaponSlot> weaponSlots = m_Ship.GetHull().WeaponSlots;
		if (weaponSlots.Count == 0)
		{
			return;
		}
		weaponSlots = weaponSlots.ToTempList();
		weaponSlots.Sort(Comparison);
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.WeaponsTitle));
		WeaponSlotType weaponSlotType = WeaponSlotType.None;
		foreach (WeaponSlot item in weaponSlots)
		{
			Ability activeAbility = item.ActiveAbility;
			if (activeAbility != null && !activeAbility.Hidden && !activeAbility.Blueprint.IsCantrip)
			{
				if (weaponSlotType != item.Type)
				{
					bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Inspect.GetWeaponSlotStringByType(item.Type), TooltipTitleType.H6));
				}
				bricks.Add(new TooltipBrickFeature(activeAbility));
				weaponSlotType = item.Type;
			}
		}
	}

	private static int Comparison(WeaponSlot s1, WeaponSlot s2)
	{
		if (s1.Type == WeaponSlotType.Dorsal && s2.Type == WeaponSlotType.Prow)
		{
			return 1;
		}
		if (s1.Type == WeaponSlotType.Prow && s2.Type == WeaponSlotType.Dorsal)
		{
			return 0;
		}
		return s1.Type.CompareTo(s2.Type);
	}

	protected void AddAbilities(List<ITooltipBrick> bricks, BlueprintAbility[] abilities, TooltipTemplateType type)
	{
		if (!abilities.Empty())
		{
			foreach (BlueprintAbility ability in abilities)
			{
				bricks.Add(new TooltipBrickFeature(ability));
			}
		}
	}

	protected void AddFeatures(List<ITooltipBrick> bricks, FeatureUIData[] features, TooltipTemplateType type)
	{
		if (features.Empty())
		{
			return;
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
					bricks.Add(new TooltipBrickFeature(feature));
				}
			}
			break;
		}
		}
	}
}
