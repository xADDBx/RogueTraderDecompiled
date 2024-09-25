using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IEntityRef
{
	string Id { get; }

	[CanBeNull]
	T Get<T>() where T : class, IEntity;

	[CanBeNull]
	IEntity Get();
}
