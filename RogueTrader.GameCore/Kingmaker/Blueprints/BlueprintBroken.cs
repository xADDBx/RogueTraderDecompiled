using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Blueprints;

[Serializable]
[TypeId("50180856b8ea4cf6965e53bb91472358")]
public class BlueprintBroken : BlueprintScriptableObject
{
	[NonSerialized]
	public Exception Exception;
}
