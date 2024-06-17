using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.PubSubSystem;

public static class EventInvokerExtensions
{
	[CanBeNull]
	public static Entity Entity => ContextData<EventInvoker>.Current?.InvokerEntity as Entity;

	[CanBeNull]
	public static MechanicEntity MechanicEntity => ContextData<EventInvoker>.Current?.InvokerEntity as MechanicEntity;

	[CanBeNull]
	public static AbstractUnitEntity AbstractUnitEntity => ContextData<EventInvoker>.Current?.InvokerEntity as AbstractUnitEntity;

	[CanBeNull]
	public static BaseUnitEntity BaseUnitEntity => ContextData<EventInvoker>.Current?.InvokerEntity as BaseUnitEntity;

	[CanBeNull]
	public static MapObjectEntity MapObjectEntity => ContextData<EventInvoker>.Current?.InvokerEntity as MapObjectEntity;

	[CanBeNull]
	public static StarshipEntity StarshipEntity => ContextData<EventInvoker>.Current?.InvokerEntity as StarshipEntity;

	[CanBeNull]
	public static TEntity GetEntity<TEntity>() where TEntity : Entity
	{
		return ContextData<EventInvoker>.Current?.InvokerEntity as TEntity;
	}
}
