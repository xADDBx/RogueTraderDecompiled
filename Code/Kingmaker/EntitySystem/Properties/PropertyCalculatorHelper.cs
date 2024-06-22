using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties;

public static class PropertyCalculatorHelper
{
	public static int CalculateValue(PropertyGetter[] getters, PropertyCalculator.OperationType op, PropertyCalculator calculator)
	{
		return op switch
		{
			PropertyCalculator.OperationType.Sum => getters.Sum(calculator), 
			PropertyCalculator.OperationType.Sub => getters.Sub(calculator), 
			PropertyCalculator.OperationType.Mul => getters.Mul(calculator), 
			PropertyCalculator.OperationType.Div => getters.Div(calculator), 
			PropertyCalculator.OperationType.Mod => getters.Mod(calculator), 
			PropertyCalculator.OperationType.And => getters.And(calculator), 
			PropertyCalculator.OperationType.Or => getters.Or(calculator), 
			PropertyCalculator.OperationType.Max => getters.Max(calculator), 
			PropertyCalculator.OperationType.Min => getters.Min(calculator), 
			PropertyCalculator.OperationType.G => getters.Greater(calculator), 
			PropertyCalculator.OperationType.GE => getters.GreaterOrEq(calculator), 
			PropertyCalculator.OperationType.L => getters.Less(calculator), 
			PropertyCalculator.OperationType.LE => getters.LessOrEq(calculator), 
			PropertyCalculator.OperationType.Eq => getters.Equal(calculator), 
			PropertyCalculator.OperationType.NEq => getters.NotEqual(calculator), 
			PropertyCalculator.OperationType.BoolAnd => getters.BoolAnd(calculator), 
			PropertyCalculator.OperationType.BoolOr => getters.BoolOr(calculator), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static int Sum(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		int num = 0;
		foreach (PropertyGetter propertyGetter in getters)
		{
			num += propertyGetter.GetValue(calculator);
		}
		return num;
	}

	private static int Sub(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		int num = getters.Get(0)?.GetValue(calculator) ?? 0;
		for (int i = 1; i < getters.Length; i++)
		{
			num -= getters[i].GetValue(calculator);
		}
		return num;
	}

	private static int Mul(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		int num = ((getters.Length != 0) ? 1 : 0);
		foreach (PropertyGetter propertyGetter in getters)
		{
			num *= propertyGetter.GetValue(calculator);
		}
		return num;
	}

	private static int Div(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		int num = getters.Get(0)?.GetValue(calculator) ?? 0;
		for (int i = 1; i < getters.Length; i++)
		{
			num /= getters[i].GetValue(calculator);
		}
		return num;
	}

	private static int Mod(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		int num = getters.Get(0)?.GetValue(calculator) ?? 0;
		for (int i = 1; i < getters.Length; i++)
		{
			num %= getters[i].GetValue(calculator);
		}
		return num;
	}

	private static int And(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		bool flag = getters.Length != 0;
		foreach (PropertyGetter propertyGetter in getters)
		{
			flag = flag && propertyGetter.GetValue(calculator) != 0;
		}
		if (!flag)
		{
			return 0;
		}
		return 1;
	}

	private static int BoolAnd(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		foreach (PropertyGetter propertyGetter in getters)
		{
			int value = propertyGetter.GetValue(calculator);
			if (value <= 0 && (!propertyGetter.Settings.Negate || value != 0))
			{
				return 0;
			}
		}
		return 1;
	}

	private static int BoolOr(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		if (getters.Length == 0)
		{
			return 1;
		}
		foreach (PropertyGetter propertyGetter in getters)
		{
			int value = propertyGetter.GetValue(calculator);
			if (value > 0 || (propertyGetter.Settings.Negate && value == 0))
			{
				return 1;
			}
		}
		return 0;
	}

	private static int Or(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		bool flag = false;
		foreach (PropertyGetter propertyGetter in getters)
		{
			flag = flag || propertyGetter.GetValue(calculator) != 0;
		}
		if (!flag)
		{
			return 0;
		}
		return 1;
	}

	private static int Max(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		int num = getters.Get(0)?.GetValue(calculator) ?? 0;
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue(calculator);
			if (value > num)
			{
				num = value;
			}
		}
		return num;
	}

	private static int Min(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		int num = getters.Get(0)?.GetValue(calculator) ?? 0;
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue(calculator);
			if (value < num)
			{
				num = value;
			}
		}
		return num;
	}

	private static int Greater(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		if (getters.Length < 2)
		{
			return 0;
		}
		int num = getters[0].GetValue(calculator);
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue(calculator);
			if (num <= value)
			{
				return 0;
			}
			num = value;
		}
		return 1;
	}

	private static int GreaterOrEq(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		if (getters.Length < 2)
		{
			return 0;
		}
		int num = getters[0].GetValue(calculator);
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue(calculator);
			if (num < value)
			{
				return 0;
			}
			num = value;
		}
		return 1;
	}

	private static int Less(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		if (getters.Length < 2)
		{
			return 0;
		}
		int num = getters[0].GetValue(calculator);
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue(calculator);
			if (num >= value)
			{
				return 0;
			}
			num = value;
		}
		return 1;
	}

	private static int LessOrEq(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		if (getters.Length < 2)
		{
			return 0;
		}
		int num = getters[0].GetValue(calculator);
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue(calculator);
			if (num > value)
			{
				return 0;
			}
			num = value;
		}
		return 1;
	}

	private static int Equal(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		if (getters.Length < 2)
		{
			return 0;
		}
		int num = getters[0].GetValue(calculator);
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue(calculator);
			if (num != value)
			{
				return 0;
			}
			num = value;
		}
		return 1;
	}

	private static int NotEqual(this PropertyGetter[] getters, PropertyCalculator calculator)
	{
		if (getters.Length < 2)
		{
			return 0;
		}
		int num = getters[0].GetValue(calculator);
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue(calculator);
			if (num == value)
			{
				return 0;
			}
			num = value;
		}
		return 1;
	}
}
