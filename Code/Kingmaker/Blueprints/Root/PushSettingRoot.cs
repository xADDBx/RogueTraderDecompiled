using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("7fdf60f3db424216be56ff94616e6c04")]
public class PushSettingRoot : BlueprintScriptableObject
{
	public float DeflectionCoefficient = 1f;

	public float AbsorptionCoefficient = 1f;

	public float DealtCoefficient = 1f;
}
