using System;
using Kingmaker.Code.UI.MVVM.VM.CounterWindow;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICounterWindowUIHandler : ISubscriber
{
	void HandleOpen(CounterWindowType type, ItemEntity item, Action<int> command);
}
