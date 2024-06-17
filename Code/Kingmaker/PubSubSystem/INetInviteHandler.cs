using System;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetInviteHandler : ISubscriber
{
	void HandleInvite(Action<bool> callback);
}
