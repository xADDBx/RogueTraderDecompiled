using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.EntitySystem.Properties;

public class PropertyContextData : ContextData<PropertyContextData>
{
	public PropertyContext Context { get; private set; }

	public PropertyContextData Setup(PropertyContext context)
	{
		Context = context;
		return this;
	}

	protected override void Reset()
	{
		Context = default(PropertyContext);
	}
}
