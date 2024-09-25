using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class VisibilityController
{
	private CanvasGroup m_Group;

	private VisibilityController(GameObject obj)
	{
		m_Group = obj.EnsureComponent<CanvasGroup>();
		SetVisible(obj.activeSelf);
	}

	public static VisibilityController Control(Component component)
	{
		if (component == null)
		{
			return null;
		}
		return new VisibilityController(component.gameObject);
	}

	public static VisibilityController ControlParent(Component component)
	{
		if (component == null)
		{
			return null;
		}
		return new VisibilityController(component.transform.parent.gameObject);
	}

	public static VisibilityController Control(GameObject obj)
	{
		if (obj == null)
		{
			return null;
		}
		return new VisibilityController(obj);
	}

	public void SetVisible(bool visible)
	{
		m_Group.alpha = (visible ? 1 : 0);
		m_Group.interactable = visible;
		if (!m_Group.gameObject.activeSelf)
		{
			m_Group.gameObject.SetActive(value: true);
		}
	}
}
