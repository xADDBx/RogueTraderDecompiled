using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rewired.UI.ControlMapper;

[AddComponentMenu("")]
public abstract class UIElementInfo : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public string identifier;

	public int intData;

	public TMP_Text text;

	public event Action<GameObject> OnSelectedEvent;

	public void OnSelect(BaseEventData eventData)
	{
		if (this.OnSelectedEvent != null)
		{
			this.OnSelectedEvent(base.gameObject);
		}
	}
}
