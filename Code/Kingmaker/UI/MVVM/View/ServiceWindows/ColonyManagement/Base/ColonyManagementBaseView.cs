using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.ColonyManagement;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.Base;

public class ColonyManagementBaseView : ViewBase<ColonyManagementVM>, IColonizationChronicleHandler, ISubscriber, IColonizationEventHandler, IColonizationProjectsUIHandler, IInitializable
{
	[Header("Common")]
	[SerializeField]
	private GameObject m_Content;

	[SerializeField]
	private GameObject m_NoDataContent;

	[SerializeField]
	private TextMeshProUGUI m_NoColoniesLabel;

	[SerializeField]
	protected FlexibleLensSelectorView m_SelectorView;

	[SerializeField]
	private GameObject[] m_ObjectsToHideForDialog;

	[SerializeField]
	private CanvasGroup m_DialogOverlay;

	[Header("Doll Room")]
	[SerializeField]
	private DollRoomTargetController m_CharacterController;

	[SerializeField]
	private GameObject m_BackupPlanet;

	[Header("Points Navigation")]
	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_PointsNavigationParameters;

	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected IExplorationComponentEntity m_CurrentFocusedEntity;

	protected FloatConsoleNavigationBehaviour m_PointsNavigationBehaviour;

	private PlanetDollRoom PlanetRoom => UIDollRooms.Instance?.PlanetDollRoom;

	private List<ColoniesState.ColonyData> Colonies => Game.Instance.Player.ColoniesState.Colonies;

	public void Initialize()
	{
		m_NoColoniesLabel.text = UIStrings.Instance.ColonizationTexts.ColonyManagementNoColonies;
		m_CharacterController.Target = m_CharacterController.transform;
		InitializeImpl();
	}

	protected virtual void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		m_SelectorView.Bind(base.ViewModel.Selector);
		CreateNavigation();
		ShowWindow();
		AddDisposable(base.ViewModel.HasColonies.Subscribe(SetContentVisibility));
		AddDisposable(base.ViewModel.IsLockUIForDialog.Subscribe(SetLockUIForDialog));
		AddDisposable(base.ViewModel.CurrentColony.Subscribe(SetPlanet));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
		HideWindow();
		m_NavigationBehaviour = null;
		m_PointsNavigationBehaviour = null;
		m_InputLayer = null;
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
		AddDisposable(m_PointsNavigationBehaviour = new FloatConsoleNavigationBehaviour(m_PointsNavigationParameters));
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "ColonyManagementBaseViewInput"
		});
		CreateInputImpl(m_InputLayer);
	}

	protected void BuildNavigation()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			BuildNavigationImpl();
		}, 1);
	}

	protected virtual void BuildNavigationImpl()
	{
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer)
	{
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		OnFocusChangedImpl(entity);
	}

	protected virtual void OnFocusChangedImpl(IConsoleEntity entity)
	{
	}

	private void ShowWindow()
	{
		UISounds.Instance.Sounds.SpaceColonization.WindowOpenFromMap.Play();
		base.gameObject.SetActive(value: true);
		BuildNavigation();
		GamePad.Instance.PushLayer(m_InputLayer);
		OnShow();
	}

	private void OnShow()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.ColonyManagement);
		});
	}

	private void HideWindow()
	{
		UISounds.Instance.Sounds.SpaceColonization.ColonizationWindowClose.Play();
		m_NavigationBehaviour.Clear();
		m_PointsNavigationBehaviour.Clear();
		GamePad.Instance.PopLayer(m_InputLayer);
		OnHide();
		HidePlanet();
		base.gameObject.SetActive(value: false);
	}

	private void OnHide()
	{
		OnHideImpl();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.ColonyManagement);
		});
	}

	protected virtual void OnHideImpl()
	{
	}

	private void SetPlanet(Colony colony)
	{
		int index = Colonies.FindIndex((ColoniesState.ColonyData e) => e.Colony == colony);
		m_SelectorView.ChangeTab(index);
		if (colony == null)
		{
			HidePlanet();
		}
		else
		{
			ShowPlanet(colony);
		}
	}

	private void ShowPlanet(Colony colony)
	{
		HidePlanet();
		m_BackupPlanet.SetActive(value: true);
		BlueprintPlanet planet = colony.Planet;
		if (planet != null && Game.Instance.Player.StarSystemsState.PlanetChangedVisualPrefabs.TryGetValue(planet, out var value))
		{
			GameObject gameObject = value.PrefabLink.Load();
			if ((bool)PlanetRoom && (bool)gameObject)
			{
				m_BackupPlanet.SetActive(value: false);
				PlanetRoom.gameObject.SetActive(value: true);
				PlanetRoom.SetupPlanet(gameObject);
				PlanetRoom.Initialize(m_CharacterController);
				PlanetRoom.Show();
				m_CharacterController.gameObject.SetActive(gameObject);
				BuildNavigation();
			}
		}
	}

	private void HidePlanet()
	{
		m_BackupPlanet.SetActive(value: false);
		if (PlanetRoom != null)
		{
			PlanetRoom.Hide();
			PlanetRoom.gameObject.SetActive(value: false);
		}
		m_CharacterController.gameObject.SetActive(value: false);
	}

	private void SetContentVisibility(bool hasColonies)
	{
		m_Content.SetActive(hasColonies);
		m_NoDataContent.SetActive(!hasColonies);
		if (!hasColonies)
		{
			UISounds.Instance.Sounds.SpaceColonization.NoConnect.Play();
		}
	}

	public void SetLockUIForDialog(bool value)
	{
		if (value)
		{
			EscHotkeyManager.Instance.Subscribe(ShowEscMenu);
		}
		else
		{
			EscHotkeyManager.Instance.Unsubscribe(ShowEscMenu);
		}
		GameObject[] objectsToHideForDialog = m_ObjectsToHideForDialog;
		for (int i = 0; i < objectsToHideForDialog.Length; i++)
		{
			objectsToHideForDialog[i].SetActive(!value);
		}
		SetDialogOverlayActive(value);
		SetLockUIForDialogImpl(value);
	}

	protected virtual void SetLockUIForDialogImpl(bool value)
	{
	}

	private void SetDialogOverlayActive(bool isActive)
	{
		m_DialogOverlay.alpha = (isActive ? 1f : 0f);
		m_DialogOverlay.interactable = isActive;
		m_DialogOverlay.blocksRaycasts = isActive;
	}

	private void ShowEscMenu()
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	public void HandleChronicleStarted(Colony colony, BlueprintDialog chronicle)
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleChronicleFinished(Colony colony, ColonyChronicle chronicle)
	{
		BuildNavigation();
	}

	public void HandleEventStarted(Colony colony, BlueprintColonyEvent colonyEvent)
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleEventFinished(Colony colony, BlueprintColonyEvent colonyEvent, BlueprintColonyEventResult result)
	{
		BuildNavigation();
	}

	public void HandleColonyProjectsUIOpen(Colony colony)
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleColonyProjectsUIClose()
	{
		BuildNavigation();
	}

	public void HandleColonyProjectPage(BlueprintColonyProject blueprintColonyProject)
	{
	}
}
