using System;
using Kingmaker.Blueprints;

namespace Code.GameCore.Blueprints.BlueprintReflectionValidator;

public class BlueprintValidatorVisitor<T> : BlueprintScriptableObjectValidationVisitor where T : BlueprintScriptableObject
{
	public override Type TargetType => typeof(T);

	protected T ConvertTarget(BlueprintScriptableObject bpso)
	{
		return bpso as T;
	}

	public override void OnValidate(BlueprintScriptableObject instance)
	{
		base.OnValidate(instance);
	}
}
