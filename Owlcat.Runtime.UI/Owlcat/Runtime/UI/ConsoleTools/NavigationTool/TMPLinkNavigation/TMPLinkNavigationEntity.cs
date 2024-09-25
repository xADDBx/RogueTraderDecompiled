using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;

public class TMPLinkNavigationEntity : IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IMonoBehaviour, IHasTooltipTemplate
{
	private TextMeshProUGUI m_Text;

	private OwlcatMultiButton m_FirstFocus;

	private OwlcatMultiButton m_SecondFocus;

	private Vector2 m_FirstLinePosition;

	private Vector2 m_SecondLinePosition;

	private Vector2 m_FirstLineSize;

	private Vector2 m_SecondLineSize;

	protected string m_LinkId;

	private Action<string> m_OnLinkClicked;

	private Action<string> m_OnLinkFocused;

	private readonly Func<string, TooltipBaseTemplate> m_GetTooltipTemplate;

	private IDisposable m_Disposable;

	private bool HasSecondLine => m_SecondLinePosition != Vector2.zero;

	public MonoBehaviour MonoBehaviour => m_FirstFocus;

	public TMPLinkNavigationEntity(TextMeshProUGUI text, OwlcatMultiButton firstFocus, OwlcatMultiButton secondFocus, GlossaryPoint glossaryPoint, Action<string> onLinkClicked = null, Action<string> onLinkFocused = null, Func<string, TooltipBaseTemplate> getTooltipTemplate = null)
	{
		m_Text = text;
		m_FirstFocus = firstFocus;
		m_SecondFocus = secondFocus;
		m_FirstLinePosition = glossaryPoint.StartTextCoordinate;
		m_SecondLinePosition = glossaryPoint.FinishTextCoordinate;
		m_FirstLineSize = glossaryPoint.StartSize;
		m_SecondLineSize = glossaryPoint.FinishSize;
		m_LinkId = glossaryPoint.LinkID;
		m_OnLinkClicked = onLinkClicked;
		m_OnLinkFocused = onLinkFocused;
		m_GetTooltipTemplate = getTooltipTemplate;
	}

	public bool CanConfirmClick()
	{
		return true;
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public void OnConfirmClick()
	{
		OnLinkClicked();
	}

	protected virtual void OnLinkClicked()
	{
		m_OnLinkClicked?.Invoke(m_LinkId);
	}

	public virtual void SetFocus(bool value)
	{
		m_FirstFocus.gameObject.SetActive(value);
		m_SecondFocus.gameObject.SetActive(value && HasSecondLine);
		m_Disposable?.Dispose();
		m_Disposable = null;
		if (value)
		{
			m_Disposable = GamePad.Instance.OnLayerPoped.Subscribe(delegate
			{
				DelayedInvoker.InvokeInFrames(OnLinkFocused, 1);
			});
			OnLinkFocused();
			m_FirstFocus.SetFocus(value: true);
			RectTransform obj = (RectTransform)m_FirstFocus.transform;
			obj.parent = m_Text.transform;
			obj.localPosition = m_FirstLinePosition;
			obj.sizeDelta = new Vector2(m_FirstLineSize.x + 10f, m_FirstLineSize.y + 5f);
			if (HasSecondLine)
			{
				m_SecondFocus.SetFocus(value: true);
				RectTransform obj2 = (RectTransform)m_SecondFocus.transform;
				obj2.parent = m_Text.transform;
				obj2.localPosition = m_SecondLinePosition;
				obj2.sizeDelta = m_SecondLineSize;
			}
		}
	}

	protected virtual void OnLinkFocused()
	{
		m_OnLinkFocused?.Invoke(m_LinkId);
	}

	public bool IsValid()
	{
		return true;
	}

	public Vector2 GetPosition()
	{
		return m_Text.transform.TransformPoint(m_FirstLinePosition);
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public virtual TooltipBaseTemplate TooltipTemplate()
	{
		return m_GetTooltipTemplate?.Invoke(m_LinkId);
	}

	~TMPLinkNavigationEntity()
	{
		m_Disposable?.Dispose();
	}
}
