using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.EscMenu;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.DLC;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.EscMenu;

public abstract class EscMenuBaseView : ViewBase<EscMenuVM>
{
	[Header("Common Buttons")]
	[SerializeField]
	protected OwlcatButton m_SaveButton;

	[SerializeField]
	protected OwlcatButton m_LoadButton;

	[SerializeField]
	protected OwlcatButton m_MultiplayerButton;

	[SerializeField]
	protected OwlcatButton m_MultiplayerRolesButton;

	[SerializeField]
	protected OwlcatButton m_FormationButton;

	[SerializeField]
	protected OwlcatButton m_OptionsButton;

	[SerializeField]
	protected OwlcatButton m_ModsButton;

	[SerializeField]
	protected OwlcatButton m_BugReportButton;

	[SerializeField]
	protected OwlcatButton m_MainMenuButton;

	[SerializeField]
	protected OwlcatButton m_QuitButton;

	[Header("Common Labels")]
	[SerializeField]
	private TextMeshProUGUI m_SaveButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_LoadButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_MultiplayerButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_MultiplayerRolesButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_FormationButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_OptionsButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_ModsButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_BugReportButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_MainMenuButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_QuitButtonLabel;

	[Header("Another")]
	[SerializeField]
	private TextMeshProUGUI m_CanSwitchOnDlcsCount;

	protected GridConsoleNavigationBehaviour NavigationBehaviour;

	protected InputLayer InputLayer;

	public static readonly string InputLayerContextName = "EscMenu";

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		Game.Instance.RequestPauseUi(isPaused: true);
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.Systems.FullscreenWindowFadeShow.Play();
		AddDisposable(base.ViewModel.IsInSpace.Subscribe(delegate(bool value)
		{
			m_FormationButton.gameObject.SetActive(!value);
		}));
		if (BuildModeUtility.IsCoopEnabled)
		{
			m_MultiplayerButton.gameObject.SetActive(value: true);
			m_MultiplayerRolesButton.gameObject.SetActive(PhotonManager.Lobby.IsActive);
			m_MultiplayerRolesButton.SetInteractable(!Game.Instance.IsSpaceCombat && base.ViewModel.IsSavingAllowed);
			AddDisposable(m_MultiplayerButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.OnMultiplayer();
			}));
			AddDisposable(m_MultiplayerRolesButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.OnMultiplayerRoles();
			}));
			AddDisposable(m_MultiplayerButton.OnConfirmClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.OnMultiplayer();
			}));
			AddDisposable(m_MultiplayerRolesButton.OnConfirmClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.OnMultiplayerRoles();
			}));
		}
		else
		{
			m_MultiplayerButton.gameObject.SetActive(value: false);
			m_MultiplayerRolesButton.gameObject.SetActive(value: false);
			m_MultiplayerRolesButton.SetInteractable(state: false);
		}
		m_QuitButton.gameObject.SetActive(value: true);
		AddDisposable(m_QuitButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			LeftClickAndHideFocusAction(delegate
			{
				base.ViewModel.OnQuit();
			});
		}));
		AddDisposable(m_QuitButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnQuit();
		}));
		bool isActive = PhotonManager.Lobby.IsActive;
		m_ModsButton.gameObject.SetActive(!isActive);
		IEnumerable<IBlueprintDlc> source = Game.Instance.Player.GetAvailableAdditionalContentDlcForCurrentCampaign().Where(delegate(IBlueprintDlc dlc)
		{
			BlueprintDlc blueprintDlc = dlc as BlueprintDlc;
			return (blueprintDlc == null || !blueprintDlc.CheckIsLateToSwitch()) && !(blueprintDlc?.GetDlcSwitchOnOffState() ?? false);
		});
		m_CanSwitchOnDlcsCount.transform.parent.gameObject.SetActive(!isActive && source.Any());
		if (!isActive && source.Any())
		{
			m_CanSwitchOnDlcsCount.text = source.Count().ToString();
		}
		if (!isActive)
		{
			AddDisposable(m_ModsButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.OnMods();
			}));
			AddDisposable(m_ModsButton.OnConfirmClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.OnMods();
			}));
		}
		SetButtonsTexts();
		AddDisposable(m_SaveButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnSave();
		}));
		AddDisposable(m_LoadButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnLoad();
		}));
		AddDisposable(m_FormationButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OpenFormation();
		}));
		AddDisposable(m_OptionsButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnSettings();
		}));
		AddDisposable(m_BugReportButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnBugReport();
		}));
		AddDisposable(m_MainMenuButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			LeftClickAndHideFocusAction(delegate
			{
				base.ViewModel.OnMainMenu();
			});
		}));
		AddDisposable(m_SaveButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnSave();
		}));
		AddDisposable(m_LoadButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnLoad();
		}));
		AddDisposable(m_FormationButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OpenFormation();
		}));
		AddDisposable(m_OptionsButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnSettings();
		}));
		AddDisposable(m_BugReportButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnBugReport();
		}));
		AddDisposable(m_MainMenuButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnMainMenu();
		}));
		m_SaveButton.SetInteractable(base.ViewModel.IsSavingAllowed);
		m_FormationButton.SetInteractable(base.ViewModel.IsFormationAllowed);
		m_OptionsButton.SetInteractable(base.ViewModel.IsOptionsAllowed);
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			AddDisposable(m_SaveButton.SetHint(UIStrings.Instance.SaveLoadTexts.SaveIsNotPossibleInIronMan));
		}
		BuildNavigation();
		AddDisposable(base.ViewModel.UpdateButtonsInteractable.Subscribe(delegate
		{
			UpdateInteractableButtons();
		}));
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.EscapeMenu);
		});
	}

	protected override void DestroyViewImplementation()
	{
		if (!base.ViewModel.InternalWindowOpened)
		{
			Game.Instance.RequestPauseUi(isPaused: false);
		}
		base.ViewModel.InternalWindowOpened = false;
		base.gameObject.SetActive(value: false);
		UISounds.Instance.Sounds.Systems.FullscreenWindowFadeHide.Play();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.EscapeMenu);
		});
	}

	private void LeftClickAndHideFocusAction(Action action)
	{
		base.ViewModel.UpdateButtonsFocus.Execute();
		action();
	}

	private void UpdateInteractableButtons()
	{
		if (BuildModeUtility.IsCoopEnabled)
		{
			m_MultiplayerRolesButton.SetInteractable(!Game.Instance.IsSpaceCombat && base.ViewModel.IsSavingAllowed);
		}
		m_SaveButton.SetInteractable(base.ViewModel.IsSavingAllowed);
		m_FormationButton.SetInteractable(base.ViewModel.IsFormationAllowed);
		m_OptionsButton.SetInteractable(base.ViewModel.IsOptionsAllowed);
		UpdateInteractableButtonsImpl();
	}

	protected virtual void UpdateInteractableButtonsImpl()
	{
	}

	private void BuildNavigation()
	{
		AddDisposable(NavigationBehaviour = new GridConsoleNavigationBehaviour());
		List<OwlcatButton> list = new List<OwlcatButton> { m_SaveButton, m_LoadButton, m_FormationButton, m_OptionsButton, m_BugReportButton, m_MainMenuButton, m_QuitButton };
		if (!PhotonManager.Lobby.IsActive)
		{
			list.Add(m_ModsButton);
		}
		if (BuildModeUtility.IsCoopEnabled)
		{
			list.Add(m_MultiplayerButton);
			list.Add(m_MultiplayerRolesButton);
		}
		list.Sort((OwlcatButton x, OwlcatButton y) => x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex()));
		NavigationBehaviour.SetEntitiesVertical(new List<IConsoleEntity>(list));
		BuildNavigationImpl(NavigationBehaviour);
		InputLayer = NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerContextName
		});
		CreateInputImpl(InputLayer);
		AddDisposable(GamePad.Instance.PushLayer(InputLayer));
	}

	protected virtual void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnClose));
	}

	protected virtual void SetButtonsTexts()
	{
		m_SaveButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuSaveGame;
		m_LoadButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuLoadGame;
		m_FormationButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuFormation;
		m_MultiplayerButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuMultiplayer;
		m_MultiplayerRolesButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuRoles;
		m_QuitButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuExit;
		m_OptionsButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuOptions;
		m_BugReportButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuBugReport;
		m_MainMenuButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuMainMenu;
		bool flag = false;
		m_ModsButtonLabel.text = (flag ? UIStrings.Instance.DlcManager.DlcManagerLabel : UIStrings.Instance.DlcManager.ModsLabel);
	}
}
