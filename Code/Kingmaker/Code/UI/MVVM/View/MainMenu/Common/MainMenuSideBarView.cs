using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ContextMenu.Common;
using Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.MainMenu.Common;

public abstract class MainMenuSideBarView<TContextMenuEntityView> : ViewBase<MainMenuSideBarVM>, IInitializable where TContextMenuEntityView : ContextMenuEntityView
{
	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_ContinueView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_NewGameView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_LoadView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_NetView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_OptionsView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_CreditView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_ExitView;

	[Header("PaperButtons")]
	[SerializeField]
	protected OwlcatButton m_WebsiteButton;

	[SerializeField]
	protected OwlcatButton m_LicenceButton;

	[SerializeField]
	protected OwlcatButton m_DiscordButton;

	[Header("WelcomeText")]
	[SerializeField]
	protected GameObject m_WelcomeTextContainer;

	[SerializeField]
	protected MoveAnimator m_WelcomeTextBlock;

	[SerializeField]
	protected float m_DelayBeforeShow = 3f;

	[SerializeField]
	protected TextMeshProUGUI m_WebsiteLabel;

	[SerializeField]
	protected TextMeshProUGUI m_LicenceLabel;

	[SerializeField]
	protected TextMeshProUGUI m_DiscordLabel;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[SerializeField]
	protected TextMeshProUGUI m_MotivationText;

	public void Initialize()
	{
		m_WelcomeTextContainer.SetActive(value: false);
		m_WelcomeTextBlock.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_ContinueView.Bind(base.ViewModel.ContinueVm);
		m_NewGameView.Bind(base.ViewModel.NewGameVm);
		m_LoadView.Bind(base.ViewModel.LoadVm);
		m_OptionsView.Bind(base.ViewModel.OptionsVm);
		m_CreditView.Bind(base.ViewModel.CreditVm);
		if (BuildModeUtility.IsCoopEnabled)
		{
			m_NetView.gameObject.SetActive(value: true);
			m_NetView.Bind(base.ViewModel.NetVm);
		}
		else
		{
			m_NetView.gameObject.SetActive(value: false);
		}
		m_DiscordButton.Or(null)?.gameObject.SetActive(value: true);
		m_WebsiteButton.Or(null)?.gameObject.SetActive(value: true);
		m_DiscordButton.Or(null)?.gameObject.SetActive(value: true);
		m_ExitView.gameObject.SetActive(base.ViewModel.ExitEnabled);
		if (base.ViewModel.ExitEnabled)
		{
			m_ExitView.Bind(base.ViewModel.ExitVm);
		}
		SetTextMessageOfTheDay();
		AddDisposable(ObservableExtensions.Subscribe(m_WebsiteButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenUrl(FeedbackPopupItemType.Website);
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_LicenceButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ShowLicense();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_DiscordButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenUrl(FeedbackPopupItemType.Discord);
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.LanguageChanged, delegate
		{
			UpdateMessageOfTheDay();
		}));
		m_WelcomeTextContainer.SetActive(value: true);
		((RectTransform)m_WelcomeTextBlock.transform).anchoredPosition = new Vector2(((RectTransform)m_WelcomeTextBlock.transform).anchoredPosition.x, m_WelcomeTextBlock.MovePartY.DisappearPosition);
		DelayedInvoker.InvokeInTime(delegate
		{
			m_WelcomeTextBlock.AppearAnimation();
		}, m_DelayBeforeShow);
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetTextMessageOfTheDay()
	{
		m_WebsiteLabel.text = UIStrings.Instance.FeedbackPopupTexts.GetTitleByPopupItemType(FeedbackPopupItemType.Website);
		m_LicenceLabel.text = UIStrings.Instance.MainMenu.License;
		m_DiscordLabel.text = UIStrings.Instance.FeedbackPopupTexts.Discord;
		base.ViewModel.GetIntroductoryText(delegate(string text)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				m_MotivationText.text = text;
			}
		});
		AddDisposable(UIUtility.SetTextLink(m_MotivationText));
	}

	protected virtual void UpdateMessageOfTheDay()
	{
		SetTextMessageOfTheDay();
	}
}
