using System.Collections;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Exploration;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.ManualCoroutines;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public abstract class ExplorationBaseView : ViewBase<ExplorationVM>, IChangePlanetTypeHandler, ISubscriber<PlanetEntity>, ISubscriber, IScanStarSystemObjectHandler, ISubscriber<StarSystemObjectEntity>, IExplorationUIHandler, IColonizationChronicleHandler, IColonizationEventHandler, IColonizationProjectsUIHandler, IPointOfInterestListUIHandler, IDialogStartHandler, IDialogFinishHandler, IFullScreenUIHandler
{
	[Header("General")]
	[SerializeField]
	private ExplorationVisualElementsWrapperBaseView m_ExplorationVisualElementsWrapperBaseView;

	[SerializeField]
	private ExplorationScanResultsWrapperBaseView m_ExplorationScanResultsWrapperBaseView;

	[SerializeField]
	private ExplorationPointOfInterestListWrapperBaseView m_ExplorationPointOfInterestListWrapperBaseView;

	[SerializeField]
	private ExplorationPlanetDollRoomWrapperBaseView m_ExplorationPlanetDollRoomWrapperBaseView;

	[SerializeField]
	private TextMeshProUGUI m_PlanetNameText;

	[SerializeField]
	private TextMeshProUGUI m_TitheGradeText;

	[SerializeField]
	private TextMeshProUGUI m_TitheGradeValueText;

	[SerializeField]
	private FadeAnimator m_TabletAnimator;

	[SerializeField]
	protected ResourceMinersView m_ResourceMinersView;

	[Header("Animation Parameters")]
	[SerializeField]
	private Animation m_ScanAnimation;

	[SerializeField]
	private float m_ScanAnimationDelay = 1f;

	[SerializeField]
	private float m_NotificationInterval = 1f;

	[Header("Other")]
	[SerializeField]
	private DollRoomTargetController m_CharacterController;

	[SerializeField]
	private GameObject m_BackupPlanet;

	[SerializeField]
	private CanvasGroup m_DialogOverlay;

	[Header("Points Navigation")]
	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_PointsNavigationParameters;

	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected IExplorationComponentEntity m_CurrentFocusedEntity;

	protected FloatConsoleNavigationBehaviour m_PointsNavigationBehaviour;

	private float m_ObjectAngle;

	private bool m_IsExploring;

	private List<ExplorationResourceBaseView> m_PlanetResourcesList = new List<ExplorationResourceBaseView>();

	private List<ColonyTraitBaseView> m_ColonyTraitsList = new List<ColonyTraitBaseView>();

	private PlanetDollRoom PlanetRoom => UIDollRooms.Instance?.PlanetDollRoom;

	public void Initialize()
	{
		m_TabletAnimator.gameObject.SetActive(value: false);
		InitializeImpl();
	}

	protected virtual void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		m_ExplorationVisualElementsWrapperBaseView.Bind(base.ViewModel.ExplorationVisualElementsWrapperVM);
		m_ExplorationScanResultsWrapperBaseView.Bind(base.ViewModel.ExplorationScanResultsWrapperVM);
		m_ExplorationPointOfInterestListWrapperBaseView.Bind(base.ViewModel.ExplorationPointOfInterestListWrapperVM);
		m_ExplorationPlanetDollRoomWrapperBaseView.Bind(base.ViewModel.ExplorationPlanetDollRoomWrapperVM);
		m_ResourceMinersView.Bind(base.ViewModel.ResourceMinersVM);
		AddDisposable(base.ViewModel.IsExploring.Subscribe(delegate(bool value)
		{
			ShowExplorationTablet(value);
		}));
		AddDisposable(base.ViewModel.IsExplored.Subscribe(delegate
		{
			BuildNavigation();
		}));
		AddDisposable(base.ViewModel.IsLockUIForDialog.Subscribe(SetLockUIForDialog));
		AddDisposable(EventBus.Subscribe(this));
		m_CharacterController.Target = m_CharacterController.transform;
		CreateNavigation();
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
		AddDisposable(m_PointsNavigationBehaviour = new FloatConsoleNavigationBehaviour(m_PointsNavigationParameters));
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "ExplorationBaseViewInput"
		});
		CreateInputImpl(m_InputLayer);
	}

	private void BuildNavigation()
	{
		if (m_NavigationBehaviour != null)
		{
			BuildNavigationImpl();
		}
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

	protected void OnCloseClick()
	{
		if (UINetUtility.IsControlMainCharacterWithWarning())
		{
			Game.Instance.GameCommandQueue.ColonyProjectsUIClose();
			Game.Instance.GameCommandQueue.CloseExplorationScreen();
		}
	}

	private void ShowWindow()
	{
		SetPlanet();
		UISounds.Instance.Sounds.SpaceColonization.WindowOpenFromMap.Play();
		m_TabletAnimator.AppearAnimation();
		GamePad.Instance.PushLayer(m_InputLayer);
	}

	private void SetPlanet(bool forceUpdate = false)
	{
		string text = ((!(base.ViewModel.StarSystemObjectView != null)) ? null : (forceUpdate ? base.ViewModel.StarSystemObjectView.Data.Name : (base.ViewModel.StarSystemObjectView.Data.IsScanned ? base.ViewModel.StarSystemObjectView.Data.Name : "???")));
		m_PlanetNameText.text = text;
		if (base.ViewModel.IsPlanet)
		{
			m_TitheGradeText.text = UIStrings.Instance.ExplorationTexts.TitheGrade.Text;
			m_TitheGradeValueText.text = (base.ViewModel.PlanetView.Data.IsScanned ? base.ViewModel.PlanetView.Data.Blueprint.TitheGrade.Text : UIStrings.Instance.ExplorationTexts.TitheGradeUndetermined.Text);
		}
		else
		{
			m_TitheGradeText.text = null;
			m_TitheGradeValueText.text = null;
		}
	}

	void IChangePlanetTypeHandler.HandleChangePlanetType()
	{
		if (base.ViewModel.IsExploring.Value && ((base.ViewModel.StarSystemObjectView != null) ? base.ViewModel.StarSystemObjectView.Data : null) != null)
		{
			SetPlanet();
			HidePlanet();
			ShowPlanet();
		}
	}

	private void HideWindow()
	{
		EscHotkeyManager.Instance.Unsubscribe(OnCloseClick);
		UISounds.Instance.Sounds.SpaceColonization.ColonizationWindowClose.Play();
		m_TabletAnimator.DisappearAnimation(delegate
		{
			m_TabletAnimator.gameObject.SetActive(value: false);
		});
		HideExplorationTablet();
		m_NavigationBehaviour.Clear();
		m_PointsNavigationBehaviour.Clear();
		GamePad.Instance.PopLayer(m_InputLayer);
		m_IsExploring = false;
		HideWindowImpl();
	}

	protected virtual void HideWindowImpl()
	{
	}

	public void ScanPlanet()
	{
		EscHotkeyManager.Instance.Unsubscribe(OnCloseClick);
		EscHotkeyManager.Instance.Subscribe(CancelScan);
		ScanPlanetImpl();
		Game.Instance.CoroutinesController.Start(ScanAnimation(), this);
		Game.Instance.CoroutinesController.Start(ShowNotifications(), this);
	}

	protected virtual void ScanPlanetImpl()
	{
	}

	private IEnumerator ShowNotifications()
	{
		yield return YieldInstructions.WaitForSecondsGameTime(m_ScanAnimationDelay);
		if (base.ViewModel.IsPlanet)
		{
			int xp = base.ViewModel.PlanetView.Data.ExperienceForScan();
			EventBus.RaiseEvent(delegate(IExperienceNotificationUIHandler h)
			{
				h.HandleExperienceNotification(xp);
			});
		}
		yield return YieldInstructions.WaitForSecondsGameTime(m_NotificationInterval);
		string encyclopediaName = base.ViewModel.StarSystemObjectView.Data?.Blueprint?.Name;
		string link = (base.ViewModel.PlanetView?.Data.Blueprint.Type).ToString() + "PlanetType";
		EventBus.RaiseEvent(delegate(IEncyclopediaNotificationUIHandler h)
		{
			h.HandleEncyclopediaNotification(link, encyclopediaName);
		});
	}

	private IEnumerator ScanAnimation()
	{
		m_ScanAnimation.Play("PlanetScan", PlayMode.StopAll);
		yield return YieldInstructions.WaitForSecondsGameTime(m_ScanAnimationDelay);
		m_ScanAnimation.Stop("PlanetScan");
		ScanAnimationImpl();
		EscHotkeyManager.Instance.Unsubscribe(CancelScan);
		SubscribeEscCloseScreen();
		base.ViewModel.ScanObject();
		SetPlanet(forceUpdate: true);
	}

	protected virtual void ScanAnimationImpl()
	{
	}

	protected void CancelScan()
	{
		if (!UINetUtility.IsControlMainCharacterWithWarning())
		{
			return;
		}
		UIUtility.ShowMessageBox(UIStrings.Instance.ExplorationTexts.ExploCancelScan, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			if (button == DialogMessageBoxBase.BoxButton.Yes)
			{
				ClearScanProgress();
			}
		});
	}

	private void ClearScanProgress()
	{
		m_ScanAnimation.Play("PlanetScanReset", PlayMode.StopAll);
		EscHotkeyManager.Instance.Unsubscribe(CancelScan);
		Game.Instance.CoroutinesController.StopAll();
		ClearScanProgressImpl();
		EventBus.RaiseEvent(delegate(IExplorationScanUIHandler h)
		{
			h.HandleScanCancelled();
		});
	}

	protected virtual void ClearScanProgressImpl()
	{
	}

	protected override void DestroyViewImplementation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_PointsNavigationBehaviour.Clear();
		m_PointsNavigationBehaviour = null;
		if (GamePad.Instance.CurrentInputLayer == m_InputLayer)
		{
			GamePad.Instance.PopLayer(m_InputLayer);
		}
		m_InputLayer = null;
		ClearPlanetResources();
		ClearColonyTraits();
		base.gameObject.SetActive(value: false);
		EscHotkeyManager.Instance.Unsubscribe(OnCloseClick);
	}

	private void ClearPlanetResources()
	{
		m_PlanetResourcesList.ForEach(WidgetFactory.DisposeWidget);
		m_PlanetResourcesList.Clear();
	}

	private void ClearColonyTraits()
	{
		m_ColonyTraitsList.ForEach(WidgetFactory.DisposeWidget);
		m_ColonyTraitsList.Clear();
	}

	public void SetLockUIForDialog(bool value)
	{
		if (value)
		{
			EscHotkeyManager.Instance.Unsubscribe(OnCloseClick);
		}
		else
		{
			SubscribeEscCloseScreen();
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

	public void ShowExplorationTablet(bool isExploring)
	{
		m_IsExploring = isExploring;
		if (isExploring)
		{
			ShowWindow();
			SubscribeEscCloseScreen();
			ShowPlanet();
			BuildNavigation();
		}
	}

	private void ShowPlanet()
	{
		StarSystemObjectView starSystemObjectView = base.ViewModel.StarSystemObjectView;
		if (starSystemObjectView == null)
		{
			return;
		}
		GameObject visualRoot = starSystemObjectView.VisualRoot;
		m_BackupPlanet.SetActive(!visualRoot);
		if ((bool)PlanetRoom && (bool)visualRoot)
		{
			if (PlanetRoom != null)
			{
				PlanetRoom.gameObject.SetActive(value: true);
				PlanetRoom.SetupPlanet(visualRoot);
				PlanetRoom.Initialize(m_CharacterController);
				PlanetRoom.Show();
			}
			m_CharacterController.gameObject.SetActive(visualRoot);
		}
	}

	public void HideExplorationTablet()
	{
		EscHotkeyManager.Instance.Unsubscribe(OnCloseClick);
		HidePlanet();
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

	private void SubscribeEscCloseScreen()
	{
		if (m_IsExploring && !EscHotkeyManager.Instance.HasCallback(OnCloseClick))
		{
			EscHotkeyManager.Instance.Subscribe(OnCloseClick);
		}
	}

	public void HandleStartScanningStarSystemObject()
	{
		ScanPlanet();
	}

	void IScanStarSystemObjectHandler.HandleScanStarSystemObject()
	{
		StarSystemObjectEntity starSystemObjectEntity = EventInvokerExtensions.Entity as StarSystemObjectEntity;
		if (base.ViewModel.StarSystemObjectView.Data == starSystemObjectEntity)
		{
			UISounds.Instance.Sounds.SpaceExploration.PlanetScanPointsAppear.Play();
			HandleScanStarSystemObjectImpl();
			SetPlanet(forceUpdate: true);
		}
	}

	protected virtual void HandleScanStarSystemObjectImpl()
	{
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
	}

	public void CloseExplorationScreen()
	{
		HideWindow();
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

	public void HandlePointOfInterestUpdated()
	{
		BuildNavigation();
	}

	public void HandleDialogStarted(BlueprintDialog dialog)
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleDialogFinished(BlueprintDialog dialog, bool success)
	{
		BuildNavigation();
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (state && m_IsExploring)
		{
			OnCloseClick();
		}
	}
}
