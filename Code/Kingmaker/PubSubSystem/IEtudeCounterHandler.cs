using System;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEtudeCounterHandler : ISubscriber
{
	void ShowEtudeCounter(string id, string label, Func<int> valueGetter, Func<int> targetValueGetter);

	void HideEtudeCounter(string id);
}
