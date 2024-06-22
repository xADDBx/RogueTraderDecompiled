using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("5f950c181b3157a4486fcd36b702b702")]
public class ContextActionOnContextCaster : ContextAction
{
	public ActionList Actions;

	public override string GetCaption()
	{
		return "Run actions on Caster of context";
	}

	protected override void RunAction()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster != null)
		{
			using (base.Context.GetDataScope((TargetWrapper)maybeCaster))
			{
				Actions.Run();
			}
		}
	}
}
