using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.EntitySystem;

public class EntityFactComponentDelegateContextData : ContextData<EntityFactComponentDelegateContextData>
{
	public EntityFactComponent Runtime { get; private set; }

	public EntityFactComponentDelegateContextData Setup(EntityFactComponent runtime)
	{
		Runtime = runtime;
		return this;
	}

	protected override void Reset()
	{
		Runtime = null;
	}
}
