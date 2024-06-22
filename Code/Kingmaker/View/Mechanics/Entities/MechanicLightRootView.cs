using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.View.Mechanics.Entities;

[KnowledgeDatabaseID("67455ded8e9d43a99ebef987e0c4622b")]
public class MechanicLightRootView : MechanicEntityView
{
	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new MechanicLightRoot(UniqueId, base.IsInGameBySettings, Root.WH.DefaultMapObjectBlueprint));
	}
}
