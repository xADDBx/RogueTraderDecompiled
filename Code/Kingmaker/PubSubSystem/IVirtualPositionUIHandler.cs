using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IVirtualPositionUIHandler : ISubscriber
{
	void HandleVirtualPositionChanged(Vector3? position);
}
