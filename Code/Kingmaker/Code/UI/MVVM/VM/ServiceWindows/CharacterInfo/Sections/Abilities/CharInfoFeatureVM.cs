using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoFeatureVM : SelectionGroupEntityVM, IHasTooltipTemplate, IUIDataProvider
{
	public Sprite Icon;

	public string Acronym;

	public string DisplayName;

	public string Description;

	public string FactDescription;

	public string TimeLeft;

	public int? Rank;

	public Ability Ability;

	public TalentIconInfo TalentIconsInfo;

	public bool ShouldShowTalentIcons;

	protected ReactiveProperty<TooltipBaseTemplate> m_Tooltip;

	private readonly object m_TooltipSource;

	public bool IsActive = true;

	public ReactiveProperty<TooltipBaseTemplate> Tooltip => m_Tooltip ?? (m_Tooltip = CreateTooltip());

	string IUIDataProvider.Name => DisplayName;

	string IUIDataProvider.Description => Description;

	Sprite IUIDataProvider.Icon => Icon;

	string IUIDataProvider.NameForAcronym => Acronym;

	private ReactiveProperty<TooltipBaseTemplate> CreateTooltip()
	{
		object tooltipSource = m_TooltipSource;
		if (!(tooltipSource is Buff buff))
		{
			if (!(tooltipSource is Feature feature))
			{
				if (!(tooltipSource is Ability ability))
				{
					if (!(tooltipSource is UIFeature uiFeature))
					{
						if (tooltipSource is ActivatableAbility ability2)
						{
							return new ReactiveProperty<TooltipBaseTemplate>(new TooltipTemplateActivatableAbility(ability2));
						}
						return new ReactiveProperty<TooltipBaseTemplate>();
					}
					return new ReactiveProperty<TooltipBaseTemplate>(new TooltipTemplateUIFeature(uiFeature));
				}
				return new ReactiveProperty<TooltipBaseTemplate>(new TooltipTemplateAbility(ability.Data));
			}
			return new ReactiveProperty<TooltipBaseTemplate>(new TooltipTemplateFeature(feature));
		}
		return new ReactiveProperty<TooltipBaseTemplate>(new TooltipTemplateBuff(buff));
	}

	public CharInfoFeatureVM(Buff buff, MechanicEntity unit)
		: base(allowSwitchOff: false)
	{
		Icon = buff.Icon;
		DisplayName = buff.Name;
		IsActive = buff.Active;
		FactDescription = UIUtilityTexts.UpdateDescriptionWithUIProperties(buff.Description, unit);
		Acronym = UIUtility.GetAbilityAcronym(buff.Blueprint.Name);
		FillTimeLeft(buff);
		FillDescription();
		m_TooltipSource = buff;
	}

	public CharInfoFeatureVM(Feature feature, MechanicEntity unit)
		: base(allowSwitchOff: false)
	{
		Icon = feature.Icon;
		DisplayName = feature.Name;
		IsActive = feature.Active;
		FactDescription = UIUtilityTexts.UpdateDescriptionWithUIProperties(feature.Description, unit);
		Rank = feature.Rank;
		Acronym = UIUtility.GetAbilityAcronym(feature.Blueprint);
		FillDescription();
		m_TooltipSource = feature;
		TalentIconsInfo = feature.Blueprint.TalentIconInfo;
		ShouldShowTalentIcons = feature.Icon == null;
	}

	public CharInfoFeatureVM(Ability ability, MechanicEntity unit)
		: base(allowSwitchOff: false)
	{
		Ability = ability;
		Icon = ability.Icon;
		DisplayName = ability.Name;
		IsActive = ability.Data.Fact?.Active ?? true;
		FactDescription = UIUtilityTexts.UpdateDescriptionWithUIProperties(ability.Description, unit);
		Acronym = UIUtility.GetAbilityAcronym(ability.Blueprint.Name);
		FillDescription();
		m_TooltipSource = ability;
	}

	public CharInfoFeatureVM(ActivatableAbility activatableAbility, MechanicEntity unit)
		: base(allowSwitchOff: false)
	{
		Icon = activatableAbility.Icon;
		DisplayName = activatableAbility.Name;
		IsActive = activatableAbility.Active;
		FactDescription = UIUtilityTexts.UpdateDescriptionWithUIProperties(activatableAbility.Description, unit);
		Acronym = UIUtility.GetAbilityAcronym(activatableAbility.Blueprint.Name);
		FillDescription();
		m_TooltipSource = activatableAbility;
	}

	public CharInfoFeatureVM(UIFeature uiFeature, MechanicEntity unit)
		: base(allowSwitchOff: false)
	{
		BlueprintAbility abilityFromFeature = RankEntryUtils.GetAbilityFromFeature(uiFeature.Feature);
		Icon = ((abilityFromFeature != null) ? abilityFromFeature.Icon : uiFeature.Icon);
		DisplayName = ((abilityFromFeature != null) ? abilityFromFeature.Name : uiFeature.Name);
		FactDescription = UIUtilityTexts.UpdateDescriptionWithUIProperties(uiFeature.Description, unit);
		Rank = uiFeature.Rank;
		Acronym = UIUtility.GetAbilityAcronym(uiFeature.Feature);
		FillDescription();
		m_TooltipSource = uiFeature;
		TalentIconsInfo = uiFeature.TalentIconsInfo;
		ShouldShowTalentIcons = Icon == null;
	}

	private void FillTimeLeft(Buff buff)
	{
		if (buff.IsPermanent)
		{
			TimeLeft = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.CharacterSheet.Permanent.Text;
			return;
		}
		string arg = ((buff.ExpirationInRounds == 1) ? Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Round.Text : Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Rounds.Text);
		TimeLeft = $"{buff.ExpirationInRounds} {arg}";
	}

	private void FillDescription()
	{
		Description = (IsActive ? string.Empty : $"<color=#B2443F>{UIStrings.Instance.CharacterSheet.DeactivatedFeature.Text}</color>");
	}

	protected override void DoSelectMe()
	{
	}

	protected override void DisposeImplementation()
	{
	}

	public void SetAvailable(bool expand)
	{
		m_IsAvailable.Value = expand;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		UpdateTemplate();
		return Tooltip.Value;
	}

	public void UpdateTemplate()
	{
		if (!(this is RankEntrySelectionStatVM rankEntrySelectionStatVM))
		{
			return;
		}
		ModifiableValue stat = rankEntrySelectionStatVM.UnitProgressionVM.LevelUpManager.PreviewUnit.Stats.GetStat(rankEntrySelectionStatVM.UnitStat.Type);
		StatTooltipData statTooltipData = default(StatTooltipData);
		if (!(stat is ModifiableValueAttributeStat attribute))
		{
			if (!(stat is ModifiableValueSkill skill))
			{
				if (!(stat is ModifiableValueSavingThrow savingThrow))
				{
					if (stat != null)
					{
						statTooltipData = new StatTooltipData(stat);
					}
					else
					{
						global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(stat);
					}
				}
				else
				{
					statTooltipData = new StatTooltipData(savingThrow);
				}
			}
			else
			{
				statTooltipData = new StatTooltipData(skill);
			}
		}
		else
		{
			statTooltipData = new StatTooltipData(attribute);
		}
		StatTooltipData statData = statTooltipData;
		Tooltip.Value = new TooltipTemplateRankEntryStat(statData, rankEntrySelectionStatVM.FeatureSelectionItem, rankEntrySelectionStatVM.SelectionStateFeature, showCompanionStats: true);
	}
}
