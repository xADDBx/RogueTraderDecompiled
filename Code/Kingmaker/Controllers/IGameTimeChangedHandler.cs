using System;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers;

public interface IGameTimeChangedHandler : ISubscriber
{
	void HandleGameTimeChanged(TimeSpan delta);

	void HandleNonGameTimeChanged();
}
