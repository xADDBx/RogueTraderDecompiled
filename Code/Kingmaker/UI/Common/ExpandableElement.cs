using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.Common;

public class ExpandableElement : MonoBehaviour, IDisposable, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplate
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private Transform m_ExpandArrow;

	[Tooltip("Optionally")]
	[SerializeField]
	private GameObject m_Content;

	[Tooltip("Optionally")]
	[SerializeField]
	private FadeAnimator m_ContentAnimator;

	private BoolReactiveProperty m_IsOn = new BoolReactiveProperty();

	private List<IDisposable> m_Disposables = new List<IDisposable>();

	private Action m_OnExpand;

	private Action m_OnCollapse;

	private Action<bool> m_OnFocus;

	private TooltipBaseTemplate m_Tooltip;

	public string Key;

	public string ParentKey;

	private bool m_CanConfirm = true;

	public BoolReactiveProperty IsOn => m_IsOn;

	public void Initialize(Action onExpand = null, Action onCollapse = null, Action<bool> onFocus = null, TooltipBaseTemplate tooltip = null, string key = null, string parentKey = null)
	{
		Dispose();
		SetFocus(value: false);
		m_Disposables.Add(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			m_IsOn.Value = !m_IsOn.Value;
		}));
		m_Disposables.Add(m_Button.OnConfirmClickAsObservable().Subscribe(delegate
		{
			m_IsOn.Value = !m_IsOn.Value;
		}));
		m_Disposables.Add(m_IsOn.Subscribe(OnChanged));
		m_OnExpand = onExpand;
		m_OnCollapse = onCollapse;
		m_OnFocus = onFocus;
		m_Tooltip = tooltip;
		Key = key;
		ParentKey = parentKey;
	}

	private void OnChanged(bool isOn)
	{
		m_Content.Or(null)?.SetActive(isOn);
		if (isOn)
		{
			m_ContentAnimator.Or(null)?.AppearAnimation();
			m_OnExpand?.Invoke();
		}
		else
		{
			m_ContentAnimator.Or(null)?.DisappearAnimation();
			m_OnCollapse?.Invoke();
		}
		m_ExpandArrow.Or(null)?.DOLocalRotate(new Vector3(0f, 0f, isOn ? 0f : 90f), 0.2f).SetUpdate(isIndependentUpdate: true);
	}

	public void Expand()
	{
		m_IsOn.Value = true;
	}

	public void Collapse()
	{
		m_IsOn.Value = false;
	}

	public void Dispose()
	{
		m_Disposables.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
		m_Disposables.Clear();
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
		m_OnFocus?.Invoke(value);
		m_Button.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public void SetCustomLayer(string layer)
	{
		m_Button.SetActiveLayer(layer);
	}

	public bool IsValid()
	{
		if (base.gameObject.activeInHierarchy)
		{
			return m_Button.IsValid();
		}
		return false;
	}

	public bool CanConfirmClick()
	{
		return m_CanConfirm;
	}

	public void SetCanConfirmClick(bool value)
	{
		m_CanConfirm = value;
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public void OnConfirmClick()
	{
		m_Button.OnConfirmClick();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return m_Tooltip;
	}
}
