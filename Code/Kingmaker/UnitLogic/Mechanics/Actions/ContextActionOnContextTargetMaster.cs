using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("0f1b4976d43541da95f7add021372893")]
public class ContextActionOnContextTargetMaster : ContextAction
{
	public ActionList Actions;

	public override string GetCaption()
	{
		return "Run actions on Master of main target of context";
	}

	protected override void RunAction()
	{
		BaseUnitEntity baseUnitEntity = (base.Context.MainTarget.Entity as BaseUnitEntity)?.Master;
		if (baseUnitEntity != null)
		{
			using (base.Context.GetDataScope((TargetWrapper)baseUnitEntity))
			{
				Actions.Run();
			}
		}
	}
}
