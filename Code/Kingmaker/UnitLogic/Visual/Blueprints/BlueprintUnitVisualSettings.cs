using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Visual.Blueprints;

[TypeId("5be77e71098fe344c8f0a05ae0bf7973")]
public class BlueprintUnitVisualSettings : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintUnitVisualSettings>
	{
	}

	public UnitVisualSettings Settings;
}
