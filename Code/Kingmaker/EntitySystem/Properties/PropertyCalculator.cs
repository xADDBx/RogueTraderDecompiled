using System;
using System.Collections.Generic;
using System.Text;
using Code.GameCore.ElementsSystem;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.EntitySystem.Properties.Getters;
using Kingmaker.RuleSystem;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties;

[Serializable]
public class PropertyCalculator : ElementsList
{
	public enum OperationType
	{
		Sum,
		Sub,
		Mul,
		Div,
		Mod,
		And,
		Or,
		Max,
		Min,
		G,
		GE,
		L,
		LE,
		Eq,
		NEq,
		BoolAnd,
		BoolOr
	}

	[SerializeField]
	public OperationType Operation;

	[SerializeField]
	public PropertyTargetType TargetType;

	[SerializeField]
	public bool FailSilentlyIfNoTarget;

	[SerializeReference]
	public PropertyGetter[] Getters = new PropertyGetter[0];

	public override IEnumerable<Element> Elements => Getters;

	public bool Any => Getters.Length != 0;

	public bool Empty => Getters.Length < 1;

	public bool IsSimple
	{
		get
		{
			if (Getters.Length > 2)
			{
				return false;
			}
			PropertyGetter[] getters = Getters;
			foreach (PropertyGetter propertyGetter in getters)
			{
				if (propertyGetter is PropertyCalculatorGetter propertyCalculatorGetter)
				{
					if (propertyCalculatorGetter.Value.Getters.Length > 1)
					{
						return false;
					}
					if (propertyCalculatorGetter.Value.Getters.Length == 1 && !propertyCalculatorGetter.Value.Getters[0].IsSimple)
					{
						return false;
					}
				}
				else if (!propertyGetter.IsSimple)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool IsBool
	{
		get
		{
			OperationType operation = Operation;
			return operation == OperationType.And || operation == OperationType.Or || operation == OperationType.BoolAnd || operation == OperationType.BoolOr || operation == OperationType.G || operation == OperationType.GE || operation == OperationType.L || operation == OperationType.LE || operation == OperationType.Eq || operation == OperationType.NEq;
		}
	}

	public int GetValue(PropertyContext context)
	{
		using (ProfileScope.New("Calculator"))
		{
			using ElementsDebugger elementsDebugger = ElementsDebugger.Scope(this);
			try
			{
				if (TargetType != 0)
				{
					MechanicEntity targetEntity = context.GetTargetEntity(TargetType);
					if (targetEntity == null)
					{
						bool flag = context.Rule is IRulebookHasNoTarget rulebookHasNoTarget && rulebookHasNoTarget.HasNoTarget;
						if (!FailSilentlyIfNoTarget && !flag)
						{
							throw new Exception($"Can't switch target to {TargetType}: inaccessible in context (Fact={context.Fact}, Ability={context.Ability})");
						}
						elementsDebugger?.SetResult(0);
						return 0;
					}
					context = context.WithCurrentEntity(targetEntity);
				}
				using (ContextData<PropertyContextData>.Request().Setup(context))
				{
					int result = PropertyCalculatorHelper.CalculateValue(Getters, Operation, this);
					elementsDebugger?.SetResult(result);
					return result;
				}
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex, "Exception in PropertyCalculator");
				elementsDebugger?.SetException(ex);
				return 0;
			}
		}
	}

	public bool GetBoolValue(PropertyContext context)
	{
		return GetValue(context) != 0;
	}

	public string GenerateDescription(bool useLineBreaks)
	{
		bool flag = true;
		if (Getters.Length == 0)
		{
			return "[0]";
		}
		using (FormulaTargetScope.Enter(TargetType, useLineBreaks))
		{
			if (Getters.Length == 1)
			{
				if (Getters[0].IsSimple)
				{
					return Getters[0].GetCaption(useLineBreaks);
				}
				using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
				StringBuilder builder = pooledStringBuilder.Builder;
				if (useLineBreaks)
				{
					builder.Append('\n');
				}
				builder.AppendIndentedFormula('(');
				using (FormulaScope.Enter(useLineBreaks))
				{
					builder.Append(Getters[0].GetCaption(useLineBreaks));
				}
				builder.AppendIndentedFormula(')');
				if (useLineBreaks)
				{
					builder.Append('\n');
				}
				return builder.ToString();
			}
			using PooledStringBuilder pooledStringBuilder2 = ContextData<PooledStringBuilder>.Request();
			StringBuilder builder2 = pooledStringBuilder2.Builder;
			if (!IsSimple && useLineBreaks)
			{
				builder2.Append('\n');
			}
			string text = OperationToString(Operation);
			if (Operation == OperationType.Max || Operation == OperationType.Min)
			{
				if (useLineBreaks)
				{
					if (!IsSimple)
					{
						string arg = (flag ? "cyan" : "#888888");
						if (useLineBreaks)
						{
							builder2.AppendIndentedFormula($"<color='{arg}'>{Operation}</color>");
						}
						else
						{
							builder2.AppendIndentedFormula(Operation.ToString());
						}
						if (useLineBreaks)
						{
							builder2.Append('\n');
						}
					}
					else
					{
						builder2.Append(Operation.ToString());
					}
				}
				else
				{
					builder2.Append(Operation.ToString());
				}
			}
			else if (useLineBreaks)
			{
				text = "<b>" + text + "</b>";
			}
			if (IsSimple)
			{
				builder2.Append('(');
			}
			else
			{
				builder2.AppendIndentedFormula('(');
			}
			if (Getters[0].IsSimple && useLineBreaks && !IsSimple && useLineBreaks)
			{
				builder2.Append('\n');
			}
			using (FormulaScope.Enter(useLineBreaks))
			{
				bool flag2 = false;
				for (int i = 0; i < Getters.Length; i++)
				{
					if (i != 0)
					{
						if (builder2[builder2.Length - 1] == '\n')
						{
							builder2.Remove(builder2.Length - 1, 1);
							flag2 = true;
						}
						if (Getters[i - 1].IsSimple || flag2)
						{
							builder2.Append(text);
						}
						else
						{
							builder2.AppendIndentedFormula(text);
						}
						if (useLineBreaks && Getters[i].IsSimple && !IsSimple)
						{
							builder2.Append("\n");
						}
						flag2 = false;
					}
					if (Getters[i].IsSimple && !IsSimple)
					{
						builder2.AppendIndentedFormula(Getters[i].GetCaption(useLineBreaks));
						continue;
					}
					bool flag3 = Getters[i] is ConditionalGetter;
					if (flag3 && useLineBreaks)
					{
						builder2.Append('\n');
						builder2.AppendIndentedFormula("(");
					}
					using (FormulaScope.Enter(useLineBreaks && flag3))
					{
						builder2.Append(Getters[i].GetCaption(useLineBreaks));
					}
					if (flag3 && useLineBreaks)
					{
						builder2.AppendIndentedFormula(")");
						flag2 = true;
					}
				}
			}
			if (!IsSimple && useLineBreaks)
			{
				builder2.Append('\n');
			}
			if (IsSimple)
			{
				builder2.Append(')');
			}
			else
			{
				builder2.AppendIndentedFormula(')');
			}
			if (!IsSimple && useLineBreaks)
			{
				builder2.Append('\n');
			}
			return builder2.ToString();
		}
	}

	public override string ToString()
	{
		string text = GenerateDescription(useLineBreaks: false);
		if (text.Length > 50)
		{
			text = text.Substring(0, 50) + "...";
		}
		return text;
	}

	private static string OperationToString(OperationType operation)
	{
		switch (operation)
		{
		case OperationType.Sum:
			return " + ";
		case OperationType.Sub:
			return " - ";
		case OperationType.Mul:
			return " * ";
		case OperationType.Div:
			return " / ";
		case OperationType.Mod:
			return " %";
		case OperationType.And:
			return " && ";
		case OperationType.Or:
			return " || ";
		case OperationType.Max:
		case OperationType.Min:
			return ", ";
		case OperationType.G:
			return " > ";
		case OperationType.GE:
			return " >= ";
		case OperationType.L:
			return " < ";
		case OperationType.LE:
			return " <= ";
		case OperationType.Eq:
			return " == ";
		case OperationType.NEq:
			return " != ";
		case OperationType.BoolAnd:
			return " && ";
		case OperationType.BoolOr:
			return " || ";
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
