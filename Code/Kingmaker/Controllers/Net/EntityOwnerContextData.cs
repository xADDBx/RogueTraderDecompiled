using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Controllers.Net;

public class EntityOwnerContextData : ContextData<EntityOwnerContextData>
{
	public Entity Owner { get; private set; }

	public EntityOwnerContextData Setup(Entity owner)
	{
		Owner = owner;
		return this;
	}

	protected override void Reset()
	{
		Owner = null;
	}
}
