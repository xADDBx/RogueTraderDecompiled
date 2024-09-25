using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UI.Common;

[TypeId("f32a521bc3574f6a909e7a21424ede67")]
public class BlueprintUILocalMapLegend : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintUILocalMapLegend>
	{
	}

	public List<LocalMapLegendBlockItemInfo> LocalMapLegendBlockItemInfo;
}
