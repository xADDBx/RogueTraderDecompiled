using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.View.Mechanics.Entities;

[KnowledgeDatabaseID("54bafdf96029471e91a1f9864ad3975a")]
public class PlatformObjectView : EntityViewBase
{
	public new PlatformObjectEntity Data => (PlatformObjectEntity)base.Data;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new PlatformObjectEntity(this));
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		SetVisible(base.IsInGame);
	}
}
