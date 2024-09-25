using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.Other;

public class ConsoleSelectableButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	public List<SelectableViewElement> m_ViewElements;

	[SerializeField]
	protected bool m_IsInteractable = true;

	[SerializeField]
	protected bool m_IsSelected;

	public ReactiveCommand OnClick = new ReactiveCommand();

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (m_IsInteractable)
		{
			SetFocused(value: true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (m_IsInteractable)
		{
			SetFocused(value: false);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (m_IsInteractable && eventData.clickCount == 1)
		{
			OnClick.Execute();
		}
	}

	private void OnEnable()
	{
		UpdateState(instantColorChange: true);
	}

	private void OnValidate()
	{
		UpdateState();
	}

	private void UpdateState(bool instantColorChange = false)
	{
		if (!base.enabled)
		{
			return;
		}
		SelectableViewElement.SelectionState state = ((!m_IsInteractable) ? SelectableViewElement.SelectionState.Disabled : (m_IsSelected ? SelectableViewElement.SelectionState.Selected : SelectableViewElement.SelectionState.Normal));
		if (m_ViewElements == null || !m_ViewElements.Any())
		{
			return;
		}
		foreach (SelectableViewElement viewElement in m_ViewElements)
		{
			viewElement.DoStateTransition(state, instantColorChange);
		}
	}

	public void SetInteractable(bool value)
	{
		m_IsInteractable = value;
		m_IsSelected &= value;
		UpdateState();
	}

	public virtual void SetFocused(bool value)
	{
		m_IsSelected = value && m_IsInteractable;
		UpdateState();
	}

	public bool IsValid()
	{
		if (m_IsInteractable)
		{
			return base.gameObject.activeInHierarchy;
		}
		return false;
	}
}
