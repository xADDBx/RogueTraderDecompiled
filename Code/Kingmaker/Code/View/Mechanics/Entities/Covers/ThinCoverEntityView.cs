using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Code.View.Mechanics.Entities.Covers;

[KnowledgeDatabaseID("56ff1b62447ca5d479c20d8e95035601")]
public class ThinCoverEntityView : BaseCoverEntityView
{
	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new ThinCoverEntity(UniqueId, base.IsInGameBySettings, base.Blueprint));
	}
}
