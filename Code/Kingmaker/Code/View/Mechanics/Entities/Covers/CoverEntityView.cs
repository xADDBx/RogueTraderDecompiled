using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Code.View.Mechanics.Entities.Covers;

[KnowledgeDatabaseID("1f447a8bc45e4344913268ef68687237")]
public class CoverEntityView : BaseCoverEntityView
{
	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new CoverEntity(UniqueId, base.IsInGameBySettings, base.Blueprint));
	}
}
