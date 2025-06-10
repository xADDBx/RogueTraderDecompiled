using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("3d78d8a020a847e09e915dcd20a8c82b")]
public class MyPet : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		UnitPartPetOwner unitPartPetOwner = null;
		if (ContextData<PropertyContextData>.Current?.Context.CurrentEntity is BaseUnitEntity baseUnitEntity)
		{
			unitPartPetOwner = baseUnitEntity.GetOptional<UnitPartPetOwner>();
		}
		if (unitPartPetOwner == null && ContextData<MechanicsContext.Data>.Current?.Context.MaybeCaster is BaseUnitEntity baseUnitEntity2)
		{
			unitPartPetOwner = baseUnitEntity2.GetOptional<UnitPartPetOwner>();
		}
		return unitPartPetOwner?.PetUnit;
	}

	public override string GetCaption()
	{
		return "Get my Pet";
	}
}
