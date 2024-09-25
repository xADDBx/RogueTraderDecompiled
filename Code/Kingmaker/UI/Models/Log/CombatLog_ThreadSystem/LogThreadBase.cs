using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;

public abstract class LogThreadBase : BaseDisposable
{
	private ReactiveCollection<CombatLogMessage> m_ThreadMessages = new ReactiveCollection<CombatLogMessage>();

	public static bool IsPreviousMessageUseSomething;

	protected static GameLogStrings Strings => GameLogStrings.Instance;

	protected static LogColors Colors => Game.Instance.BlueprintRoot.UIConfig.LogColors;

	public IReadOnlyList<CombatLogMessage> AllMessages => m_ThreadMessages;

	public virtual void StartThread()
	{
	}

	public IObservable<CollectionAddEvent<CombatLogMessage>> ObserveAdd()
	{
		return m_ThreadMessages.ObserveAdd();
	}

	public IObservable<CollectionRemoveEvent<CombatLogMessage>> ObserveRemove()
	{
		return m_ThreadMessages.ObserveRemove();
	}

	protected void AddMessage(CombatLogMessage newMessage)
	{
		IsPreviousMessageUseSomething = false;
		if (!ContextData<GameLogDisabled>.Current && newMessage != null)
		{
			m_ThreadMessages.Add(newMessage);
		}
	}

	public static ITooltipBrick CreateBrickModifier(Modifier modifier, bool valueIsPercent = false, string additionText = null, int nestedLevel = 0, bool isResultValue = false, bool isWithoutPlus = false)
	{
		if (modifier.Value == 0)
		{
			return null;
		}
		string text = StatModifiersBreakdown.GetBonusSourceText(modifier);
		if (text.IsNullOrEmpty())
		{
			text = LocalizedTexts.Instance.Descriptors.GetText(modifier.Descriptor);
		}
		string text2 = "";
		string text3 = ((modifier.Type == ModifierType.PctMul) ? "×" : "");
		string text4 = ((!isWithoutPlus && modifier.Value > 0 && modifier.Type != ModifierType.PctMul) ? "+" : "");
		string text5 = (((valueIsPercent && modifier.Type == ModifierType.ValAdd) || modifier.Type == ModifierType.PctAdd) ? "%" : "");
		float num = modifier.Value;
		if (valueIsPercent && modifier.Type == ModifierType.PctAdd)
		{
			num = num / 100f + 1f;
			text3 = "×";
			text4 = "";
			text5 = "";
		}
		if (modifier.Type == ModifierType.PctMul)
		{
			num /= 100f;
		}
		if (!text3.IsNullOrEmpty())
		{
			text2 = " (" + num * 100f + "%)";
		}
		string value = text3 + text4 + num.ToString(CultureInfo.InvariantCulture) + text5 + additionText + text2;
		return new TooltipBrickTextValue(text, value, nestedLevel, isResultValue);
	}

	public void Cleanup()
	{
		m_ThreadMessages.Clear();
	}

	protected static IEnumerable<ITooltipBrick> CreateBrickModifiers(IEnumerable<Modifier> allModifiers, bool valueIsPercent = false, string additionText = null, int nestedLevel = 0, bool isResultValue = false, bool isFirstWithoutPlus = false)
	{
		foreach (Modifier allModifier in allModifiers)
		{
			ITooltipBrick tooltipBrick = CreateBrickModifier(allModifier, valueIsPercent, additionText, nestedLevel, isResultValue, isFirstWithoutPlus);
			isFirstWithoutPlus = false;
			if (tooltipBrick != null)
			{
				yield return tooltipBrick;
			}
		}
	}

	public static IEnumerable<ITooltipBrick> GetDamageModifiers(DamageData damageData, int nestedLevel, bool minMax, bool common)
	{
		TooltipBrickStrings s = Strings.TooltipBrickStrings;
		List<Modifier> copyList;
		if (minMax)
		{
			copyList = TempList.Get<Modifier>();
			foreach (Modifier item in damageData.MinValueModifiers.List)
			{
				bool flag = false;
				foreach (Modifier item2 in damageData.MaxValueModifiers.List)
				{
					if (item.Type == item2.Type && item.Value == item2.Value && item.Fact == item2.Fact && item.Item == item2.Item && item.Bonus == item2.Bonus && item.Stat == item2.Stat && item.Descriptor == item2.Descriptor)
					{
						flag = true;
						copyList.Add(item2);
					}
				}
				string text = (flag ? "" : s.MinDamage.Text);
				yield return CreateBrickModifier(item, valueIsPercent: false, " " + text, nestedLevel, isResultValue: true);
			}
			foreach (Modifier item3 in damageData.MaxValueModifiers.List)
			{
				if (!copyList.Contains(item3))
				{
					yield return CreateBrickModifier(item3, valueIsPercent: false, " " + s.MaxDamage.Text, nestedLevel, isResultValue: true);
				}
			}
		}
		if (!common)
		{
			yield break;
		}
		copyList = damageData.Modifiers.ValueModifiersList.ToList();
		if (copyList.Count > 0)
		{
			int num = 0;
			bool flag2 = false;
			foreach (Modifier item4 in copyList)
			{
				if (item4.Value != 0)
				{
					flag2 = true;
					num += item4.Value;
				}
			}
			if (num != 0 || flag2)
			{
				yield return new TooltipBrickIconTextValue(s.ValAdd.Text, UIUtility.AddSign(num).ToString(CultureInfo.InvariantCulture) ?? "", nestedLevel + 1, isResultValue: true, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
				bool needPrefix = false;
				foreach (Modifier item5 in copyList)
				{
					if (item5.Value != 0)
					{
						string text2 = ((!needPrefix) ? "" : ((item5.Value > 0) ? "+" : ""));
						needPrefix = true;
						string modifierName = GetModifierName(item5);
						int value = item5.Value;
						yield return new TooltipBrickTextValue(modifierName, text2 + value.ToString(CultureInfo.InvariantCulture), nestedLevel + 1, isResultValue: true);
					}
				}
			}
		}
		List<Modifier> pctAddList = damageData.Modifiers.PercentModifiersList.ToList();
		if (pctAddList.Count > 0)
		{
			float num2 = 0f;
			bool flag3 = false;
			foreach (Modifier item6 in pctAddList)
			{
				if (item6.Value != 0)
				{
					flag3 = true;
					num2 += (float)item6.Value / 100f;
				}
			}
			num2 *= 100f;
			if (num2 != 0f || flag3)
			{
				num2 += 100f;
				yield return new TooltipBrickIconTextValue(s.PctAdd.Text, $"×{(num2 / 100f).ToString(CultureInfo.InvariantCulture)} ({num2}%)", nestedLevel + 1, isResultValue: true, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
				yield return new TooltipBrickTextValue(s.BaseModifier.Text, "100%", nestedLevel + 1, isResultValue: true);
				foreach (Modifier item7 in pctAddList)
				{
					if (item7.Value != 0)
					{
						yield return new TooltipBrickTextValue(GetModifierName(item7), UIUtility.AddSign(item7.Value).ToString(CultureInfo.InvariantCulture) + "%", nestedLevel + 1, isResultValue: true);
					}
				}
			}
		}
		List<Modifier> pctMulList = damageData.Modifiers.PercentMultipliersList.ToList();
		if (pctMulList.Count > 0)
		{
			float num3 = 1f;
			foreach (Modifier item8 in pctMulList)
			{
				num3 *= (float)item8.Value / 100f;
			}
			num3 *= 100f;
			yield return new TooltipBrickIconTextValue(s.PctMul.Text, "×" + (num3 / 100f).ToString(CultureInfo.InvariantCulture) + " (" + num3 + "%)", nestedLevel + 1, isResultValue: true, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
			yield return new TooltipBrickTextValue(s.BaseModifier.Text, "100%", nestedLevel + 1, isResultValue: true);
			foreach (Modifier item9 in pctMulList)
			{
				string modifierName2 = GetModifierName(item9);
				string[] obj = new string[5]
				{
					"×",
					((float)item9.Value / 100f).ToString(CultureInfo.InvariantCulture),
					" (",
					null,
					null
				};
				int value = item9.Value;
				obj[3] = value.ToString();
				obj[4] = "%)";
				yield return new TooltipBrickTextValue(modifierName2, string.Concat(obj), nestedLevel + 1, isResultValue: true);
			}
		}
		List<Modifier> valAddExtraList = damageData.Modifiers.ValueModifiersExtraList.ToList();
		if (valAddExtraList.Count > 0)
		{
			int num4 = 0;
			bool flag4 = false;
			foreach (Modifier item10 in valAddExtraList)
			{
				if (item10.Value != 0)
				{
					flag4 = true;
					num4 += item10.Value;
				}
			}
			if (num4 != 0 || flag4)
			{
				yield return new TooltipBrickIconTextValue(s.ValAddExtra.Text, UIUtility.AddSign(num4).ToString(CultureInfo.InvariantCulture) ?? "", nestedLevel + 1, isResultValue: true, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
				bool needPrefix = false;
				foreach (Modifier item11 in valAddExtraList)
				{
					if (item11.Value != 0)
					{
						string text3 = ((!needPrefix) ? "" : ((item11.Value > 0) ? "+" : ""));
						needPrefix = true;
						string modifierName3 = GetModifierName(item11);
						int value = item11.Value;
						yield return new TooltipBrickTextValue(modifierName3, text3 + value.ToString(CultureInfo.InvariantCulture), nestedLevel + 1, isResultValue: true);
					}
				}
			}
		}
		List<Modifier> pctMulExtraList = damageData.Modifiers.PercentMultipliersExtraList.ToList();
		if (pctMulExtraList.Count <= 0)
		{
			yield break;
		}
		float num5 = 1f;
		foreach (Modifier item12 in pctMulExtraList)
		{
			num5 *= (float)item12.Value / 100f;
		}
		num5 *= 100f;
		yield return new TooltipBrickIconTextValue(s.PctMulExtra.Text, "×" + (num5 / 100f).ToString(CultureInfo.InvariantCulture) + " (" + num5 + "%)", nestedLevel + 1, isResultValue: true, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
		yield return new TooltipBrickTextValue(s.BaseModifier.Text, "100%", nestedLevel + 1, isResultValue: true);
		foreach (Modifier item13 in pctMulExtraList)
		{
			string modifierName4 = GetModifierName(item13);
			string[] obj2 = new string[5]
			{
				"×",
				((float)item13.Value / 100f).ToString(CultureInfo.InvariantCulture),
				" (",
				null,
				null
			};
			int value = item13.Value;
			obj2[3] = value.ToString();
			obj2[4] = "%)";
			yield return new TooltipBrickTextValue(modifierName4, string.Concat(obj2), nestedLevel + 1, isResultValue: true);
		}
	}

	private static string GetModifierName(Modifier modifier)
	{
		string text = StatModifiersBreakdown.GetBonusSourceText(modifier);
		if (text.IsNullOrEmpty())
		{
			text = LocalizedTexts.Instance.Descriptors.GetText(modifier.Descriptor);
		}
		return text;
	}

	protected static IEnumerable<ITooltipBrick> ShowReroll(RuleRollChance roll, int chance, bool isTargetHitIcon = false, bool isProtectionIcon = false)
	{
		if (roll.AnyRerollChance.HasValue)
		{
			int num = roll.RollHistory[0];
			yield return new TooltipBrickTriggeredAuto(Strings.TooltipBrickStrings.TriggeredReroll.Text, null, num <= chance);
			yield return new TooltipBrickTextValue(roll.RerollSourceFactName, null, 2);
			for (int i = roll.RollHistory.Count - 2; i >= 0; i--)
			{
				yield return new TooltipBrickChance((i == 0) ? Strings.TooltipBrickStrings.InitialRoll.Text : Strings.TooltipBrickStrings.CheckRoll.Text, (i == 0) ? chance : roll.AnyRerollChance.Value, roll.RollHistory[i], 2, isResultValue: false, null, isProtectionIcon, isTargetHitIcon);
			}
		}
	}

	protected static ITooltipBrick ShowTooltipBrickDamageNullifier(NullifyInformation nullifyInformation, int finalValue)
	{
		string resultText = (nullifyInformation.HasDamageNullify ? GameLogStrings.Instance.TooltipBrickStrings.NullifierResultSuccess.Text : GameLogStrings.Instance.TooltipBrickStrings.NullifierResultFailed.Text);
		List<NullifyInformation.BuffInformation> list = new List<NullifyInformation.BuffInformation>();
		foreach (NullifyInformation.BuffInformation nullifyBuff in nullifyInformation.NullifyBuffList)
		{
			list.Add(nullifyBuff);
		}
		return new TooltipBrickDamageNullifier(nullifyInformation.DamageChance, nullifyInformation.DamageNegationRoll, (!nullifyInformation.HasDamageNullify) ? finalValue : 0, GameLogStrings.Instance.TooltipBrickStrings.Reasons.Text, list, resultText);
	}

	protected static ITooltipBrick MinMaxChanceBorder(int chance, int maxChanceBorder, int nestedLevel = 2)
	{
		TooltipBrickStrings tooltipBrickStrings = Strings.TooltipBrickStrings;
		if (chance < 0)
		{
			return new TooltipBrickTextValue(tooltipBrickStrings.ChanceBorderMin.Text, tooltipBrickStrings.MinValue.Text + " " + 0 + "% (" + chance + "%)", nestedLevel);
		}
		if (chance > maxChanceBorder)
		{
			return new TooltipBrickTextValue(tooltipBrickStrings.ChanceBorder.Text, tooltipBrickStrings.MaxValue.Text + " " + maxChanceBorder + "% (" + chance + "%)", nestedLevel);
		}
		return null;
	}

	protected static ITooltipBrick AddMinMaxValue(float value, int nestedLevel, int minValue = 0, bool isResultValue = false)
	{
		if (value < (float)minValue)
		{
			return new TooltipBrickIconTextValue("<b>" + Strings.TooltipBrickStrings.ChanceBorderMin.Text + "</b>", "<b>" + Strings.TooltipBrickStrings.MinValue.Text + " " + minValue + "% (" + value.ToString(CultureInfo.InvariantCulture) + "%</b>)", nestedLevel, isResultValue, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: true, isGrayBackground: true);
		}
		if (value > 100f)
		{
			return new TooltipBrickIconTextValue("<b>" + Strings.TooltipBrickStrings.ChanceBorder.Text + "</b>", "<b>" + Strings.TooltipBrickStrings.MaxValue.Text + " " + 100 + "% (" + value.ToString(CultureInfo.InvariantCulture) + "%</b>)", nestedLevel, isResultValue, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: true, isGrayBackground: true);
		}
		return null;
	}

	protected override void DisposeImplementation()
	{
		m_ThreadMessages.Clear();
		m_ThreadMessages = null;
	}
}
