using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SectorMap.AllSystemsInformationWindow;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap.AllSystemsInformationWindow.Base;

public class AllSystemsInformationWindowBaseView : ViewBase<AllSystemsInformationWindowVM>
{
	[SerializeField]
	private TextMeshProUGUI m_AllSystemsLabel;

	[SerializeField]
	protected WidgetListMVVM m_SystemsWidgetList;

	[Header("Show Hide Move")]
	[SerializeField]
	protected float m_ShowPosX = 25f;

	[Header("Show Hide Move")]
	[SerializeField]
	protected float m_HidePosX = -600f;

	[Header("Scrollbar")]
	[SerializeField]
	protected ScrollRectExtended m_ScrollRectExtended;

	private RectTransform m_RectTransform;

	protected RectTransform RectTransform => m_RectTransform = (m_RectTransform ? m_RectTransform : GetComponent<RectTransform>());

	public void Initialize()
	{
		RectTransform.anchoredPosition = new Vector2(m_HidePosX, RectTransform.anchoredPosition.y);
		base.ViewModel.ShowAllSystemsWindow.Value = false;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ShowAllSystemsWindow.Subscribe(ShowHideWindow));
		m_AllSystemsLabel.text = UIStrings.Instance.GlobalMap.KnownStarSystems;
	}

	protected override void DestroyViewImplementation()
	{
	}

	protected virtual void ShowHideWindow(bool state)
	{
		if (!state)
		{
			RectTransform.DOAnchorPosX(m_HidePosX, 0.5f);
			base.ViewModel.ShowAllSystemsWindow.Value = false;
			return;
		}
		if (Game.Instance.IsControllerMouse)
		{
			RectTransform.DOAnchorPosX(m_ShowPosX + 25f, 0.5f);
			DelayedInvoker.InvokeInTime(delegate
			{
				RectTransform.DOAnchorPosX(m_ShowPosX, 0.1f);
			}, 0.5f);
		}
		else
		{
			RectTransform.DOAnchorPosX(m_ShowPosX, 0.5f);
		}
		DrawSystems();
		SetScrollbarSettings();
	}

	protected virtual void DrawSystems()
	{
		m_SystemsWidgetList.Clear();
		m_SystemsWidgetList.gameObject.SetActive(base.ViewModel.Systems.Any());
	}

	protected void CloseInformationWindow()
	{
		base.ViewModel.CloseWindow();
	}

	private void SetScrollbarSettings()
	{
		m_ScrollRectExtended.ScrollToTop();
	}
}
