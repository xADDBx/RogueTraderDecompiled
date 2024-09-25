using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic;

[TypeId("4d0c0980ff194ef991416e047f486152")]
public class BlueprintSoulMark : BlueprintFeature
{
	public override MechanicEntityFact CreateFact(MechanicsContext parentContext, MechanicEntity owner, BuffDuration duration)
	{
		return new SoulMark(this, (BaseUnitEntity)owner, parentContext);
	}
}
