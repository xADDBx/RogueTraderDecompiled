using System.Reflection;
using UnityEngine;

namespace Owlcat.QA.Validation;

public abstract class ValidatingFieldAttribute : PropertyAttribute
{
	public abstract void ValidateField(object obj, FieldInfo field, ValidationContext context, int parentIndex);
}
