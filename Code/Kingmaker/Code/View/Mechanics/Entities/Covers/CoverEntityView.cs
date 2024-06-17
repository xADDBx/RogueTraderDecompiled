using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Code.View.Mechanics.Entities.Covers;

public class CoverEntityView : BaseCoverEntityView
{
	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new CoverEntity(UniqueId, base.IsInGameBySettings, base.Blueprint));
	}
}
