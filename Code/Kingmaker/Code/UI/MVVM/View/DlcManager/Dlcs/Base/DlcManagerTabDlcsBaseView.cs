using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Dlcs;
using Kingmaker.UI.Common;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Base;

public class DlcManagerTabDlcsBaseView : ViewBase<DlcManagerTabDlcsVM>
{
	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_DlcDescription;

	[SerializeField]
	private List<Image> m_DlcDescriptionBackgroundGradients;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("Art")]
	[SerializeField]
	private Image m_DlcArt;

	[SerializeField]
	private VideoPlayerHelper m_DlcVideo;

	[Header("Bottom Block")]
	[SerializeField]
	private TextMeshProUGUI m_YouDontHaveThisDlc;

	[SerializeField]
	protected OwlcatButton m_PurchaseButton;

	[SerializeField]
	private TextMeshProUGUI m_PurchaseLabel;

	[SerializeField]
	private TextMeshProUGUI m_PurchasedLabel;

	[SerializeField]
	private TextMeshProUGUI m_ComingSoonLabel;

	[Header("Dlcs Block")]
	[SerializeField]
	private ScrollRectExtended m_ScrollRectDlcs;

	private bool m_IsInit;

	public virtual void Initialize()
	{
		if (!m_IsInit)
		{
			m_DlcVideo.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		ScrollToTop();
		AddDisposable(base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
			if (value)
			{
				ScrollToTop();
				UpdateDlcEntities();
			}
		}));
		AddDisposable(base.ViewModel.ChangeStory.Subscribe(delegate
		{
			VideoClip videoClip = base.ViewModel.Video?.Value;
			m_DlcArt.gameObject.SetActive(base.ViewModel.Art.Value != null);
			m_DlcVideo.gameObject.SetActive(videoClip != null);
			if (base.ViewModel.Art.Value != null)
			{
				m_DlcArt.sprite = base.ViewModel.Art.Value;
			}
			if (m_DlcVideo.VideoClip != videoClip)
			{
				m_DlcVideo.Stop();
				m_DlcVideo.SetClip(videoClip);
			}
			m_DlcDescription.text = base.ViewModel.Description.Value;
			m_ScrollRect.Or(null)?.ScrollToTop();
			CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.Value, base.ViewModel.DlcIsBought.Value);
		}));
		AddDisposable((from value in m_ScrollRect.verticalScrollbar.OnValueChangedAsObservable()
			select 1f - value).Subscribe(delegate(float invertedValue)
		{
			foreach (Image dlcDescriptionBackgroundGradient in m_DlcDescriptionBackgroundGradients)
			{
				Color color = dlcDescriptionBackgroundGradient.color;
				color.a = ((!m_ScrollRect.verticalScrollbar.gameObject.activeSelf) ? 0f : invertedValue);
				dlcDescriptionBackgroundGradient.color = color;
			}
		}));
		m_YouDontHaveThisDlc.text = UIStrings.Instance.ProfitFactorTexts.AvailableToUseValue;
		m_PurchaseLabel.text = UIStrings.Instance.DlcManager.Purchase;
		m_PurchasedLabel.text = UIStrings.Instance.DlcManager.Purchased;
		m_ComingSoonLabel.text = UIStrings.Instance.DlcManager.ComingSoon;
		CheckAvailableState(base.ViewModel.DlcIsAvailableToPurchase.Value, base.ViewModel.DlcIsBought.Value);
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void UpdateDlcEntities()
	{
		UpdateDlcEntitiesImpl();
	}

	protected virtual void UpdateDlcEntitiesImpl()
	{
	}

	private void CheckAvailableState(bool canPurchase, bool available)
	{
		m_YouDontHaveThisDlc.transform.parent.gameObject.SetActive(!available && canPurchase);
		m_PurchaseButton.gameObject.SetActive(!available && canPurchase);
		m_PurchasedLabel.transform.parent.gameObject.SetActive(available && canPurchase);
		m_ComingSoonLabel.transform.parent.gameObject.SetActive(!available && !canPurchase);
		CheckAvailableStateImpl(canPurchase, available);
	}

	protected virtual void CheckAvailableStateImpl(bool canPurchase, bool available)
	{
	}

	public void ScrollToTop()
	{
		m_ScrollRect.Or(null)?.ScrollToTop();
		m_ScrollRectDlcs.Or(null)?.ScrollToTop();
	}

	public void ScrollList(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRectDlcs.Or(null)?.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}
}
