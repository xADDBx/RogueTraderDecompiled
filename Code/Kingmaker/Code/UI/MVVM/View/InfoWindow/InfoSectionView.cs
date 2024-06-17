using System;
using JetBrains.Annotations;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.InfoWindow;

public class InfoSectionView : ViewBase<InfoSectionVM>, IConsoleNavigationOwner, IConsoleEntity, INavigationDownDirectionHandler
{
	[UsedImplicitly]
	private bool m_IsInit;

	[SerializeField]
	[UsedImplicitly]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	[UsedImplicitly]
	private InfoBodyView m_InfoBodyView;

	[SerializeField]
	[UsedImplicitly]
	private ScrollRectExtended m_ScrollRectExtended;

	private GridConsoleNavigationBehaviour m_Navigation;

	private IDisposable m_NavigationFocusSubscription;

	private readonly ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>(initialValue: true);

	public bool IsScrollActive => m_ScrollRectExtended.verticalScrollbar.IsActive();

	public bool ScrollbarOnBottom => m_ScrollRectExtended.verticalNormalizedPosition < 0.01f;

	public bool ScrollbarOnTop => m_ScrollRectExtended.verticalNormalizedPosition > 0.99f;

	public ScrollRectExtended ScrollRectExtended => m_ScrollRectExtended;

	public bool HasScroll => m_ScrollRectExtended.content.sizeDelta.y >= m_ScrollRectExtended.viewport.sizeDelta.y;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			base.gameObject.SetActive(value: false);
			m_FadeAnimator.Initialize();
			m_IsInit = true;
		}
	}

	public void SetActive(bool state)
	{
		m_IsActive.Value = state;
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		AddDisposable(base.ViewModel.InfoVM.ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			TooltipDataChanged();
		}));
		AddDisposable(m_IsActive.ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			TooltipDataChanged();
		}));
	}

	private void Show()
	{
		m_FadeAnimator.AppearAnimation();
		m_ScrollRectExtended.Or(null)?.ScrollToTop();
	}

	public void Hide()
	{
		m_FadeAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	private void TooltipDataChanged()
	{
		if (base.ViewModel.InfoVM.Value == null || !m_IsActive.Value)
		{
			Hide();
		}
		else
		{
			Show();
		}
		m_InfoBodyView.Bind(base.ViewModel.InfoVM.Value);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		m_NavigationFocusSubscription?.Dispose();
		m_NavigationFocusSubscription = null;
		UILog.ViewUnbinded("TooltipSectionView");
	}

	public void Scroll(float x)
	{
		if (!(m_ScrollRectExtended == null))
		{
			PointerEventData data = new PointerEventData(EventSystem.current)
			{
				scrollDelta = new Vector2(0f, x * m_ScrollRectExtended.scrollSensitivity)
			};
			m_ScrollRectExtended.OnSmoothlyScroll(data);
		}
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		MonoBehaviour monoBehaviour2 = ((entity is MonoBehaviour monoBehaviour) ? monoBehaviour : ((entity is SimpleConsoleNavigationEntity simpleConsoleNavigationEntity) ? simpleConsoleNavigationEntity.MonoBehaviour : ((!(entity is TMPLinkNavigationEntity tMPLinkNavigationEntity)) ? null : tMPLinkNavigationEntity.MonoBehaviour)));
		MonoBehaviour monoBehaviour3 = monoBehaviour2;
		if (monoBehaviour3 != null)
		{
			m_ScrollRectExtended.EnsureVisibleVertical(monoBehaviour3.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		TooltipDataChanged();
		m_Navigation = m_InfoBodyView.GetNavigationBehaviour(this);
		m_NavigationFocusSubscription?.Dispose();
		m_NavigationFocusSubscription = m_Navigation.DeepestFocusAsObservable.Subscribe(OnFocusChanged);
		return m_Navigation;
	}

	public void EntityFocused(IConsoleEntity entity)
	{
	}

	public bool HandleDown()
	{
		float num = 1f;
		if (ScrollRectExtended.verticalNormalizedPosition > 0.01f)
		{
			Scroll(0f - num);
			return true;
		}
		return false;
	}
}
