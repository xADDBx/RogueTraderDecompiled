using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.View.Mechanics.Entities;

public class MechanicLightRootView : MechanicEntityView
{
	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new MechanicLightRoot(UniqueId, base.IsInGameBySettings, Root.WH.DefaultMapObjectBlueprint));
	}
}
