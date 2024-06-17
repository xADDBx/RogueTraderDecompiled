using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.View.Mechanics;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("086ea1e7680449d5ab2a7006667f71c8")]
public class BlueprintMeteor : BlueprintMechanicEntityFact
{
	public int Damage;

	public MechanicEntityView Prefab;

	public Size MeteorSize = Size.Medium;
}
