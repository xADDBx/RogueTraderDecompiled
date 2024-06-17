using System;
using System.Reflection;

namespace Owlcat.QA.Validation;

[AttributeUsage(AttributeTargets.Field)]
public class ValidatePositiveNumberAttribute : ValidatingFieldAttribute
{
	public override void ValidateField(object obj, FieldInfo field, ValidationContext context, int parentIndex)
	{
	}
}
