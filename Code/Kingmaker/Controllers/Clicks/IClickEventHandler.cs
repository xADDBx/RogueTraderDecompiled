using UnityEngine;

namespace Kingmaker.Controllers.Clicks;

public interface IClickEventHandler
{
	PointerMode GetMode();

	HandlerPriorityResult GetPriority(GameObject gameObject, Vector3 worldPosition);

	bool OnClick(GameObject gameObject, Vector3 worldPosition, int button, bool simulate = false, bool muteEvents = false);
}
