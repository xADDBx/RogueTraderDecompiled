using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickIconStatValueView : TooltipBaseBrickView<TooltipBrickIconStatValueVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_AddValue;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_IconText;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private List<GameObject> m_HasValueGroup = new List<GameObject>();

	[SerializeField]
	private float m_DefaultIconSize = 30f;

	[Header("Text Colors")]
	[SerializeField]
	private Color m_NormalColor;

	[SerializeField]
	private Color m_PositiveColor;

	[SerializeField]
	private Color m_NegativeColor;

	[Header("Background Colors")]
	[SerializeField]
	private Color m_NormalBackgroundColor;

	[SerializeField]
	private Color m_PositiveBackgroundColor;

	[SerializeField]
	private Color m_NegativeBackgroundColor;

	[SerializeField]
	private float m_NonNormalWidthExpanse;

	[Header("TextSize")]
	[SerializeField]
	private float m_DefaultFontSizeLabel = 20f;

	[SerializeField]
	private float m_DefaultFontSizeValue = 22f;

	[SerializeField]
	private float m_DefaultFontSizeAddValue = 16f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeLabel = 20f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeValue = 22f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeAddValue = 16f;

	private bool m_IsExpansed;

	private Color m_DefaultIconColor;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		m_TextHelper = new AccessibilityTextHelper(m_Label, m_Value, m_AddValue);
		m_DefaultIconColor = m_Icon.color;
		base.BindViewImplementation();
		m_Label.text = base.ViewModel.Name;
		m_Value.text = base.ViewModel.Value;
		UpdateAddValue(base.ViewModel.AddValue);
		AddDisposable(m_AddValue.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		m_Icon.gameObject.SetActive(base.ViewModel.Icon != null);
		m_Icon.sprite = base.ViewModel.Icon;
		if (base.ViewModel.IconColor.HasValue)
		{
			m_Icon.color = base.ViewModel.IconColor.Value;
		}
		SetIconSize(base.ViewModel.IconSize);
		if ((bool)m_IconText)
		{
			m_IconText.gameObject.SetActive(!string.IsNullOrEmpty(base.ViewModel.IconText));
		}
		if ((bool)m_IconText && !string.IsNullOrEmpty(base.ViewModel.IconText))
		{
			m_IconText.text = base.ViewModel.IconText;
		}
		if (!string.IsNullOrEmpty(base.ViewModel.ValueHint))
		{
			AddDisposable(m_Value.SetHint(base.ViewModel.ValueHint));
		}
		foreach (GameObject item in m_HasValueGroup)
		{
			item.SetActive(base.ViewModel.HasValue);
		}
		ApplyStyle();
		SetTooltip();
		if (base.ViewModel.ReactiveValue != null)
		{
			AddDisposable(base.ViewModel.ReactiveValue.Subscribe(delegate(string value)
			{
				m_Value.text = value;
			}));
		}
		if (base.ViewModel.ReactiveAddValue != null)
		{
			AddDisposable(base.ViewModel.ReactiveAddValue.Subscribe(UpdateAddValue));
		}
		SetTextSize();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_Icon.color = m_DefaultIconColor;
		SetIconSize(m_DefaultIconSize);
		m_TextHelper.Dispose();
	}

	private void SetTextSize()
	{
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_Label.fontSize = (isControllerMouse ? m_DefaultFontSizeLabel : m_DefaultConsoleFontSizeLabel) * FontMultiplier;
		m_Value.fontSize = (isControllerMouse ? m_DefaultFontSizeValue : m_DefaultConsoleFontSizeValue) * FontMultiplier;
		m_AddValue.fontSize = (isControllerMouse ? m_DefaultFontSizeAddValue : m_DefaultConsoleFontSizeAddValue) * FontMultiplier;
	}

	private void UpdateAddValue(string value)
	{
		m_AddValue.gameObject.SetActive(!value.IsNullOrEmpty());
		m_AddValue.text = value;
	}

	private void ApplyStyle()
	{
		Color color = base.ViewModel.Type switch
		{
			TooltipBrickIconStatValueType.Normal => m_NormalColor, 
			TooltipBrickIconStatValueType.Positive => m_PositiveColor, 
			TooltipBrickIconStatValueType.Negative => m_NegativeColor, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		TextMeshProUGUI value = m_Value;
		Color color3 = (m_AddValue.color = color);
		value.color = color3;
		Color color4 = base.ViewModel.BackgroundType switch
		{
			TooltipBrickIconStatValueType.Normal => m_NormalBackgroundColor, 
			TooltipBrickIconStatValueType.Positive => m_PositiveBackgroundColor, 
			TooltipBrickIconStatValueType.Negative => m_NegativeBackgroundColor, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if ((bool)m_Background)
		{
			m_Background.color = color4;
			RectTransform component = m_Background.GetComponent<RectTransform>();
			Vector2 sizeDelta = component.sizeDelta;
			if (base.ViewModel.BackgroundType != TooltipBrickIconStatValueType.Positive && m_IsExpansed)
			{
				sizeDelta.x -= m_NonNormalWidthExpanse;
				m_IsExpansed = false;
			}
			else if (base.ViewModel.BackgroundType == TooltipBrickIconStatValueType.Positive && !m_IsExpansed)
			{
				sizeDelta.x += m_NonNormalWidthExpanse;
				m_IsExpansed = true;
			}
			component.sizeDelta = sizeDelta;
		}
		m_Label.fontStyle = ((base.ViewModel.TextStyle == TooltipBrickIconStatValueStyle.Bold) ? FontStyles.Bold : FontStyles.Normal);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	private void SetTooltip()
	{
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
	}

	private void SetIconSize(float? width)
	{
		LayoutElement component = m_Icon.GetComponent<LayoutElement>();
		if ((bool)component)
		{
			component.minWidth = width ?? m_DefaultIconSize;
		}
	}
}
