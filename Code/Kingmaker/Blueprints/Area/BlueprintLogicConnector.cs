using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Mechanics.Blueprints;

namespace Kingmaker.Blueprints.Area;

[TypeId("1099d88d73fc6eb4ea246d545cf144a6")]
public class BlueprintLogicConnector : BlueprintMechanicEntityFact
{
	protected override Type GetFactType()
	{
		return typeof(EntityFact);
	}
}
