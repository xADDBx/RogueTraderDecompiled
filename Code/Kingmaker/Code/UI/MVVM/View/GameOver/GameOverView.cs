using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Common;
using Kingmaker.Code.UI.MVVM.VM.GameOver;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.GameOver;

public abstract class GameOverView : CommonStaticComponentView<GameOverVM>, IInitializable
{
	[Header("Common Labels")]
	[SerializeField]
	private TextMeshProUGUI m_ResultText;

	[SerializeField]
	private TextMeshProUGUI m_DescriptionText;

	[Header("Buttons")]
	[SerializeField]
	protected OwlcatButton m_QuickLoadButton;

	[SerializeField]
	protected OwlcatButton m_LoadButton;

	[SerializeField]
	protected OwlcatButton m_MainMenuButton;

	[SerializeField]
	protected OwlcatButton m_IronManDeleteSaveButton;

	[SerializeField]
	protected OwlcatButton m_IronManContinueGameButton;

	[Header("Buttons Labels")]
	[SerializeField]
	private TextMeshProUGUI m_QuickLoadLabel;

	[SerializeField]
	private TextMeshProUGUI m_LoadLabel;

	[SerializeField]
	private TextMeshProUGUI m_MainMenuLabel;

	[SerializeField]
	private TextMeshProUGUI m_IronManDeleteSaveLabel;

	[SerializeField]
	private TextMeshProUGUI m_IronManContinueGameLabel;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		DelayedInvoker.InvokeInTime(OnActivate, 3f);
		AddDisposable(base.ViewModel.Reason.Subscribe(delegate(string value)
		{
			m_ResultText.text = value;
		}));
		bool isIronMan = base.ViewModel.IsIronMan;
		m_DescriptionText.Or(null)?.gameObject.SetActive(isIronMan && base.ViewModel.HasDowngradedIronManSave);
		if (isIronMan && m_DescriptionText != null && base.ViewModel.HasDowngradedIronManSave)
		{
			m_DescriptionText.text = UIStrings.Instance.GameOverScreen.GameOverIronManDescription;
		}
		SetButtonsLabel();
		SetButtonVisible(isIronMan);
		AddDisposable(ObservableExtensions.Subscribe(m_QuickLoadButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnQuickLoad();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_LoadButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnButtonLoadGame();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_MainMenuButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnButtonMainMenu();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_IronManDeleteSaveButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnIronManDeleteSave();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_IronManContinueGameButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnIronManContinueGame();
		}));
		AddDisposable(m_QuickLoadButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnQuickLoad));
		AddDisposable(m_LoadButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnButtonLoadGame));
		AddDisposable(m_MainMenuButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnButtonMainMenu));
		AddDisposable(m_IronManDeleteSaveButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnIronManDeleteSave));
		AddDisposable(m_IronManContinueGameButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnIronManContinueGame));
		AddDisposable(base.ViewModel.CanQuickLoad.Subscribe(m_QuickLoadButton.SetInteractable));
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.EscapeMenu);
		});
	}

	protected override void DestroyViewImplementation()
	{
		UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
		base.gameObject.SetActive(value: false);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.EscapeMenu);
		});
	}

	protected virtual void OnActivate()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
	}

	private void SetButtonsLabel()
	{
		if (m_QuickLoadLabel != null)
		{
			m_QuickLoadLabel.text = UIStrings.Instance.GameOverScreen.QuickLoadLabel;
		}
		if (m_LoadLabel != null)
		{
			m_LoadLabel.text = UIStrings.Instance.GameOverScreen.LoadLabel;
		}
		if (m_MainMenuLabel != null)
		{
			m_MainMenuLabel.text = UIStrings.Instance.GameOverScreen.MainMenuLabel;
		}
		if (m_IronManDeleteSaveLabel != null)
		{
			m_IronManDeleteSaveLabel.text = UIStrings.Instance.GameOverScreen.IronManDeleteSaveLabel;
		}
		if (m_IronManContinueGameLabel != null)
		{
			m_IronManContinueGameLabel.text = UIStrings.Instance.GameOverScreen.IronManContinueGameLabel;
		}
	}

	private void SetButtonVisible(bool isIronMan)
	{
		m_QuickLoadButton.Or(null)?.gameObject.SetActive(!isIronMan);
		m_LoadButton.Or(null)?.gameObject.SetActive(!isIronMan);
		m_MainMenuButton.Or(null)?.gameObject.SetActive(!isIronMan || !base.ViewModel.HasDowngradedIronManSave);
		m_IronManDeleteSaveButton.Or(null)?.gameObject.SetActive(isIronMan && base.ViewModel.HasDowngradedIronManSave);
		m_IronManContinueGameButton.Or(null)?.gameObject.SetActive(isIronMan && base.ViewModel.HasDowngradedIronManSave);
	}
}
