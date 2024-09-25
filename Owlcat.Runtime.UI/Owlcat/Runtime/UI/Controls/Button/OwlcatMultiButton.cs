using System;
using System.Collections;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Controls.Selectable;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Owlcat.Runtime.UI.Controls.Button;

[AddComponentMenu("UI/Owlcat/OwlcatMultiButton", 70)]
[SelectionBase]
[DisallowMultipleComponent]
public class OwlcatMultiButton : OwlcatMultiSelectable, IConfirmClickHandler, IConsoleEntity, ILongConfirmClickHandler, IDeclineClickHandler, ILongDeclineClickHandler, IFunc01ClickHandler, ILongFunc01ClickHandler, IFunc02ClickHandler, ILongFunc02ClickHandler, IPointerClickHandler, IEventSystemHandler, IConsolePointerLeftClickEvent, IConsoleNavigationEntity
{
	[SerializeField]
	private ClickEvent m_ConfirmClickEvent = new ClickEvent();

	public string ConfirmClickHint;

	public bool CanConfirm = true;

	private ClickEvent m_LongConfirmClickEvent = new ClickEvent();

	public string LongConfirmClickHint;

	public bool CanLongConfirm = true;

	private ClickEvent m_DeclineClickEvent = new ClickEvent();

	public string DeclineClickHint;

	public bool CanDecline = true;

	private ClickEvent m_LongDeclineClickEvent = new ClickEvent();

	public string LongDeclineClickHint;

	public bool CanLongDecline = true;

	private ClickEvent m_Func01ClickEvent = new ClickEvent();

	public string Func01ClickHint;

	public bool CanFunc01 = true;

	private ClickEvent m_LongFunc01ClickEvent = new ClickEvent();

	public string LongFunc01ClickHint;

	public bool CanLongFunc01 = true;

	private ClickEvent m_Func02ClickEvent = new ClickEvent();

	public string Func02ClickHint;

	public bool CanFunc02 = true;

	private ClickEvent m_LongFunc02ClickEvent = new ClickEvent();

	public string LongFunc02ClickHint;

	public bool CanLongFunc02 = true;

	private const float m_EndTime = 0.3f;

	[SerializeField]
	private UnityEngine.UI.Button.ButtonClickedEvent m_OnLeftClick = new UnityEngine.UI.Button.ButtonClickedEvent();

	[SerializeField]
	private UnityEngine.UI.Button.ButtonClickedEvent m_OnRightClick = new UnityEngine.UI.Button.ButtonClickedEvent();

	[SerializeField]
	private UnityEngine.UI.Button.ButtonClickedEvent m_OnSingleLeftClick = new UnityEngine.UI.Button.ButtonClickedEvent();

	[SerializeField]
	private UnityEngine.UI.Button.ButtonClickedEvent m_OnSingleRightClick = new UnityEngine.UI.Button.ButtonClickedEvent();

	[SerializeField]
	private UnityEngine.UI.Button.ButtonClickedEvent m_OnLeftDoubleClick = new UnityEngine.UI.Button.ButtonClickedEvent();

	[SerializeField]
	private UnityEngine.UI.Button.ButtonClickedEvent m_OnRightDoubleClick = new UnityEngine.UI.Button.ButtonClickedEvent();

	[SerializeField]
	private UnityEngine.UI.Button.ButtonClickedEvent m_OnLeftClickNotInteractable = new UnityEngine.UI.Button.ButtonClickedEvent();

	[SerializeField]
	private UnityEngine.UI.Button.ButtonClickedEvent m_OnRightClickNotInteractable = new UnityEngine.UI.Button.ButtonClickedEvent();

	[SerializeField]
	private UnityEngine.UI.Button.ButtonClickedEvent m_OnSingleLeftClickNotInteractable = new UnityEngine.UI.Button.ButtonClickedEvent();

	[SerializeField]
	private UnityEngine.UI.Button.ButtonClickedEvent m_OnSingleRightClickNotInteractable = new UnityEngine.UI.Button.ButtonClickedEvent();

	[SerializeField]
	private UnityEngine.UI.Button.ButtonClickedEvent m_OnLeftDoubleClickNotInteractable = new UnityEngine.UI.Button.ButtonClickedEvent();

	[SerializeField]
	private UnityEngine.UI.Button.ButtonClickedEvent m_OnRightDoubleClickNotInteractable = new UnityEngine.UI.Button.ButtonClickedEvent();

	private float m_Time;

	public ClickEvent ConfirmClickEvent => m_ConfirmClickEvent;

	public ClickEvent LongConfirmClickEvent => m_LongConfirmClickEvent;

	public ClickEvent DeclineClickEvent => m_DeclineClickEvent;

	public ClickEvent LongDeclineClickEvent => m_LongDeclineClickEvent;

	public ClickEvent Func01ClickEvent => m_Func01ClickEvent;

	public ClickEvent LongFunc01ClickEvent => m_LongFunc01ClickEvent;

	public ClickEvent Func02ClickEvent => m_Func02ClickEvent;

	public ClickEvent LongFunc02ClickEvent => m_LongFunc02ClickEvent;

	public ReactiveCommand PointerLeftClickCommand { get; } = new ReactiveCommand();


	public UnityEngine.UI.Button.ButtonClickedEvent OnLeftClick => m_OnLeftClick;

	public UnityEngine.UI.Button.ButtonClickedEvent OnRightClick => m_OnRightClick;

	public UnityEngine.UI.Button.ButtonClickedEvent OnSingleLeftClick => m_OnSingleLeftClick;

	public UnityEngine.UI.Button.ButtonClickedEvent OnSingleRightClick => m_OnSingleRightClick;

	public UnityEngine.UI.Button.ButtonClickedEvent OnLeftDoubleClick => m_OnLeftDoubleClick;

	public UnityEngine.UI.Button.ButtonClickedEvent OnRightDoubleClick => m_OnRightDoubleClick;

	public UnityEngine.UI.Button.ButtonClickedEvent OnLeftClickNotInteractable => m_OnLeftClickNotInteractable;

	public UnityEngine.UI.Button.ButtonClickedEvent OnRightClickNotInteractable => m_OnRightClickNotInteractable;

	public UnityEngine.UI.Button.ButtonClickedEvent OnSingleLeftClickNotInteractable => m_OnSingleLeftClickNotInteractable;

	public UnityEngine.UI.Button.ButtonClickedEvent OnSingleRightClickNotInteractable => m_OnSingleRightClickNotInteractable;

	public UnityEngine.UI.Button.ButtonClickedEvent OnLeftDoubleClickNotInteractable => m_OnLeftDoubleClickNotInteractable;

	public UnityEngine.UI.Button.ButtonClickedEvent OnRightDoubleClickNotInteractable => m_OnRightDoubleClickNotInteractable;

	public bool CanConfirmClick()
	{
		if (m_ConfirmClickEvent.AllListenersCount > 0)
		{
			return CanConfirm;
		}
		return false;
	}

	public string GetConfirmClickHint()
	{
		return ConfirmClickHint;
	}

	public void OnConfirmClick()
	{
		if (IsActive() && base.Interactable)
		{
			m_ConfirmClickEvent.Invoke();
		}
	}

	public bool CanLongConfirmClick()
	{
		if (m_LongConfirmClickEvent.AllListenersCount > 0)
		{
			return CanLongConfirm;
		}
		return false;
	}

	public string GetLongConfirmClickHint()
	{
		return LongConfirmClickHint;
	}

	public void OnLongConfirmClick()
	{
		if (IsActive() && base.Interactable)
		{
			m_LongConfirmClickEvent.Invoke();
		}
	}

	public bool CanDeclineClick()
	{
		if (m_DeclineClickEvent.AllListenersCount > 0)
		{
			return CanDecline;
		}
		return false;
	}

	public string GetDeclineClickHint()
	{
		return DeclineClickHint;
	}

	public void OnDeclineClick()
	{
		if (IsActive() && base.Interactable)
		{
			m_DeclineClickEvent.Invoke();
		}
	}

	public bool CanLongDeclineClick()
	{
		if (m_LongDeclineClickEvent.AllListenersCount > 0)
		{
			return CanLongDecline;
		}
		return false;
	}

	public string GetLongDeclineClickHint()
	{
		return LongDeclineClickHint;
	}

	public void OnLongDeclineClick()
	{
		if (IsActive() && base.Interactable)
		{
			m_LongDeclineClickEvent.Invoke();
		}
	}

	public bool CanFunc01Click()
	{
		if (m_Func01ClickEvent.AllListenersCount > 0)
		{
			return CanFunc01;
		}
		return false;
	}

	public string GetFunc01ClickHint()
	{
		return Func01ClickHint;
	}

	public void OnFunc01Click()
	{
		if (IsActive() && base.Interactable)
		{
			m_Func01ClickEvent.Invoke();
		}
	}

	public bool CanLongFunc01Click()
	{
		if (m_LongFunc01ClickEvent.AllListenersCount > 0)
		{
			return CanLongFunc01;
		}
		return false;
	}

	public string GetLongFunc01ClickHint()
	{
		return LongFunc01ClickHint;
	}

	public void OnLongFunc01Click()
	{
		if (IsActive() && base.Interactable)
		{
			m_LongFunc01ClickEvent.Invoke();
		}
	}

	public bool CanFunc02Click()
	{
		if (m_Func02ClickEvent.AllListenersCount > 0)
		{
			return CanFunc02;
		}
		return false;
	}

	public string GetFunc02ClickHint()
	{
		return Func02ClickHint;
	}

	public void OnFunc02Click()
	{
		if (IsActive() && base.Interactable)
		{
			m_Func02ClickEvent.Invoke();
		}
	}

	public bool CanLongFunc02Click()
	{
		if (m_LongFunc02ClickEvent.AllListenersCount > 0)
		{
			return CanLongFunc02;
		}
		return false;
	}

	public string GetLongFunc02ClickHint()
	{
		return LongFunc02ClickHint;
	}

	public void OnLongFunc02Click()
	{
		if (IsActive() && base.Interactable)
		{
			m_LongFunc02ClickEvent.Invoke();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!IsActive())
		{
			return;
		}
		bool flag = eventData.clickCount >= 2;
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			PointerLeftClickCommand.Execute();
			if (!eventData.dragging)
			{
				StartCoroutine(LeftClickTime());
			}
			if (base.Interactable)
			{
				m_OnLeftClick?.Invoke();
			}
			else
			{
				m_OnLeftClickNotInteractable?.Invoke();
			}
			if (flag)
			{
				if (base.Interactable)
				{
					m_OnLeftDoubleClick.Invoke();
				}
				else
				{
					m_OnLeftDoubleClickNotInteractable.Invoke();
				}
				m_Time = 0f;
				StopAllCoroutines();
			}
			break;
		case PointerEventData.InputButton.Right:
			if (!eventData.dragging)
			{
				StartCoroutine(RightClickTime());
			}
			if (base.Interactable)
			{
				m_OnRightClick?.Invoke();
			}
			else
			{
				m_OnRightClickNotInteractable?.Invoke();
			}
			if (flag)
			{
				if (base.Interactable)
				{
					m_OnRightDoubleClick?.Invoke();
				}
				else
				{
					m_OnRightDoubleClickNotInteractable?.Invoke();
				}
				m_Time = 0f;
				StopAllCoroutines();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case PointerEventData.InputButton.Middle:
			break;
		}
	}

	private IEnumerator LeftClickTime()
	{
		while (m_Time < 0.3f)
		{
			m_Time += Time.unscaledDeltaTime;
			yield return null;
		}
		m_Time = 0f;
		if (base.Interactable)
		{
			m_OnSingleLeftClick.Invoke();
		}
		else
		{
			m_OnSingleLeftClickNotInteractable.Invoke();
		}
	}

	private IEnumerator RightClickTime()
	{
		while (m_Time < 0.3f)
		{
			m_Time += Time.unscaledDeltaTime;
			yield return null;
		}
		m_Time = 0f;
		if (base.Interactable)
		{
			m_OnSingleRightClick.Invoke();
		}
		else
		{
			m_OnSingleRightClickNotInteractable.Invoke();
		}
	}
}
