using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[TypeId("3969212ace044208a60899dc4e1f3b3e")]
public class BuffSpawnFx : AbilitySpawnFx
{
	public bool DestroyOnDeAttach = true;
}
