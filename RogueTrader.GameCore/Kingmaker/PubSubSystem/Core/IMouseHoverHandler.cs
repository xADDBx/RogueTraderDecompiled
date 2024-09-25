using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem.Core;

public interface IMouseHoverHandler : ISubscriber
{
	void OnHoverObjectChanged(GameObject oldHover, GameObject newHover);
}
