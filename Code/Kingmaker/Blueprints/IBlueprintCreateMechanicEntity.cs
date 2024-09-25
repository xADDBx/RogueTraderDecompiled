using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Blueprints;

public interface IBlueprintCreateMechanicEntity<TEntity> where TEntity : MechanicEntity
{
	TEntity CreateEntity(string uniqueId, bool isInGame);
}
