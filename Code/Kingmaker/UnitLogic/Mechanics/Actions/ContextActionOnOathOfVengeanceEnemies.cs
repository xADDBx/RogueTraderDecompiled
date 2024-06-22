using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("537a683dd09f4fc0926480abe3fb1108")]
public class ContextActionOnOathOfVengeanceEnemies : ContextAction
{
	public ActionList Actions;

	public override string GetCaption()
	{
		return "Run actions on all enemies registered for the target ally";
	}

	protected override void RunAction()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		UnitEntity unitEntity = base.Context.MainTarget.Entity as UnitEntity;
		UnitPartOathOfVengeance unitPartOathOfVengeance = maybeCaster?.GetOptional<UnitPartOathOfVengeance>();
		if (maybeCaster == null || unitEntity == null || !unitEntity.IsAlly(maybeCaster) || unitPartOathOfVengeance == null || !unitPartOathOfVengeance.HasEntries(unitEntity))
		{
			return;
		}
		foreach (OathOfVengeanceEntry entry in unitPartOathOfVengeance.GetEntries(unitEntity))
		{
			using (base.Context.GetDataScope(entry.Enemy))
			{
				Actions.Run();
			}
		}
	}
}
