using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;

namespace Kingmaker.Blueprints;

[HashRoot]
[TypeId("fc91c2d06c5f09a419eeca7a14709271")]
public class BlueprintSummonPool : BlueprintScriptableObject
{
	public int Limit;

	public bool DoNotRemoveDeadUnits;
}
