using Kingmaker.Globalmap.SectorMap;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Clicks.Handlers;

public class SectorMapClickObjectHandler : IClickEventHandler
{
	public PointerMode GetMode()
	{
		return PointerMode.Default;
	}

	public virtual HandlerPriorityResult GetPriority(GameObject gameObject, Vector3 worldPosition)
	{
		return new HandlerPriorityResult((GetSectorMapObject(gameObject) != null) ? 1f : 0f);
	}

	public bool OnClick(GameObject gameObject, Vector3 worldPosition, int button, bool simulate = false, bool muteEvents = false)
	{
		if (GetSectorMapObject(gameObject) != null)
		{
			return false;
		}
		return true;
	}

	private static SectorMapObject GetSectorMapObject(GameObject gameObject)
	{
		return gameObject.GetComponentNonAlloc<SectorMapObject>();
	}
}
