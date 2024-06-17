using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem.Core;

public interface IClickMarkHandler : ISubscriber
{
	void OnClickHandled(Vector3 position);
}
