using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using MemoryPack;

namespace Kingmaker.UnitLogic.Levelup.Selections;

[TypeId("60eeb523421a43e3a7c232ee79c0a0ae")]
[MemoryPackable(GenerateType.NoGenerate)]
public abstract class BlueprintSelection : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintSelection>
	{
	}
}
