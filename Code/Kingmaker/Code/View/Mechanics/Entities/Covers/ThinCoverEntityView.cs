using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Code.View.Mechanics.Entities.Covers;

public class ThinCoverEntityView : BaseCoverEntityView
{
	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new ThinCoverEntity(UniqueId, base.IsInGameBySettings, base.Blueprint));
	}
}
