using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UI.Common;

[TypeId("f83e0c0243ed49498d11e6d3597e615c")]
public class BlueprintUINetLobbyTutorial : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintUINetLobbyTutorial>
	{
	}

	public List<NetLobbyTutorialBlockInfo> TutorialBlocksInfo;
}
