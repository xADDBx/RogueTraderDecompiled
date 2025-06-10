using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("02b69fcc42484aa7be9d1966ec044123")]
public class ContextActionOnContextCasterMaster : ContextAction
{
	public ActionList Actions;

	public override string GetCaption()
	{
		return "Run actions on Master of caster of context";
	}

	protected override void RunAction()
	{
		BaseUnitEntity baseUnitEntity = (base.Context.MaybeCaster as BaseUnitEntity)?.Master;
		if (baseUnitEntity != null)
		{
			using (base.Context.GetDataScope((TargetWrapper)baseUnitEntity))
			{
				Actions.Run();
			}
		}
	}
}
