using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Tutorial;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Kingmaker.UI.Sound;
using Kingmaker.Utility;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tutorial.PC;

public abstract class TutorialWindowBaseView<TViewModel> : ViewBase<TViewModel>, ISettingsFontSizeUIHandler, ISubscriber where TViewModel : TutorialWindowVM
{
	[SerializeField]
	protected TextMeshProUGUI m_Title;

	[Space]
	[SerializeField]
	private GameObject m_ImageContainer;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private VideoPlayerHelper m_VideoPlayerHelper;

	[SerializeField]
	private Sprite m_DefaultSprite;

	[Space]
	[SerializeField]
	protected TextMeshProUGUI m_TriggerText;

	[SerializeField]
	protected TextMeshProUGUI m_TutorialText;

	[SerializeField]
	protected TextMeshProUGUI m_SolutionText;

	[Space]
	[SerializeField]
	protected OwlcatToggle m_DontShowToggle;

	[SerializeField]
	protected TextMeshProUGUI m_DontShowLabel;

	[Space]
	[SerializeField]
	private WindowAnimator m_WindowAnimator;

	[SerializeField]
	private ScrollRectExtended m_BodyScrollRect;

	[SerializeField]
	private RectTransform m_BodyContentRectTransform;

	protected virtual bool IsShowDefaultSprite => false;

	protected virtual bool IsBigTutorial => true;

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_WindowAnimator.Initialize();
		if (m_VideoPlayerHelper != null)
		{
			m_VideoPlayerHelper.Initialize();
		}
		Show();
		AddDisposable(m_TriggerText.SetLinkTooltip());
		AddDisposable(m_TutorialText.SetLinkTooltip());
		AddDisposable(m_SolutionText.SetLinkTooltip());
		UITutorial tutorial = UIStrings.Instance.Tutorial;
		m_DontShowToggle.gameObject.SetActive(base.ViewModel.CanBeBanned);
		m_DontShowToggle.Set(value: false);
		UISounds.Instance.SetClickAndHoverSound(m_DontShowToggle.ConsoleEntityProxy as OwlcatMultiButton, UISounds.ButtonSoundsEnum.NoSound);
		if (base.ViewModel.CanBeBanned && !(m_DontShowLabel == null))
		{
			if (base.ViewModel.BanTutorInsteadOfTag)
			{
				m_DontShowLabel.text = tutorial.DontShowThisTutorial.Text;
			}
			else
			{
				m_DontShowLabel.text = string.Format(tutorial.DontShowTutorialTag.Text, (!base.ViewModel.TutorialTag.HasValue) ? null : tutorial.TagNames.GetTagName(base.ViewModel.TutorialTag.Value)?.Text);
			}
			AddDisposable(EventBus.Subscribe(this));
		}
	}

	protected override void DestroyViewImplementation()
	{
		OnHide();
		if (m_DontShowToggle.IsOn.Value)
		{
			base.ViewModel.BanTutor();
		}
		m_DontShowToggle.Set(value: false);
		SetActive(show: false);
		foreach (TutorialData.Page page in base.ViewModel.Data.Pages)
		{
			page.Picture?.ForceUnload();
			page.Video?.ForceUnload();
		}
	}

	public virtual void Show()
	{
		m_WindowAnimator.AppearAnimation();
		OnShow();
	}

	public void SetActive(bool show)
	{
		base.gameObject.SetActive(show);
		SetPage(null);
	}

	protected virtual void OnShow()
	{
		AddDisposable(EscHotkeyManager.Instance.Subscribe(OnEscPressed));
	}

	protected virtual void OnHide()
	{
		EscHotkeyManager.Instance.Unsubscribe(OnEscPressed);
	}

	private void OnEscPressed()
	{
		base.ViewModel.Hide();
	}

	protected void SetPage(TutorialData.Page page)
	{
		TViewModel viewModel = base.ViewModel;
		if (viewModel != null)
		{
			_ = viewModel.FontSizeMultiplier;
			if (true)
			{
				SetTextsSize(base.ViewModel.FontSizeMultiplier);
			}
		}
		m_Title.text = page?.Title;
		m_ImageContainer.gameObject.SetActive((page?.Picture != null && page.Picture.Exists()) || (page?.Video != null && page.Video.Exists()) || m_DefaultSprite != null);
		if (IsShowDefaultSprite)
		{
			m_Image.gameObject.SetActive(page != null && (page.Video == null || !page.Video.Exists()));
			if (page != null && (page.Video == null || !page.Video.Exists()))
			{
				m_Image.sprite = ((page.Picture != null && page.Picture.Exists()) ? page.Picture.Load() : m_DefaultSprite);
			}
		}
		else
		{
			m_Image.gameObject.SetActive(page?.Picture != null && page.Picture.Exists());
			if (page?.Picture != null)
			{
				m_Image.sprite = page.Picture.Load();
			}
		}
		bool valueOrDefault = (page?.Video?.Exists()).GetValueOrDefault();
		if (m_VideoPlayerHelper != null)
		{
			m_VideoPlayerHelper.gameObject.SetActive(valueOrDefault);
			if (valueOrDefault)
			{
				m_VideoPlayerHelper.SetClip(page.Video.Load(), SoundStateType.Video, prepareVideo: false, null, null);
			}
		}
		m_TriggerText.text = page?.TriggerText;
		m_TriggerText.gameObject.SetActive(!string.IsNullOrEmpty(page?.TriggerText));
		m_TutorialText.text = page?.Description;
		m_TutorialText.gameObject.SetActive(!string.IsNullOrEmpty(page?.Description));
		m_SolutionText.text = page?.SolutionText;
		m_SolutionText.gameObject.SetActive(!string.IsNullOrEmpty(page?.SolutionText));
	}

	protected virtual void SetTextsSize(float multiplier)
	{
		if (m_BodyContentRectTransform != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_BodyContentRectTransform);
		}
		m_BodyScrollRect.Or(null)?.ScrollToTop();
	}

	public void HandleChangeFontSizeSettings(float size)
	{
		SetTextsSize(size);
	}
}
