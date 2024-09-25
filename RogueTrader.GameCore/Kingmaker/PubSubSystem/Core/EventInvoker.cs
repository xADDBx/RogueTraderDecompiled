using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public class EventInvoker : ContextData<EventInvoker>
{
	public IEntity InvokerEntity { get; private set; }

	public EventInvoker Setup(IEntity entity)
	{
		InvokerEntity = entity;
		return this;
	}

	protected override void Reset()
	{
	}
}
