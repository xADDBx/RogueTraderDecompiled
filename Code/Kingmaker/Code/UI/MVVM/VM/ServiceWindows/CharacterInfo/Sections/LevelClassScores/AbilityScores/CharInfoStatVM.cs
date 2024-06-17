using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;

public class CharInfoStatVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly ReactiveProperty<ModifiableValue> m_Stat = new ReactiveProperty<ModifiableValue>();

	private readonly ReactiveProperty<ModifiableValue> m_PreviewStat = new ReactiveProperty<ModifiableValue>();

	public readonly StringReactiveProperty Name = new StringReactiveProperty();

	public readonly BoolReactiveProperty HasBonuses = new BoolReactiveProperty();

	public readonly BoolReactiveProperty HasPenalties = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsValueEnabled = new BoolReactiveProperty(initialValue: true);

	public readonly IntReactiveProperty StatValue = new IntReactiveProperty();

	public readonly IntReactiveProperty PreviewStatValue = new IntReactiveProperty();

	public readonly StringReactiveProperty StringValue = new StringReactiveProperty();

	public readonly BoolReactiveProperty IsRecommended = new BoolReactiveProperty();

	public readonly IntReactiveProperty Bonus = new IntReactiveProperty();

	public readonly ReactiveProperty<bool> HighlightedBySource = new ReactiveProperty<bool>();

	private readonly bool m_IsValuePermanent;

	private readonly ReactiveProperty<int?> m_RaceBonus = new ReactiveProperty<int?>();

	private readonly IntReactiveProperty m_Rank = new IntReactiveProperty();

	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private LocalizedString m_FlatFooted;

	public readonly float FontMultiplier = FontSizeMultiplier;

	public StatType? SourceStatType { get; private set; }

	private StatType StatType { get; set; }

	public string ShortName => UIUtilityTexts.GetStatShortName(StatType);

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public CharInfoStatVM([NotNull] ModifiableValue stat, bool showPermanentValue)
	{
		CharInfoStatVM charInfoStatVM = this;
		m_Stat.Value = stat;
		m_IsValuePermanent = showPermanentValue;
		AddDisposable(m_Stat.Subscribe(delegate
		{
			charInfoStatVM.OnStatUpdated();
		}));
		stat.OnChanged += OnStatValueChanged;
		AddDisposable(Disposable.Create(delegate
		{
			stat.OnChanged -= charInfoStatVM.OnStatValueChanged;
		}));
	}

	public CharInfoStatVM([NotNull] ModifiableValue stat, ModifiableValue previewStat)
		: this(stat, showPermanentValue: false)
	{
		CharInfoStatVM @object = this;
		m_PreviewStat.Value = previewStat;
		if (previewStat != null)
		{
			previewStat.OnChanged += OnStatValueChanged;
			AddDisposable(Disposable.Create(delegate
			{
				previewStat.OnChanged -= @object.OnStatValueChanged;
			}));
		}
		OnStatUpdated();
	}

	public void UpdateStat(ModifiableValue stat)
	{
		m_Stat.SetValueAndForceNotify(stat);
	}

	private void OnStatValueChanged(ModifiableValue stat, int oldValue)
	{
		OnStatUpdated();
	}

	private void OnStatUpdated()
	{
		if (m_Stat.Value == null)
		{
			return;
		}
		StatType = m_Stat.Value.Type;
		SourceStatType = UIUtilityUnit.GetSourceStatType(m_Stat.Value);
		Name.Value = LocalizedTexts.Instance.Stats.GetText(StatType);
		StatValue.Value = (m_IsValuePermanent ? m_Stat.Value.PermanentValue : m_Stat.Value.ModifiedValue);
		m_Rank.Value = m_Stat.Value.BaseValue;
		if (m_PreviewStat.Value != null)
		{
			PreviewStatValue.Value = (m_IsValuePermanent ? m_PreviewStat.Value.PermanentValue : m_PreviewStat.Value.ModifiedValue);
		}
		else
		{
			PreviewStatValue.Value = StatValue.Value;
		}
		if (!TryExtractModifiableValueAttributeStat(m_Stat.Value))
		{
			TryExtractModifiableValueSkill(m_Stat.Value);
		}
		StatTooltipData statData = null;
		if (m_Stat.Value is ModifiableValueAttributeStat attribute)
		{
			statData = new StatTooltipData(attribute);
		}
		else if (m_Stat.Value is ModifiableValueSkill skill)
		{
			statData = new StatTooltipData(skill);
		}
		else if (m_Stat.Value is ModifiableValueSavingThrow savingThrow)
		{
			statData = new StatTooltipData(savingThrow);
		}
		else
		{
			ModifiableValue value = m_Stat.Value;
			if (value != null)
			{
				statData = new StatTooltipData(value);
			}
		}
		Tooltip.Value = new TooltipTemplateStat(statData);
	}

	public void UpdateRecommendedMark(List<StatType> recommendedStats)
	{
		IsRecommended.Value = recommendedStats?.Contains(StatType) ?? false;
	}

	private bool TryExtractModifiableValueAttributeStat(ModifiableValue stat)
	{
		if (!(stat is ModifiableValueAttributeStat modifiableValueAttributeStat))
		{
			return false;
		}
		IsValueEnabled.Value = modifiableValueAttributeStat.Enabled;
		Bonus.Value = GetModifier(modifiableValueAttributeStat, m_IsValuePermanent);
		HasBonuses.Value = modifiableValueAttributeStat.HasBonuses;
		HasPenalties.Value = modifiableValueAttributeStat.HasPenalties;
		return true;
	}

	private bool TryExtractModifiableValueSkill(ModifiableValue stat)
	{
		if (!(stat is ModifiableValueSkill modifiableValueSkill))
		{
			return false;
		}
		IsValueEnabled.Value = true;
		Bonus.Value = GetModifier(modifiableValueSkill, m_IsValuePermanent);
		HasBonuses.Value = modifiableValueSkill.HasBonuses;
		HasPenalties.Value = modifiableValueSkill.HasPenalties;
		return true;
	}

	private int GetModifier(ModifiableValueAttributeStat stat, bool permanent)
	{
		if (!permanent)
		{
			return stat.ModifiedValue - stat.PermanentValue;
		}
		return stat.ModifiedValue - stat.BaseValue;
	}

	private int GetModifier(ModifiableValueSkill skill, bool permanent)
	{
		if (!permanent)
		{
			return skill.ModifiedValue - skill.PermanentValue;
		}
		return skill.ModifiedValue - skill.BaseValue;
	}

	public void HighlightBySourceType(StatType? sourceType)
	{
		HighlightedBySource.Value = sourceType.HasValue && SourceStatType == sourceType;
	}

	protected override void DisposeImplementation()
	{
	}
}
