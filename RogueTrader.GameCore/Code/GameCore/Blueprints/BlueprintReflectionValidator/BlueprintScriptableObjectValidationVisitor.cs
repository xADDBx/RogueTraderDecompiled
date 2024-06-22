using System;
using Kingmaker.Blueprints;
using Owlcat.QA.Validation;

namespace Code.GameCore.Blueprints.BlueprintReflectionValidator;

public class BlueprintScriptableObjectValidationVisitor : SimpleBlueprintValidationVisitor
{
	public virtual Type TargetType => typeof(BlueprintScriptableObject);

	public virtual bool IsApplicableForType(Type type)
	{
		return TargetType.IsAssignableFrom(type);
	}

	public virtual void OnValidate(BlueprintScriptableObject instance)
	{
		if (BlueprintValidationHelper.AllowOnValidate)
		{
			base.OnValidate(instance);
		}
	}
}
