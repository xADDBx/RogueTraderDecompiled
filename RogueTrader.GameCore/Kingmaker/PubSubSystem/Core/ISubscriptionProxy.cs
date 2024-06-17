using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface ISubscriptionProxy
{
	[CanBeNull]
	ISubscriber GetSubscriber();

	[CanBeNull]
	IDisposable RequestEventContext();

	[CanBeNull]
	IEntity GetSubscribingEntity();
}
