using System;
using System.Text;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.QA;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties;

[Serializable]
public class PropertyCalculator
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

	public bool Any => Getters.Length != 0;

	public bool Empty => Getters.Length < 1;

	public int GetValue(PropertyContext context)
	{
		try
		{
			if (TargetType != 0)
			{
				MechanicEntity targetEntity = context.GetTargetEntity(TargetType);
				if (targetEntity == null)
				{
					if (!FailSilentlyIfNoTarget)
					{
						PFLog.Default.ErrorWithReport($"Can't switch target to {TargetType}: inaccessible in context");
					}
					return 0;
				}
				context = context.WithCurrentEntity(targetEntity);
			}
			using (ContextData<PropertyContextData>.Request().Setup(context))
			{
				return GetValueInternal();
			}
		}
		catch (Exception exception)
		{
			PFLog.Default.ExceptionWithReport(exception, "Exception in PropertyCalculator");
			return 0;
		}
	}

	public bool GetBoolValue(PropertyContext context)
	{
		return GetValue(context) != 0;
	}

	public (bool Success, int Value) GetValueSafe(PropertyContext context)
	{
		try
		{
			int value = GetValue(context);
			return (Success: true, Value: value);
		}
		catch
		{
			return (Success: false, Value: 0);
		}
	}

	private int GetValueInternal()
	{
		return PropertyCalculatorHelper.CalculateValue(Getters, Operation);
	}

	public override string ToString()
	{
		if (Getters.Length == 0)
		{
			return "[0]";
		}
		if (Getters.Length == 1)
		{
			if (TargetType == PropertyTargetType.CurrentEntity)
			{
				return Getters[0].GetCaption();
			}
			using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
			StringBuilder builder = pooledStringBuilder.Builder;
			builder.Append(TargetType.ToShortString());
			builder.Append('{');
			builder.Append(Getters[0].GetCaption());
			builder.Append('}');
		}
		using PooledStringBuilder pooledStringBuilder2 = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder2 = pooledStringBuilder2.Builder;
		string value = OperationToString(Operation);
		if (TargetType != 0)
		{
			builder2.Append(TargetType.ToShortString());
			builder2.Append('{');
		}
		if (Operation == OperationType.Max || Operation == OperationType.Min)
		{
			builder2.Append(Operation);
		}
		builder2.Append('(');
		for (int i = 0; i < Getters.Length; i++)
		{
			if (i != 0)
			{
				builder2.Append(value);
			}
			builder2.Append(Getters[i].GetCaption());
		}
		builder2.Append(')');
		if (TargetType != 0)
		{
			builder2.Append('}');
		}
		return builder2.ToString();
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
