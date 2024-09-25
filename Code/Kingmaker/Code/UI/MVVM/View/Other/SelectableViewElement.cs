using System;
using JetBrains.Annotations;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Other;

[Serializable]
public class SelectableViewElement
{
	public enum SelectionState
	{
		Normal,
		Highlighted,
		Selected,
		Pressed,
		Disabled
	}

	private enum Transition
	{
		None,
		ColorTint,
		SpriteSwap,
		ObjectOnOff
	}

	[FormerlySerializedAs("highlightGraphic")]
	[FormerlySerializedAs("m_HighlightGraphic")]
	[SerializeField]
	private Graphic m_TargetGraphic;

	[FormerlySerializedAs("transition")]
	[SerializeField]
	private Transition m_Transition;

	[FormerlySerializedAs("colors")]
	[SerializeField]
	[ShowIf("ColorTintIsChosen")]
	private ColorBlock m_Colors;

	[FormerlySerializedAs("spriteState")]
	[SerializeField]
	[ShowIf("SpriteSwapIsChosen")]
	private SpriteState m_SpriteState;

	[FormerlySerializedAs("On/Off")]
	[SerializeField]
	[ShowIf("ObjectOnOffIsChosen")]
	private bool m_ActiveState_Normal;

	[SerializeField]
	[ShowIf("ObjectOnOffIsChosen")]
	private bool m_ActiveState_Highlighted;

	[SerializeField]
	[ShowIf("ObjectOnOffIsChosen")]
	private bool m_ActiveState_Selected;

	[SerializeField]
	[ShowIf("ObjectOnOffIsChosen")]
	private bool m_ActiveState_Pressed;

	[SerializeField]
	[ShowIf("ObjectOnOffIsChosen")]
	private bool m_ActiveState_Disabled;

	[UsedImplicitly]
	private bool ColorTintIsChosen => m_Transition == Transition.ColorTint;

	[UsedImplicitly]
	private bool SpriteSwapIsChosen => m_Transition == Transition.SpriteSwap;

	[UsedImplicitly]
	private bool ObjectOnOffIsChosen => m_Transition == Transition.ObjectOnOff;

	private Image Image => m_TargetGraphic as Image;

	public void DoStateTransition(SelectionState state, bool instant)
	{
		if (!(m_TargetGraphic == null))
		{
			Color color;
			Sprite newSprite;
			bool onState;
			switch (state)
			{
			case SelectionState.Normal:
				color = m_Colors.normalColor;
				newSprite = null;
				onState = m_ActiveState_Normal;
				break;
			case SelectionState.Highlighted:
				color = m_Colors.highlightedColor;
				newSprite = m_SpriteState.highlightedSprite;
				onState = m_ActiveState_Highlighted;
				break;
			case SelectionState.Pressed:
				color = m_Colors.pressedColor;
				newSprite = m_SpriteState.pressedSprite;
				onState = m_ActiveState_Pressed;
				break;
			case SelectionState.Selected:
				color = m_Colors.selectedColor;
				newSprite = m_SpriteState.selectedSprite;
				onState = m_ActiveState_Selected;
				break;
			case SelectionState.Disabled:
				color = m_Colors.disabledColor;
				newSprite = m_SpriteState.disabledSprite;
				onState = m_ActiveState_Disabled;
				break;
			default:
				color = Color.black;
				newSprite = null;
				onState = true;
				break;
			}
			switch (m_Transition)
			{
			case Transition.ColorTint:
				StartColorTween(color * m_Colors.colorMultiplier, instant);
				break;
			case Transition.SpriteSwap:
				DoSpriteSwap(newSprite);
				break;
			case Transition.ObjectOnOff:
				DoActiveState(onState);
				break;
			case Transition.None:
				break;
			}
		}
	}

	private void DoActiveState(bool onState)
	{
		if (!(m_TargetGraphic == null))
		{
			m_TargetGraphic.gameObject.SetActive(onState);
		}
	}

	private void StartColorTween(Color targetColor, bool instant)
	{
		if (!(m_TargetGraphic == null))
		{
			m_TargetGraphic.CrossFadeColor(targetColor, (!instant) ? m_Colors.fadeDuration : 0f, ignoreTimeScale: true, useAlpha: true);
		}
	}

	private void DoSpriteSwap(Sprite newSprite)
	{
		if (!(Image == null))
		{
			Image.overrideSprite = newSprite;
		}
	}
}
