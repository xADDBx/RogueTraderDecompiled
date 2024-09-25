using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Visual.HitSystem;

namespace Kingmaker.UnitLogic.Mechanics.Blueprints;

[TypeId("907fc3bb404843afa2393f7be56df153")]
public class BlueprintDestructibleObject : BlueprintMechanicEntityFact
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintDestructibleObject>
	{
	}

	public int HitPoints;

	public int Toughness;

	public int DamageDeflection;

	public int DamageAbsorption;

	public SurfaceType SurfaceType;
}
