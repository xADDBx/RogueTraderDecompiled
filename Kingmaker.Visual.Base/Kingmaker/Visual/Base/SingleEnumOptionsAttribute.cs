using System;
using UnityEngine;

namespace Kingmaker.Visual.Base;

public class SingleEnumOptionsAttribute : PropertyAttribute
{
	private Type m_EnumType;

	private string m_FieldName;

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

	public string FieldName => m_FieldName;

	public bool IsValid { get; private set; }

	public SingleEnumOptionsAttribute(Type enumType, string fieldName)
	{
		EnumType = enumType;
		m_FieldName = fieldName;
	}
}
