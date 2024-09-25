using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Owlcat.Runtime.UI.Controls.SelectableState;

[Serializable]
public class OwlcatSelectableLayerPart
{
	[SerializeField]
	private Graphic m_TargetGraphic;

	[SerializeField]
	private GameObject m_TargetGameObject;

	[SerializeField]
	private CanvasGroup m_TargetCanvasGroup;

	[SerializeField]
	private OwlcatTransition m_Transition = OwlcatTransition.ColorTint;

	[SerializeField]
	private ColorBlock m_Colors = ColorBlock.defaultColorBlock;

	[SerializeField]
	private SpriteState m_SpriteState;

	[SerializeField]
	private OwlcatSelectableActiveBlock m_ActiveBlock = OwlcatSelectableActiveBlock.DefaultActiveBlock;

	[SerializeField]
	private OwlcatSelectableCanvasGroupBlock m_CanvasGroupBlock = OwlcatSelectableCanvasGroupBlock.DefaultActiveBlock;

	[SerializeField]
	private OwlcatSelectableSpriteSwapBlock m_SpriteSwap;

	private bool m_IsActive = true;

	private OwlcatSelectionState m_LastState = OwlcatSelectionState.Normal;

	private bool m_StopFadeCanvasCoroutine;

	public bool IsActive
	{
		get
		{
			return m_IsActive;
		}
		set
		{
			m_IsActive = value;
			DoPartTransitionInternal(m_LastState, instant: true);
		}
	}

	public Image Image
	{
		get
		{
			return m_TargetGraphic as Image;
		}
		set
		{
			m_TargetGraphic = value;
		}
	}

	public Graphic TargetGraphic
	{
		get
		{
			return m_TargetGraphic;
		}
		set
		{
			m_TargetGraphic = value;
		}
	}

	public CanvasGroup CanvasGroup
	{
		get
		{
			return m_TargetCanvasGroup;
		}
		set
		{
			m_TargetCanvasGroup = value;
		}
	}

	public OwlcatTransition Transition
	{
		get
		{
			return m_Transition;
		}
		set
		{
			m_Transition = value;
		}
	}

	public ColorBlock Colors
	{
		get
		{
			return m_Colors;
		}
		set
		{
			m_Colors = value;
		}
	}

	public SpriteState SpriteState
	{
		get
		{
			return m_SpriteState;
		}
		set
		{
			m_SpriteState = value;
		}
	}

	public OwlcatSelectableActiveBlock ActiveBlock
	{
		get
		{
			return m_ActiveBlock;
		}
		set
		{
			m_ActiveBlock = value;
		}
	}

	public OwlcatSelectableCanvasGroupBlock CanvasGroupBlock
	{
		get
		{
			return m_CanvasGroupBlock;
		}
		set
		{
			m_CanvasGroupBlock = value;
		}
	}

	public OwlcatSelectableSpriteSwapBlock SpriteSwap
	{
		get
		{
			return m_SpriteSwap;
		}
		set
		{
			m_SpriteSwap = value;
		}
	}

	public virtual void DoPartTransition(OwlcatSelectionState state, bool instant)
	{
		m_LastState = state;
		if (IsActive)
		{
			DoPartTransitionInternal(state, instant);
		}
	}

	private void DoPartTransitionInternal(OwlcatSelectionState state, bool instant)
	{
		if (m_Transition != 0)
		{
			Color color;
			Sprite newSprite;
			Sprite newSprite2;
			bool state2;
			float state3;
			switch (state)
			{
			case OwlcatSelectionState.Normal:
				color = m_Colors.normalColor;
				newSprite = null;
				newSprite2 = m_SpriteSwap.normalSprite;
				state2 = m_ActiveBlock.Normal;
				state3 = m_CanvasGroupBlock.Normal;
				break;
			case OwlcatSelectionState.Focused:
				color = m_Colors.selectedColor;
				newSprite = m_SpriteState.selectedSprite;
				newSprite2 = m_SpriteSwap.focusedSprite;
				state2 = m_ActiveBlock.Selected;
				state3 = m_CanvasGroupBlock.Selected;
				break;
			case OwlcatSelectionState.Highlighted:
				color = m_Colors.highlightedColor;
				newSprite = m_SpriteState.highlightedSprite;
				newSprite2 = m_SpriteSwap.highlightedSprite;
				state2 = m_ActiveBlock.Highlighted;
				state3 = m_CanvasGroupBlock.Highlighted;
				break;
			case OwlcatSelectionState.Pressed:
				color = m_Colors.pressedColor;
				newSprite = m_SpriteState.pressedSprite;
				newSprite2 = m_SpriteSwap.pressedSprite;
				state2 = m_ActiveBlock.Pressed;
				state3 = m_CanvasGroupBlock.Pressed;
				break;
			case OwlcatSelectionState.Disabled:
				color = m_Colors.disabledColor;
				newSprite = m_SpriteState.disabledSprite;
				newSprite2 = m_SpriteSwap.disabledSprite;
				state2 = m_ActiveBlock.Disabled;
				state3 = m_CanvasGroupBlock.Disabled;
				break;
			default:
				color = Color.black;
				newSprite = null;
				newSprite2 = null;
				state2 = false;
				state3 = 0f;
				break;
			}
			switch (m_Transition)
			{
			case OwlcatTransition.ColorTint:
				StartColorTween(color * m_Colors.colorMultiplier, instant);
				break;
			case OwlcatTransition.SpriteSwapLegacy:
				DoSpriteSwap(newSprite);
				break;
			case OwlcatTransition.Activate:
				DoActiveState(state2);
				break;
			case OwlcatTransition.CanvasGroup:
				DoCanvasGroupState(state3, instant);
				break;
			case OwlcatTransition.SpriteSwap:
				DoSpriteSwap(newSprite2);
				break;
			}
		}
	}

	private void StartColorTween(Color targetColor, bool instant)
	{
		if (!(m_TargetGraphic == null) && IsActive)
		{
			m_TargetGraphic.CrossFadeColor(targetColor, instant ? 0f : m_Colors.fadeDuration, ignoreTimeScale: true, useAlpha: true);
		}
	}

	private void DoCanvasGroupState(float state, bool instant)
	{
		if (CanvasGroup == null || !IsActive)
		{
			return;
		}
		m_StopFadeCanvasCoroutine = true;
		if (!CanvasGroup.alpha.Equals(state))
		{
			if (instant)
			{
				CanvasGroup.alpha = state;
			}
			else
			{
				MainThreadDispatcher.StartCoroutine(FadeCanvas(CanvasGroup, state, m_CanvasGroupBlock.FadeDuration));
			}
		}
	}

	public IEnumerator FadeCanvas(CanvasGroup canvasGroup, float entAlpha, float duration)
	{
		m_StopFadeCanvasCoroutine = false;
		float startAlpha = canvasGroup.alpha;
		float startTime = Time.realtimeSinceStartup;
		float endTime = Time.realtimeSinceStartup + duration;
		while (Time.realtimeSinceStartup <= endTime)
		{
			float t = Time.realtimeSinceStartup - startTime;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, entAlpha, t);
			yield return new WaitForEndOfFrame();
		}
		if (!m_StopFadeCanvasCoroutine)
		{
			canvasGroup.alpha = entAlpha;
		}
	}

	private void DoSpriteSwap(Sprite newSprite)
	{
		if (!(Image == null) && IsActive)
		{
			Image.overrideSprite = newSprite;
		}
	}

	private void TriggerAnimation(string triggerName)
	{
	}

	private void DoActiveState(bool state)
	{
		if (!(m_TargetGameObject == null))
		{
			state &= IsActive;
			if (state != m_TargetGameObject.activeSelf)
			{
				m_TargetGameObject.SetActive(state);
			}
		}
	}
}
