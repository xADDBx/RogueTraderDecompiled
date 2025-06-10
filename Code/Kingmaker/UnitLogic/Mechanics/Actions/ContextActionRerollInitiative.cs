using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("61d40b6be056475688f25e8540e66032")]
public class ContextActionRerollInitiative : ContextAction
{
	public override string GetCaption()
	{
		return "Reroll initiative for target";
	}

	protected override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity != null)
		{
			entity.Initiative.Value = 0f;
			EventBus.RaiseEvent(delegate(IInitiativeChangeHandler h)
			{
				h.HandleInitiativeChanged();
			});
		}
	}
}
