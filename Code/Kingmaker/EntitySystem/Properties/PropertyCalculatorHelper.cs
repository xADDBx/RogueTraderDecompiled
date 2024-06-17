using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties;

public static class PropertyCalculatorHelper
{
	public static int CalculateValue(PropertyGetter[] getters, PropertyCalculator.OperationType op)
	{
		return op switch
		{
			PropertyCalculator.OperationType.Sum => getters.Sum(), 
			PropertyCalculator.OperationType.Sub => getters.Sub(), 
			PropertyCalculator.OperationType.Mul => getters.Mul(), 
			PropertyCalculator.OperationType.Div => getters.Div(), 
			PropertyCalculator.OperationType.Mod => getters.Mod(), 
			PropertyCalculator.OperationType.And => getters.And(), 
			PropertyCalculator.OperationType.Or => getters.Or(), 
			PropertyCalculator.OperationType.Max => getters.Max(), 
			PropertyCalculator.OperationType.Min => getters.Min(), 
			PropertyCalculator.OperationType.G => getters.Greater(), 
			PropertyCalculator.OperationType.GE => getters.GreaterOrEq(), 
			PropertyCalculator.OperationType.L => getters.Less(), 
			PropertyCalculator.OperationType.LE => getters.LessOrEq(), 
			PropertyCalculator.OperationType.Eq => getters.Equal(), 
			PropertyCalculator.OperationType.NEq => getters.NotEqual(), 
			PropertyCalculator.OperationType.BoolAnd => getters.BoolAnd(), 
			PropertyCalculator.OperationType.BoolOr => getters.BoolOr(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static int Sum(this PropertyGetter[] getters)
	{
		int num = 0;
		foreach (PropertyGetter propertyGetter in getters)
		{
			num += propertyGetter.GetValue();
		}
		return num;
	}

	private static int Sub(this PropertyGetter[] getters)
	{
		int num = getters.Get(0)?.GetValue() ?? 0;
		for (int i = 1; i < getters.Length; i++)
		{
			num -= getters[i].GetValue();
		}
		return num;
	}

	private static int Mul(this PropertyGetter[] getters)
	{
		int num = ((getters.Length != 0) ? 1 : 0);
		foreach (PropertyGetter propertyGetter in getters)
		{
			num *= propertyGetter.GetValue();
		}
		return num;
	}

	private static int Div(this PropertyGetter[] getters)
	{
		int num = getters.Get(0)?.GetValue() ?? 0;
		for (int i = 1; i < getters.Length; i++)
		{
			num /= getters[i].GetValue();
		}
		return num;
	}

	private static int Mod(this PropertyGetter[] getters)
	{
		int num = getters.Get(0)?.GetValue() ?? 0;
		for (int i = 1; i < getters.Length; i++)
		{
			num %= getters[i].GetValue();
		}
		return num;
	}

	private static int And(this PropertyGetter[] getters)
	{
		bool flag = getters.Length != 0;
		foreach (PropertyGetter propertyGetter in getters)
		{
			flag = flag && propertyGetter.GetValue() != 0;
		}
		if (!flag)
		{
			return 0;
		}
		return 1;
	}

	private static int BoolAnd(this PropertyGetter[] getters)
	{
		foreach (PropertyGetter propertyGetter in getters)
		{
			int value = propertyGetter.GetValue();
			if (value <= 0 && (!propertyGetter.Settings.Negate || value != 0))
			{
				return 0;
			}
		}
		return 1;
	}

	private static int BoolOr(this PropertyGetter[] getters)
	{
		if (getters.Length == 0)
		{
			return 1;
		}
		foreach (PropertyGetter propertyGetter in getters)
		{
			int value = propertyGetter.GetValue();
			if (value > 0 || (propertyGetter.Settings.Negate && value == 0))
			{
				return 1;
			}
		}
		return 0;
	}

	private static int Or(this PropertyGetter[] getters)
	{
		bool flag = false;
		foreach (PropertyGetter propertyGetter in getters)
		{
			flag = flag || propertyGetter.GetValue() != 0;
		}
		if (!flag)
		{
			return 0;
		}
		return 1;
	}

	private static int Max(this PropertyGetter[] getters)
	{
		int num = getters.Get(0)?.GetValue() ?? 0;
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue();
			if (value > num)
			{
				num = value;
			}
		}
		return num;
	}

	private static int Min(this PropertyGetter[] getters)
	{
		int num = getters.Get(0)?.GetValue() ?? 0;
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue();
			if (value < num)
			{
				num = value;
			}
		}
		return num;
	}

	private static int Greater(this PropertyGetter[] getters)
	{
		if (getters.Length < 2)
		{
			return 0;
		}
		int num = getters[0].GetValue();
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue();
			if (num <= value)
			{
				return 0;
			}
			num = value;
		}
		return 1;
	}

	private static int GreaterOrEq(this PropertyGetter[] getters)
	{
		if (getters.Length < 2)
		{
			return 0;
		}
		int num = getters[0].GetValue();
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue();
			if (num < value)
			{
				return 0;
			}
			num = value;
		}
		return 1;
	}

	private static int Less(this PropertyGetter[] getters)
	{
		if (getters.Length < 2)
		{
			return 0;
		}
		int num = getters[0].GetValue();
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue();
			if (num >= value)
			{
				return 0;
			}
			num = value;
		}
		return 1;
	}

	private static int LessOrEq(this PropertyGetter[] getters)
	{
		if (getters.Length < 2)
		{
			return 0;
		}
		int num = getters[0].GetValue();
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue();
			if (num > value)
			{
				return 0;
			}
			num = value;
		}
		return 1;
	}

	private static int Equal(this PropertyGetter[] getters)
	{
		if (getters.Length < 2)
		{
			return 0;
		}
		int num = getters[0].GetValue();
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue();
			if (num != value)
			{
				return 0;
			}
			num = value;
		}
		return 1;
	}

	private static int NotEqual(this PropertyGetter[] getters)
	{
		if (getters.Length < 2)
		{
			return 0;
		}
		int num = getters[0].GetValue();
		for (int i = 1; i < getters.Length; i++)
		{
			int value = getters[i].GetValue();
			if (num == value)
			{
				return 0;
			}
			num = value;
		}
		return 1;
	}
}
