using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Levelup.Components;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("5474d502f4f54412b9cd7e7bbd0ddeec")]
public class CharacterLevelLimit : BlueprintComponent
{
	public int LevelLimit;
}
