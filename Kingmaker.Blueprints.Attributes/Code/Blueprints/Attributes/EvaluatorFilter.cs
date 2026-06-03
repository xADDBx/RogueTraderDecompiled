using System;

namespace Code.Blueprints.Attributes;

public class EvaluatorFilter : Attribute
{
	public readonly string FilterName;

	public const string Universal = "Universal";

	public const string Inherited = "Inherited";

	public const string Ability = "Ability";

	public const string Unit = "Unit";

	public const string InteractableSurface = "InteractableSurface";

	public const string AreaEffect = "AreaEffect";

	public EvaluatorFilter(string filterName)
	{
		FilterName = filterName;
	}
}
