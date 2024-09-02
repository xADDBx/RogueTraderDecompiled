using System;
using System.ComponentModel;

namespace Kingmaker.Enums;

[AttributeUsage(AttributeTargets.Field)]
public class DescriptionEmail : DescriptionAttribute
{
	public DescriptionEmail(string email)
		: base(email)
	{
	}
}
