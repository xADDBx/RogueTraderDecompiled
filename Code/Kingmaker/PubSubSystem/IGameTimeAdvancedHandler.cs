using System;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGameTimeAdvancedHandler : ISubscriber
{
	void HandleGameTimeAdvanced(TimeSpan deltaTime);
}
