using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Clicks.Handlers;

public class ClickOnDetectClicksObjectHandler : IClickEventHandler
{
	private readonly List<IDetectClicks> m_ClickComponents = new List<IDetectClicks>();

	public PointerMode GetMode()
	{
		return PointerMode.Default;
	}

	public HandlerPriorityResult GetPriority(GameObject gameObject, Vector3 worldPosition)
	{
		return new HandlerPriorityResult((gameObject.GetComponentNonAlloc<IDetectClicks>() != null) ? 1f : 0f);
	}

	public bool OnClick(GameObject gameObject, Vector3 worldPosition, int button, bool simulate = false, bool muteEvents = false)
	{
		gameObject.GetComponentsInParent(includeInactive: false, m_ClickComponents);
		m_ClickComponents.ForEach(delegate(IDetectClicks h)
		{
			h.HandleClick();
		});
		return true;
	}
}
