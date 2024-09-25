using UnityEngine;

namespace Kingmaker.Controllers.Clicks;

public interface IDragClickEventHandler
{
	bool OnClick(GameObject gameObject, Vector3 startDrag, Vector3 endDrag);

	bool OnDrag(GameObject gameObject, Vector3 startDrag, Vector3 endDrag);

	bool OnStartDrag(GameObject gameObject, Vector3 startDrag);
}
