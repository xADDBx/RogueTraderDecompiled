using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Blocks;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Base;

public class EncyclopediaPageBaseView : ViewBase<EncyclopediaPageVM>
{
	[Header("Header")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[Header("Body")]
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	public ScrollRectExtended m_ScrollRect;

	[Header("Animator")]
	[SerializeField]
	[UsedImplicitly]
	public FadeAnimator ContentAnimator;

	[Header("Blocks")]
	[SerializeField]
	[UsedImplicitly]
	private EncyclopediaPageBlockTextPCView m_TextBlockPrefab;

	[SerializeField]
	[UsedImplicitly]
	private EncyclopediaPageBlockImagePCView m_ImageBlockPrefab;

	[SerializeField]
	[UsedImplicitly]
	private EncyclopediaPageBlockUnitPCView m_UnitBlockPrefab;

	[SerializeField]
	[UsedImplicitly]
	private EncyclopediaPageBlockChildPagesPCView m_ChildPagesPrefab;

	[SerializeField]
	private EncyclopediaPageBlockBookEventPCView m_BookEventHistoryBlockPrefab;

	[SerializeField]
	private EncyclopediaPageBlockGlossaryEntryPCView m_GlossaryEntryBlockPrefab;

	[SerializeField]
	private EncyclopediaPageBlockPlanetPCView m_PlanetBlockPrefab;

	[SerializeField]
	private EncyclopediaPageBlockAstropathBriefPCView m_AstropathBriefBlockPrefab;

	[Header("Page Addition")]
	[SerializeField]
	private TextMeshProUGUI m_PageAdditionText;

	[SerializeField]
	private float m_DefaultFontSizeTitle = 25f;

	[SerializeField]
	private float m_DefaultFontSizeAdditionalText = 21f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeTitle = 25f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeAdditionalText = 21f;

	[UsedImplicitly]
	private bool m_IsInit;

	private List<IWidgetView> m_Prefabs;

	private bool m_ContentRefreshing;

	private bool m_IsShowed;

	private IDisposable m_Disposable;

	public WidgetListMVVM WidgetList => m_WidgetList;

	public ScrollRectExtended ScrollRect => m_ScrollRect;

	public TextMeshProUGUI PageAdditionText => m_PageAdditionText;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
			m_Prefabs = new List<IWidgetView> { m_TextBlockPrefab, m_ImageBlockPrefab, m_UnitBlockPrefab, m_ChildPagesPrefab, m_BookEventHistoryBlockPrefab, m_GlossaryEntryBlockPrefab, m_PlanetBlockPrefab, m_AstropathBriefBlockPrefab };
			ContentAnimator.Initialize();
			Hide();
			base.gameObject.SetActive(value: false);
		}
	}

	protected override void BindViewImplementation()
	{
		if (m_IsShowed)
		{
			OnContentChanged();
		}
		else
		{
			Show();
		}
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		m_ContentRefreshing = false;
	}

	private void OnContentChanged()
	{
		if (!m_ContentRefreshing)
		{
			m_ContentRefreshing = true;
			ContentAnimator.DisappearAnimation(delegate
			{
				ContentAnimator.AppearAnimation();
				UpdateView();
				m_ContentRefreshing = false;
			});
		}
	}

	private void Show()
	{
		if (!m_ContentRefreshing)
		{
			m_ContentRefreshing = true;
			ContentAnimator.AppearAnimation();
			UpdateView();
			m_IsShowed = true;
			m_ContentRefreshing = false;
		}
	}

	private void Hide()
	{
		ContentAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		m_IsShowed = false;
		m_Disposable?.Dispose();
		m_Disposable = null;
	}

	private void UpdateView()
	{
		SetupHeader();
		SetupBody();
		ScrollToTop();
	}

	private void SetupHeader()
	{
		m_Title.text = base.ViewModel.Title;
		m_Title.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSizeTitle : m_DefaultConsoleFontSizeTitle) * base.ViewModel.FontMultiplier;
	}

	private void SetupBody()
	{
		m_WidgetList.Clear();
		m_WidgetList.DrawMultiEntries(base.ViewModel.BlockVMs.ToArray(), m_Prefabs);
		m_PageAdditionText.transform.parent.gameObject.gameObject.SetActive(value: false);
		SetAdditionalText();
	}

	private void SetAdditionalText()
	{
		if (!string.IsNullOrWhiteSpace(base.ViewModel.GlossaryText))
		{
			m_PageAdditionText.gameObject.transform.parent.gameObject.SetActive(value: true);
			m_Disposable?.Dispose();
			m_PageAdditionText.text = base.ViewModel.GlossaryText;
			m_PageAdditionText.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSizeAdditionalText : m_DefaultConsoleFontSizeAdditionalText) * base.ViewModel.FontMultiplier;
			m_Disposable = m_PageAdditionText.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true));
		}
	}

	public void ScrollToTop()
	{
		m_ScrollRect.ScrollToTop();
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}
}
