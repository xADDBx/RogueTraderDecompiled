using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.RewiredCursor;
using Owlcat.Runtime.UI.Controls.SelectableState;
using Owlcat.Runtime.UI.Dependencies;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Owlcat.Runtime.UI.Controls.Selectable;

[AddComponentMenu("UI/Owlcat/OwlcatSelectable", 70)]
[SelectionBase]
[DisallowMultipleComponent]
public class OwlcatSelectable : UIBehaviour, IConsoleNavigationEntity, IConsoleEntity, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	public class FocusEvent : UnityEvent<bool>
	{
	}

	public class HoverEvent : UnityEvent<bool>
	{
	}

	public bool AutoRefresh;

	public OwlcatSelectionState TestState = OwlcatSelectionState.Normal;

	[Tooltip("Can the Selectable be interacted with?")]
	[SerializeField]
	private bool m_Interactable = true;

	private bool m_IsHighlighted;

	private bool m_IsFocus;

	private bool m_IsPressed;

	public bool ResetOnDisable = true;

	[SerializeField]
	protected List<OwlcatSelectable> m_ChildSelectables = new List<OwlcatSelectable>();

	[SerializeField]
	private List<OwlcatSelectableLayerPart> m_CommonLayer = new List<OwlcatSelectableLayerPart>();

	private int m_HoverSoundType = -1;

	private int m_ClickSoundType = -1;

	private bool m_GroupsAllowInteraction = true;

	private readonly List<CanvasGroup> m_CanvasGroupCache = new List<CanvasGroup>();

	public FocusEvent OnFocus { get; } = new FocusEvent();


	public int HoverSoundType
	{
		get
		{
			return m_HoverSoundType;
		}
		set
		{
			m_HoverSoundType = value;
		}
	}

	public int ClickSoundType
	{
		get
		{
			return m_ClickSoundType;
		}
		set
		{
			m_ClickSoundType = value;
		}
	}

	protected OwlcatSelectionState CurrentState { get; private set; }

	public bool Interactable
	{
		get
		{
			if (m_Interactable)
			{
				return m_GroupsAllowInteraction;
			}
			return false;
		}
		set
		{
			if (m_Interactable != value)
			{
				m_Interactable = value;
				if (!m_Interactable && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == base.gameObject)
				{
					EventSystem.current.SetSelectedGameObject(null);
				}
				DoSetState();
			}
		}
	}

	public bool IsHighlighted
	{
		get
		{
			return m_IsHighlighted;
		}
		protected set
		{
			m_IsHighlighted = value;
			DoSetState();
		}
	}

	public bool IsFocus
	{
		get
		{
			return m_IsFocus;
		}
		protected set
		{
			m_IsFocus = value;
			DoSetState();
		}
	}

	public bool IsPressed
	{
		get
		{
			return m_IsPressed;
		}
		protected set
		{
			m_IsPressed = value;
			DoSetState();
		}
	}

	public HoverEvent OnHover { get; } = new HoverEvent();


	public void SetFocus(bool value)
	{
		IsFocus = value;
		OnFocus?.Invoke(value);
	}

	public virtual bool IsValid()
	{
		if (IsDestroyed())
		{
			Debug.LogError("Error UIKit: the object of type 'GameObject' has been destroyed, but you are still trying to access it");
			return false;
		}
		if (Interactable)
		{
			return IsActive();
		}
		return false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		CurrentState = OwlcatSelectionState.None;
		DoSetState(instant: true);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (ResetOnDisable)
		{
			m_IsFocus = false;
			m_IsPressed = false;
			m_IsHighlighted = false;
		}
	}

	public void SetInteractable(bool state)
	{
		Interactable = state;
	}

	private OwlcatSelectionState GetCurrentState()
	{
		if (!Interactable)
		{
			return OwlcatSelectionState.Disabled;
		}
		if (m_IsPressed)
		{
			return OwlcatSelectionState.Pressed;
		}
		if (m_IsFocus)
		{
			return OwlcatSelectionState.Focused;
		}
		if (m_IsHighlighted)
		{
			return OwlcatSelectionState.Highlighted;
		}
		return OwlcatSelectionState.Normal;
	}

	protected override void OnCanvasGroupChanged()
	{
		bool flag = true;
		Transform parent = base.transform;
		while (parent != null)
		{
			parent.GetComponents(m_CanvasGroupCache);
			bool flag2 = false;
			for (int i = 0; i < m_CanvasGroupCache.Count; i++)
			{
				if (!m_CanvasGroupCache[i].interactable)
				{
					flag = false;
					flag2 = true;
				}
				if (m_CanvasGroupCache[i].ignoreParentGroups)
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				break;
			}
			parent = parent.parent;
		}
		if (flag != m_GroupsAllowInteraction)
		{
			m_GroupsAllowInteraction = flag;
			DoSetState();
		}
	}

	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();
		DoSetState();
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		OnCanvasGroupChanged();
	}

	public void AddLayerToMainPart(OwlcatSelectableLayerPart layer)
	{
		m_CommonLayer.Add(layer);
	}

	protected virtual void DoSetState(bool instant = false)
	{
		OwlcatSelectionState currentState = GetCurrentState();
		if (currentState == CurrentState)
		{
			return;
		}
		CurrentState = currentState;
		if (IsActive())
		{
			if (CurrentState == OwlcatSelectionState.Highlighted || CurrentState == OwlcatSelectionState.Focused)
			{
				UIKitSoundManager.PlayHoverSound(HoverSoundType);
			}
			else if (CurrentState == OwlcatSelectionState.Pressed)
			{
				UIKitSoundManager.PlayButtonClickSound(ClickSoundType);
			}
			DoSetCommonParts(CurrentState, instant);
		}
	}

	private void DoSetCommonParts(OwlcatSelectionState state, bool instant)
	{
		foreach (OwlcatSelectableLayerPart item in m_CommonLayer)
		{
			item.DoPartTransition(state, instant);
		}
		m_ChildSelectables.ForEach(delegate(OwlcatSelectable c)
		{
			c.DoSetCommonParts(state, instant);
		});
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if ((Switch2Helper.IsRunningOnSwitch2 || Cursor.visible || UIKitRewiredCursorController.Enabled) && (!Switch2Helper.IsRunningOnSwitch2 || GamePad.Instance.Switch2MouseModeActive || UIKitRewiredCursorController.Enabled))
		{
			OnPointerEnter();
		}
	}

	public void OnPointerEnter()
	{
		IsHighlighted = true;
		OnHover?.Invoke(arg0: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if ((Switch2Helper.IsRunningOnSwitch2 || Cursor.visible || UIKitRewiredCursorController.Enabled) && (!Switch2Helper.IsRunningOnSwitch2 || GamePad.Instance.Switch2MouseModeActive || UIKitRewiredCursorController.Enabled))
		{
			OnPointerExit();
		}
	}

	public void OnPointerExit()
	{
		IsHighlighted = false;
		OnHover?.Invoke(arg0: false);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if ((Switch2Helper.IsRunningOnSwitch2 || Cursor.visible || UIKitRewiredCursorController.Enabled) && (!Switch2Helper.IsRunningOnSwitch2 || GamePad.Instance.Switch2MouseModeActive || UIKitRewiredCursorController.Enabled))
		{
			OnPointerDown();
		}
	}

	public void OnPointerDown()
	{
		IsPressed = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if ((Switch2Helper.IsRunningOnSwitch2 || Cursor.visible || UIKitRewiredCursorController.Enabled) && (!Switch2Helper.IsRunningOnSwitch2 || GamePad.Instance.Switch2MouseModeActive || UIKitRewiredCursorController.Enabled))
		{
			OnPointerUp();
		}
	}

	public void OnPointerUp()
	{
		IsPressed = false;
	}
}
