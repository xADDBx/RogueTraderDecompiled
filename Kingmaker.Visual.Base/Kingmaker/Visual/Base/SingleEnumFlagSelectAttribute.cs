using System;
using UnityEngine;

namespace Kingmaker.Visual.Base;

public class SingleEnumFlagSelectAttribute : PropertyAttribute
{
	private Type m_EnumType;

	public Type EnumType
	{
		get
		{
			return m_EnumType;
		}
		set
		{
			if (value == null)
			{
				PFLog.Default.Warning(GetType().Name + ": EnumType cannot be null");
				return;
			}
			if (!value.IsEnum)
			{
				PFLog.Default.Warning(GetType().Name + ": EnumType is " + value.Name + " this is not an enum");
				return;
			}
			m_EnumType = value;
			IsValid = true;
		}
	}

	public bool IsValid { get; private set; }

	public SingleEnumFlagSelectAttribute(Type enumType)
	{
		EnumType = enumType;
	}
}
