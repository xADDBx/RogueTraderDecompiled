using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.PC;

public class NewGamePCView : NewGameBaseView
{
	[Header("Views")]
	[SerializeField]
	private NewGamePhaseStoryPCView m_NewGamePhaseStoryPCView;

	[SerializeField]
	private NewGamePhaseDifficultyPCView m_NewGamePhaseDifficultyPCView;

	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private OwlcatButton m_BackButton;

	[SerializeField]
	private OwlcatButton m_NextButton;

	[Header("Texts")]
	[SerializeField]
	private TextMeshProUGUI m_BackButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_NextButtonLabel;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_NewGamePhaseDifficultyPCView.Initialize();
		m_Selector.Initialize();
		m_NewGamePhaseStoryPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnBack));
		SetButtonsSounds();
		AddDisposable(base.ViewModel.StoryVM.IsNextButtonAvailable.Subscribe(m_NextButton.SetInteractable));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnBack();
		}));
		AddDisposable(m_BackButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnButtonBack();
		}));
		AddDisposable(m_NextButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnButtonNext();
		}));
		AddDisposable(m_CloseButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnBack();
		}));
		AddDisposable(m_BackButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnButtonBack();
		}));
		AddDisposable(m_NextButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnButtonNext();
		}));
		m_NewGamePhaseStoryPCView.Bind(base.ViewModel.StoryVM);
		m_NewGamePhaseDifficultyPCView.Bind(base.ViewModel.DifficultyVM);
		m_BackButtonLabel.text = UIStrings.Instance.ContextMenu.Back;
		m_NextButtonLabel.text = UIStrings.Instance.MainMenu.Continue;
	}

	private void SetButtonsSounds()
	{
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_BackButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_NextButton, UISounds.ButtonSoundsEnum.PlastickSound);
	}
}
