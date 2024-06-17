using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using UnityEngine;

namespace Owlcat.QA.Validation;

public class ValidateHasComponentAttribute : ValidatingFieldAttribute
{
	private readonly Type m_ComponenType;

	public ValidateHasComponentAttribute(Type componenType)
	{
		m_ComponenType = componenType;
	}

	public override void ValidateField(object obj, FieldInfo field, ValidationContext context, int parentIndex)
	{
		GameObject gameObject = field.GetValue(obj) as GameObject;
		if (gameObject != null && !gameObject.GetComponent(m_ComponenType))
		{
			context.AddError(ErrorLevel.Critical, field.Name + " must have a " + m_ComponenType.Name + " attached.");
		}
		if (field.GetValue(obj) is BlueprintScriptableObject blueprintScriptableObject && !blueprintScriptableObject.ComponentsArray.Any((BlueprintComponent c) => m_ComponenType.IsInstanceOfType(c)))
		{
			context.AddError(ErrorLevel.Critical, field.Name + " must have a " + m_ComponenType.Name + " attached.");
		}
	}
}
