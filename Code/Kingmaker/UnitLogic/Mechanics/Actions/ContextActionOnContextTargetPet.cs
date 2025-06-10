using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("2d5d10e63cc44328ad7d1575fb1c2a6b")]
public class ContextActionOnContextTargetPet : ContextAction
{
	public ActionList Actions;

	public override string GetCaption()
	{
		return "Run actions on the pet of main target of context";
	}

	protected override void RunAction()
	{
		BaseUnitEntity baseUnitEntity = base.Context.MainTarget.Entity?.Parts.GetOptional<UnitPartPetOwner>()?.PetUnit;
		if (baseUnitEntity != null)
		{
			using (base.Context.GetDataScope((TargetWrapper)baseUnitEntity))
			{
				Actions.Run();
			}
		}
	}
}
