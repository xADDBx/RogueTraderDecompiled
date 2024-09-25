using System.Reflection;
using UnityEngine;

namespace Owlcat.QA.Validation;

internal class ValidateIsPrefabAttribute : ValidatingFieldAttribute
{
	public override void ValidateField(object obj, FieldInfo field, ValidationContext context, int parentIndex)
	{
		_ = field.GetValue(obj) as Object == null;
	}
}
