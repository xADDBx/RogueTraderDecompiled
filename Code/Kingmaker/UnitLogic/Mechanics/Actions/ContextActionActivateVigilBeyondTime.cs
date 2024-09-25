using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("478d42d6423a4eb7a525cfd9393fb3c9")]
public class ContextActionActivateVigilBeyondTime : ContextAction
{
	protected override void RunAction()
	{
		UnitEntity unitEntity = base.Context.MainTarget.Entity as UnitEntity;
		if (base.Context.MaybeCaster is UnitEntity unitEntity2 && unitEntity != null)
		{
			unitEntity2.GetOptional<UnitPartVigilBeyondTime>()?.ActivateVigil(unitEntity);
		}
	}

	public override string GetCaption()
	{
		return "Activate Vigil Beyond Time on target - teleport it and restore its wounds";
	}
}
