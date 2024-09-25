using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.BugReport;
using Kingmaker.Code.UI.MVVM.VM.IngameMenu;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.Tutorial.Console;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.IngameMenu.Console;

public class IngameMenuConsoleView : ViewBase<IngameMenuVM>
{
	[Header("Items")]
	[SerializeField]
	private IngameMenuItemConsoleView m_Inventory;

	[SerializeField]
	private IngameMenuItemConsoleView m_Character;

	[SerializeField]
	private IngameMenuItemConsoleView m_Journal;

	[SerializeField]
	private IngameMenuItemConsoleView m_Map;

	[SerializeField]
	private IngameMenuItemConsoleView m_Encyclopedia;

	[SerializeField]
	private IngameMenuItemConsoleView m_ShipCustomization;

	[SerializeField]
	private IngameMenuItemConsoleView m_ColonyManagement;

	[SerializeField]
	private IngameMenuItemConsoleView m_LevelUp;

	[SerializeField]
	private IngameMenuItemConsoleView m_VoidshipLevelUp;

	[SerializeField]
	private IngameMenuItemConsoleView m_CargoManagement;

	[Space]
	[SerializeField]
	private RectTransform m_Content;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[Header("Console")]
	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_Parameters;

	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private OwlcatMultiButton m_FirstSelectionButton;

	private FloatConsoleNavigationBehaviour m_NewNavigationBehaviour;

	private SimpleConsoleNavigationEntity m_FirstSelection;

	private readonly BoolReactiveProperty m_CanCancel = new BoolReactiveProperty();

	private InputLayer m_InputLayer;

	public void Initialize()
	{
		InitializeItems();
		m_FadeAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		BindItems();
		m_FirstSelectionButton.gameObject.SetActive(value: true);
		CreateNavigation();
		AddDisposable(GamePad.Instance.PushLayer(GetInputLayer()));
		m_FadeAnimator.AppearAnimation();
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state: true, ModalWindowUIType.InGameMenu);
		});
		AddDisposable(GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged));
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator.DisappearAnimation();
		UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
		IngameMenuItemConsoleView ingameMenuItemConsoleView = m_NewNavigationBehaviour.CurrentEntity as IngameMenuItemConsoleView;
		if (ingameMenuItemConsoleView != null)
		{
			ingameMenuItemConsoleView.OnConfirmClick();
		}
		m_NewNavigationBehaviour.Clear();
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state: false, ModalWindowUIType.InGameMenu);
		});
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = m_NewNavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "IngameMenuConsoleView"
		}, null, leftStick: true, rightStick: true);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			m_NewNavigationBehaviour.ResetCurrentEntity();
			base.ViewModel.DestroyView();
		}, 9, m_CanCancel), UIStrings.Instance.CommonTexts.Cancel));
		AddDisposable(inputLayer.AddButton(delegate
		{
			base.ViewModel.DestroyView();
		}, 8, m_CanCancel));
		m_InputLayer = inputLayer;
		return inputLayer;
	}

	private void InitializeItems()
	{
		UIMeinMenuTexts mainMenu = UIStrings.Instance.MainMenu;
		m_Inventory.Initialize(mainMenu.Inventory);
		m_Character.Initialize(mainMenu.CharacterInfo);
		m_Journal.Initialize(mainMenu.Journal);
		m_Map.Initialize(mainMenu.LocalMap);
		m_Encyclopedia.Initialize(mainMenu.Encyclopedia);
		m_ShipCustomization.Initialize(mainMenu.ShipCustomization);
		m_ColonyManagement.Initialize(mainMenu.ColonyManagement);
		m_LevelUp.Initialize(mainMenu.LevelUp);
		m_VoidshipLevelUp.Initialize(mainMenu.VoidshipLevelUp);
		m_CargoManagement.Initialize(mainMenu.CargoManagement);
	}

	private void BindItems()
	{
		m_Inventory.Bind(base.ViewModel.OpenInventory);
		m_Character.Bind(base.ViewModel.OpenCharScreen);
		m_Journal.Bind(base.ViewModel.OpenJournal);
		if (!base.ViewModel.IsInSpace())
		{
			m_Map.Bind(base.ViewModel.OpenMap);
		}
		m_Map.gameObject.SetActive(!base.ViewModel.IsInSpace());
		m_Encyclopedia.Bind(base.ViewModel.OpenEncyclopedia);
		m_CargoManagement.Bind(base.ViewModel.OpenCargoManagement);
		bool canAccessStarshipInventory = Game.Instance.Player.CanAccessStarshipInventory;
		bool flag = Game.Instance.Player.ColoniesState.ForbidColonization;
		if (canAccessStarshipInventory)
		{
			m_ShipCustomization.Bind(base.ViewModel.OpenShipCustomization);
			if (!flag)
			{
				m_ColonyManagement.Bind(base.ViewModel.OpenColonyManagement);
			}
			if (base.ViewModel.HasShipLvlUp())
			{
				m_VoidshipLevelUp.Bind(base.ViewModel.OpenShipLevelUp);
			}
		}
		if (base.ViewModel.HasLevelUp())
		{
			m_LevelUp.Bind(base.ViewModel.OpenLevelUpOnFirstDecentUnit);
		}
		m_LevelUp.gameObject.SetActive(base.ViewModel.HasLevelUp());
		m_ShipCustomization.gameObject.SetActive(canAccessStarshipInventory);
		m_ColonyManagement.gameObject.SetActive(canAccessStarshipInventory && !flag);
		m_VoidshipLevelUp.gameObject.SetActive(canAccessStarshipInventory && base.ViewModel.HasShipLvlUp());
	}

	private void CreateNavigation()
	{
		if (m_NewNavigationBehaviour == null)
		{
			AddDisposable(m_NewNavigationBehaviour = new FloatConsoleNavigationBehaviour(m_Parameters));
		}
		else
		{
			m_NewNavigationBehaviour.Clear();
		}
		List<IngameMenuItemConsoleView> entities = m_Content.GetComponentsInChildren<IngameMenuItemConsoleView>().ToList();
		m_FirstSelection = new SimpleConsoleNavigationEntity(m_FirstSelectionButton);
		m_NewNavigationBehaviour.AddEntities(entities);
		m_NewNavigationBehaviour.AddEntity(m_FirstSelection);
		m_NewNavigationBehaviour.DeepestFocusAsObservable.Skip(1).Subscribe(OnFocusEntity);
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_NewNavigationBehaviour.FocusOnEntityManual(m_FirstSelection);
		}, 1);
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_CanCancel.Value = entity != m_FirstSelection && entity != null;
		if (entity != m_FirstSelection && m_NewNavigationBehaviour.Entities.Contains(m_FirstSelection))
		{
			m_NewNavigationBehaviour.RemoveEntity(m_FirstSelection);
			m_FirstSelectionButton.gameObject.SetActive(value: false);
		}
	}

	private void OnCurrentInputLayerChanged()
	{
		InputLayer currentInputLayer = GamePad.Instance.CurrentInputLayer;
		if (currentInputLayer != m_InputLayer && !(currentInputLayer.ContextName == BugReportBaseView.InputLayerContextName) && !(currentInputLayer.ContextName == BugReportDrawingView.InputLayerContextName) && !RootUIContext.Instance.IsBugReportShow)
		{
			if (currentInputLayer.ContextName == TutorialBigWindowConsoleView.InputLayerContextName || currentInputLayer.ContextName == TutorialBigWindowConsoleView.GlossaryContextName)
			{
				base.ViewModel.DestroyView();
				return;
			}
			GamePad.Instance.PopLayer(m_InputLayer);
			GamePad.Instance.PushLayer(m_InputLayer);
		}
	}
}
