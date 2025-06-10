using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("9a9711e7bb7840f48ad264059580b262")]
public class ContextActionOnContextCasterPet : ContextAction
{
	public ActionList Actions;

	public override string GetCaption()
	{
		return "Run actions on the pet of Caster of context";
	}

	protected override void RunAction()
	{
		BaseUnitEntity baseUnitEntity = base.Context.MaybeCaster?.Parts.GetOptional<UnitPartPetOwner>()?.PetUnit;
		if (baseUnitEntity != null)
		{
			using (base.Context.GetDataScope((TargetWrapper)baseUnitEntity))
			{
				Actions.Run();
			}
		}
	}
}
