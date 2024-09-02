using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Pointer;

public abstract class BaseCursor : MonoBehaviour, IDisposable
{
	protected Vector2 m_Position;

	[SerializeField]
	protected RectTransform m_CursorTransform;

	[SerializeField]
	protected Image m_CursorImage;

	[Header("Ability")]
	[SerializeField]
	protected GameObject m_AbilityGroup;

	[SerializeField]
	private Image m_AbilityImage;

	[Header("Texts")]
	[SerializeField]
	protected GameObject m_TextsGroup;

	[SerializeField]
	private TextMeshProUGUI m_UpperText;

	[SerializeField]
	private TextMeshProUGUI m_LowerText;

	[Header("Additional")]
	[SerializeField]
	protected GameObject m_NoMoveObject;

	protected CursorType m_CurrentType;

	protected bool m_CastMode;

	protected CompositeDisposable m_Disposable;

	public bool IsActive { get; private set; }

	public Vector2 Position
	{
		get
		{
			if (!IsActive)
			{
				return Input.mousePosition;
			}
			return m_Position;
		}
		protected set
		{
			if (IsActive)
			{
				m_Position = value;
				m_CursorTransform.anchoredPosition = m_Position;
				OnSetPosition(value);
			}
		}
	}

	public CursorType CurrentType => m_CurrentType;

	public bool PrevActiveCursorState { get; private set; }

	public IDisposable Bind()
	{
		m_CursorTransform.gameObject.SetActive(value: false);
		m_Disposable = new CompositeDisposable();
		OnBind();
		return this;
	}

	protected virtual void OnBind()
	{
	}

	public void Dispose()
	{
		m_Disposable?.Dispose();
		SetActive(active: false);
		Game.Instance.ClickEventsController?.ClearPointerMode();
		OnDispose();
	}

	protected virtual void OnDispose()
	{
	}

	protected virtual void OnSetPosition(Vector2 position)
	{
	}

	public void SetActive(bool active)
	{
		if (HideCursorAnyWay())
		{
			return;
		}
		Cursor.visible = Game.Instance.IsControllerMouse && !active;
		if (IsActive != active)
		{
			m_CursorTransform.gameObject.SetActive(active);
			IsActive = active;
			PrevActiveCursorState = active;
			if (!active)
			{
				ClearComponents();
			}
			OnSetActive(active);
		}
	}

	protected virtual void OnSetActive(bool active)
	{
	}

	public bool HideCursorAnyWay()
	{
		if (UIVisibilityState.VisibilityPreset.Value.HasFlag(UIVisibilityFlags.Pointer))
		{
			return false;
		}
		Cursor.visible = false;
		m_CursorTransform.gameObject.SetActive(value: false);
		IsActive = false;
		ClearComponents();
		OnSetActive(active: false);
		return true;
	}

	public void SetCursor(CursorType type)
	{
		if (m_CurrentType != type)
		{
			m_CastMode = type == CursorType.Cast || type == CursorType.CastRestricted;
			m_CursorImage.sprite = BlueprintRoot.Instance.Cursors.GetSprite(type);
			Vector2 hotspot = ((type != CursorType.Vertical && type != CursorType.Horizontal && type != CursorType.DiagonalLeft && type != CursorType.DiagonalRight && type != CursorType.RotateCamera) ? Vector2.zero : new Vector2(32f, 32f));
			Cursor.SetCursor(BlueprintRoot.Instance.Cursors.GetTexture(type), hotspot, CursorMode.Auto);
			m_AbilityGroup.SetActive(m_CastMode);
			m_CurrentType = type;
			OnSetCursor(type);
		}
	}

	protected virtual void OnSetCursor(CursorType type)
	{
	}

	public void SetAbilityCursor(Sprite abilityIcon)
	{
		m_AbilityImage.sprite = abilityIcon;
		SetCursor(CursorType.Cast);
	}

	public void SetTexts(string upperText, string lowerText)
	{
		bool flag = string.IsNullOrEmpty(upperText) && string.IsNullOrEmpty(lowerText);
		m_TextsGroup.SetActive(!flag);
		OnSetText(!flag);
		if (!flag)
		{
			m_UpperText.text = upperText;
			m_LowerText.text = lowerText;
		}
	}

	protected virtual void OnSetText(bool hasText)
	{
	}

	public void SetNoMove(bool noMove)
	{
		m_NoMoveObject.SetActive(noMove);
	}

	public void ClearComponents()
	{
		m_AbilityImage.sprite = null;
		m_AbilityGroup.SetActive(value: false);
		SetTexts(null, null);
		SetNoMove(noMove: false);
	}

	public virtual void OnGuiChanged(bool onGui)
	{
	}
}
