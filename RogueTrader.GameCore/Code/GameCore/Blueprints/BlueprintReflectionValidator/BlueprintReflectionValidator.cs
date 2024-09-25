using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;

namespace Code.GameCore.Blueprints.BlueprintReflectionValidator;

public class BlueprintReflectionValidator
{
	private const string ValidatorAssemblyName = "RogueTrader.GameCore.Editor.BlueprintsValidation";

	private static readonly Dictionary<Type, BlueprintScriptableObjectValidationVisitor> Validators;

	private static BlueprintScriptableObjectValidationVisitor s_DefaultValidator;

	static BlueprintReflectionValidator()
	{
		Validators = new Dictionary<Type, BlueprintScriptableObjectValidationVisitor>();
		s_DefaultValidator = new BlueprintScriptableObjectValidationVisitor();
	}

	public static void ValidateBlueprint(BlueprintScriptableObject bp)
	{
		if (Validators.Count == 0)
		{
			return;
		}
		Type type = bp.GetType();
		if (Validators.TryGetValue(type, out var value))
		{
			value.OnValidate(bp);
			return;
		}
		foreach (BlueprintScriptableObjectValidationVisitor value2 in Validators.Values)
		{
			if (value2.IsApplicableForType(type))
			{
				value = value2;
				break;
			}
		}
		if (value == null)
		{
			value = s_DefaultValidator;
		}
		value.OnValidate(bp);
	}
}
