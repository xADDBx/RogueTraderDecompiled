using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("1798cb198ad64ff7a1fb4917d17f6768")]
public class MythicLevelLimit : BlueprintComponent
{
	public int LevelLimit;
}
