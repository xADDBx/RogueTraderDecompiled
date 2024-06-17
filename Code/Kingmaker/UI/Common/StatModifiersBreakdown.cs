using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Utility;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.UnitLogic.Progression.Paths;
using UnityEngine;

namespace Kingmaker.UI.Common;

public static class StatModifiersBreakdown
{
	[NotNull]
	private static List<StatBonusEntry> s_Data = new List<StatBonusEntry>();

	[CanBeNull]
	private static string s_BonusColor = null;

	[CanBeNull]
	private static string s_PenaltyColor = null;

	private static string BonusColor
	{
		get
		{
			if (s_BonusColor == null)
			{
				s_BonusColor = ColorUtility.ToHtmlStringRGB(BlueprintRoot.Instance.UIConfig.TooltipColors.Bonus);
			}
			return s_BonusColor;
		}
	}

	private static string PenaltyColor
	{
		get
		{
			if (s_PenaltyColor == null)
			{
				s_PenaltyColor = ColorUtility.ToHtmlStringRGB(BlueprintRoot.Instance.UIConfig.TooltipColors.Penaty);
			}
			return s_PenaltyColor;
		}
	}

	private static bool ShouldShowIfZero(Modifier bonus)
	{
		return bonus.Stat != StatType.Unknown;
	}

	public static string GetBonusSourceText(Modifier bonus)
	{
		if (bonus.Fact != null)
		{
			return GetBonusSourceText(bonus.Fact);
		}
		if (bonus.Stat != 0)
		{
			return LocalizedTexts.Instance.Stats.GetText(bonus.Stat);
		}
		return string.Empty;
	}

	public static string GetBonusSourceText(ModifiableValue.Modifier bonus)
	{
		if (bonus.SourceFact != null)
		{
			return GetBonusSourceText(bonus.SourceFact);
		}
		if (bonus.SourceStat != 0)
		{
			return LocalizedTexts.Instance.Stats.GetText(bonus.SourceStat);
		}
		return string.Empty;
	}

	private static string GetBonusSourceText(IUIDataProvider source)
	{
		if (source == null)
		{
			return string.Empty;
		}
		string text = ((!(source is ItemEnchantment itemEnchantment)) ? source.Name : itemEnchantment.Owner.Name);
		if (string.IsNullOrEmpty(text))
		{
			Object @object = source as Object;
			if (@object != null)
			{
				text = @object.name;
			}
		}
		return text;
	}

	private static void AddBonus(Modifier bonus)
	{
		if (bonus.Value != 0 || ShouldShowIfZero(bonus))
		{
			AddBonus(bonus.Value, GetBonusSourceText(bonus), bonus.Descriptor, ShouldShowIfZero(bonus));
		}
	}

	private static void AddBonus(int value, [CanBeNull] IUIDataProvider bonusSource, ModifierDescriptor descriptor)
	{
		if (value != 0)
		{
			AddBonus(value, GetBonusSourceText(bonusSource), descriptor);
		}
	}

	public static void AddBonus(int bonusValue, [CanBeNull] string bonusSource, ModifierDescriptor descriptor = ModifierDescriptor.None, bool addZero = false)
	{
		if (bonusValue != 0 || addZero)
		{
			s_Data.Add(new StatBonusEntry
			{
				Bonus = bonusValue,
				Source = bonusSource,
				Descriptor = descriptor
			});
		}
	}

	public static void AddModifiersManager(AbstractModifiersManager modifiersManager)
	{
		AddModifiersManagerInternal(modifiersManager.List);
	}

	public static void AddCompositeModifiersManager(CompositeModifiersManager modifiersManager)
	{
		AddModifiersManagerInternal(modifiersManager.AllModifiersList);
	}

	private static void AddModifiersManagerInternal(IEnumerable<Modifier> modifiers)
	{
		foreach (Modifier modifier in modifiers)
		{
			if (modifier.Value != 0)
			{
				ModifierDescriptor descriptor = ((modifier.Descriptor != 0) ? modifier.Descriptor : ModifierDescriptor.UntypedUnstackable);
				if (modifier.Stat != 0)
				{
					string text = LocalizedTexts.Instance.Stats.GetText(modifier.Stat);
					AddBonus(modifier.Value, text, descriptor);
				}
				else
				{
					IUIDataProvider bonusSource = TryGetSourceFromFact(modifier.Fact) ?? modifier.Item;
					AddBonus(modifier.Value, bonusSource, descriptor);
				}
			}
		}
	}

	public static void AddStatModifiers([NotNull] ModifiableValue stat)
	{
		foreach (ModifiableValue.Modifier displayModifier in stat.GetDisplayModifiers())
		{
			if (displayModifier.ModValue == 0 || ModifierDisabled(stat, displayModifier))
			{
				continue;
			}
			ModifierDescriptor modifierDescriptor = displayModifier.ModDescriptor;
			if (modifierDescriptor == ModifierDescriptor.CareerAdvancement)
			{
				AddCareerBonuses(displayModifier);
				continue;
			}
			if (displayModifier.SourceStat != 0)
			{
				string text = LocalizedTexts.Instance.Stats.GetText(displayModifier.SourceStat);
				AddBonus(displayModifier.ModValue, text, modifierDescriptor);
				continue;
			}
			IUIDataProvider iUIDataProvider = TryGetSourceFromFact(displayModifier.SourceFact) ?? displayModifier.SourceItem;
			if ((iUIDataProvider == null || string.IsNullOrEmpty(iUIDataProvider.Name)) && modifierDescriptor == ModifierDescriptor.None)
			{
				modifierDescriptor = (displayModifier.Stacks ? ModifierDescriptor.UntypedStackable : ModifierDescriptor.UntypedUnstackable);
			}
			AddBonus(displayModifier.ModValue, iUIDataProvider, modifierDescriptor);
		}
	}

	private static IUIDataProvider TryGetSourceFromFact(EntityFact fact)
	{
		if (fact != null)
		{
			return ((IUIDataProvider)fact.SourceItem) ?? fact;
		}
		return null;
	}

	private static void AddCareerBonuses(ModifiableValue.Modifier mod)
	{
		EntityFact sourceFact = mod.SourceFact;
		if (sourceFact == null || !sourceFact.Sources.Any())
		{
			return;
		}
		Dictionary<BlueprintPath, int> dictionary = new Dictionary<BlueprintPath, int>();
		int num = (sourceFact.Blueprint as BlueprintStatAdvancement)?.ValuePerRank ?? 0;
		BlueprintPath key;
		int value;
		foreach (EntityFactSource source in sourceFact.Sources)
		{
			if (source.Path != null)
			{
				if (!dictionary.ContainsKey(source.Path))
				{
					dictionary[source.Path] = 0;
				}
				key = source.Path;
				value = dictionary[key]++;
			}
		}
		foreach (KeyValuePair<BlueprintPath, int> item in dictionary)
		{
			item.Deconstruct(out key, out value);
			BlueprintPath bonusSource = key;
			int num2 = value;
			AddBonus(num * num2, bonusSource, ModifierDescriptor.CareerAdvancement);
		}
	}

	public static bool ModifierDisabled([NotNull] ModifiableValue stat, ModifiableValue.Modifier mod)
	{
		return false;
	}

	public static void AddBonusSources([NotNull] AbstractModifiersManager modifiers)
	{
		foreach (Modifier item in modifiers.List)
		{
			AddBonus(item);
		}
	}

	public static void AddStoredData([NotNull] StatModifiersBreakdownData storedData)
	{
		s_Data.AddRange(storedData.Bonuses);
	}

	public static void AppendModifiersBreakdown(this StringBuilder sb, string caption = "")
	{
		if (s_Data.Count <= 0)
		{
			return;
		}
		s_Data.Sort(StatBonusEntry.Compare);
		sb.Append(caption);
		foreach (StatBonusEntry s_Datum in s_Data)
		{
			sb.AppendBonus(s_Datum.Bonus, s_Datum.Source, s_Datum.Descriptor);
		}
		s_Data.Clear();
	}

	public static StatModifiersBreakdownData Build()
	{
		StatModifiersBreakdownData result = new StatModifiersBreakdownData(s_Data);
		s_Data = new List<StatBonusEntry>();
		return result;
	}

	public static void Clear()
	{
		s_Data.Clear();
	}

	private static void AppendBonus([NotNull] this StringBuilder sb, int bonusValue, [CanBeNull] string bonusSource, ModifierDescriptor descriptor)
	{
		if (descriptor != 0)
		{
			string text = Game.Instance.BlueprintRoot.LocalizedTexts.Descriptors.GetText(descriptor);
			sb.Append(text).Append(": ");
		}
		else if (!string.IsNullOrWhiteSpace(bonusSource))
		{
			sb.Append(bonusSource).Append(": ");
		}
		string value = ((bonusValue < 0) ? PenaltyColor : BonusColor);
		sb.Append("<color=#").Append(value).Append('>');
		sb.Append(UIConstsExtensions.GetValueWithSign(bonusValue));
		if (descriptor != 0 && !string.IsNullOrWhiteSpace(bonusSource))
		{
			sb.Append(" [").Append(bonusSource).Append(']');
		}
		sb.Append("</color>");
		sb.AppendLine();
	}
}
