using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;

namespace Kingmaker.Blueprints;

[HashRoot]
[TypeId("84a976c8e48e6274e8367073fad4a237")]
public class BlueprintAbilityGroup : BlueprintScriptableObject
{
	public int CooldownInRounds;

	public bool IsWeaponAttackGroup;
}
