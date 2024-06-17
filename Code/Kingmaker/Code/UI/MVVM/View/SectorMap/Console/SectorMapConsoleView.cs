using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.NavigatorResource.Console;
using Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap.Console;
using Kingmaker.Code.UI.MVVM.View.SectorMap.AllSystemsInformationWindow.Console;
using Kingmaker.Code.UI.MVVM.View.SectorMap.Base;
using Kingmaker.Code.UI.MVVM.View.SectorMap.SpaceSystemInformationWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Space.InputLayers;
using Kingmaker.Code.UI.MVVM.View.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.NavigatorResource;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap.Console;

public class SectorMapConsoleView : SectorMapBaseView, IGlobalMapFocusOnTargetObjectConsoleHandler, ISubscriber, ICloseLoadingScreenHandler
{
	[Header("BottomHud")]
	[SerializeField]
	private SectorMapBottomHudConsoleView m_SectorMapBottomHudConsoleView;

	[Header("InformationWindow")]
	[SerializeField]
	private SpaceSystemInformationWindowConsoleView m_SpaceSystemInformationWindowConsoleView;

	[Header("AllSystemsWindow")]
	[SerializeField]
	private AllSystemsInformationWindowConsoleView m_AllSystemsInformationWindowConsoleView;

	[Header("Hints")]
	[SerializeField]
	private FadeAnimator m_AdditionalHintsContainer;

	private IDisposable m_DisappearTask;

	[SerializeField]
	private float m_HintsDisappearTime = 4f;

	[SerializeField]
	private ConsoleHint m_HorizontalDPadInformationWindowsHint;

	[SerializeField]
	private ConsoleHint m_VerticalDPadInformationWindowsHint;

	[SerializeField]
	private ConsoleHint m_CloseWindowInformationWindowsHint;

	[SerializeField]
	private ConsoleHint m_ShowTooltipBigInfoWindowInformationWindowsHint;

	[SerializeField]
	private ConsoleHint m_SwitchCursorHint;

	[SerializeField]
	private ConsoleHint m_SystemInfoHint;

	[SerializeField]
	private ConsoleHint m_ResourcesHint;

	[SerializeField]
	private ConsoleHint m_CloseResourcesHint;

	[SerializeField]
	private ConsoleHint m_EscMenuHint;

	private InputLayer m_ResourcesInputLayer;

	private GridConsoleNavigationBehaviour m_ResourcesNavigationBehavior;

	private readonly BoolReactiveProperty m_ResourcesMode = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_InformationWindowsMode = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_InformationWindowsCloseButtonActive = new BoolReactiveProperty();

	private FloatConsoleNavigationBehaviour m_NavigationBehaviour;

	[Header("Console")]
	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_Parameters;

	[SerializeField]
	private SectorMapOvertipsConsoleView m_SectorMapOvertipsConsoleView;

	private IEnumerable<OvertipSystemConsoleView> NavigationEntities => m_NavigationBehaviour.Entities.Cast<OvertipSystemConsoleView>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		EventBus.Subscribe(this);
		base.ViewModel.SectorMapArtView.NeedShowSectorMapMouseLines = false;
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.ShowInformationWindows, delegate
		{
			HandleShowInformationWindow();
		}));
		m_SectorMapBottomHudConsoleView.Bind(base.ViewModel.SectorMapBottomHudVM);
		m_SpaceSystemInformationWindowConsoleView.Bind(base.ViewModel.SpaceSystemInformationWindowVM);
		m_AllSystemsInformationWindowConsoleView.Bind(base.ViewModel.AllSystemsInformationWindowVM);
		AddDisposable(m_InformationWindowsMode.CombineLatest(m_SpaceSystemInformationWindowConsoleView.InspectMode, (bool informationWindowsMode, bool inspectMode) => new { informationWindowsMode, inspectMode }).Subscribe(state =>
		{
			m_InformationWindowsCloseButtonActive.Value = state.informationWindowsMode && !state.inspectMode;
		}));
		ShowAdditionalHints();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_InformationWindowsMode.Value = false;
		EventBus.Unsubscribe(this);
	}

	public void AddBaseInput(InputLayer baseInputLayer)
	{
	}

	public void AddGlobalMapInput(InputLayer inputLayer)
	{
		AddDisposable(m_NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_Parameters));
		m_NavigationBehaviour.GetInputLayer(inputLayer);
		AddDisposable(m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity));
		m_SectorMapOvertipsConsoleView.SystemViewCollection.ForEach(OnAddOvertip);
		AddDisposable(m_SectorMapOvertipsConsoleView.SystemViewCollection.ObserveAdd().Subscribe(delegate(CollectionAddEvent<OvertipSystemConsoleView> value)
		{
			OnAddOvertip(value.Value);
		}));
		AddDisposable(m_SectorMapOvertipsConsoleView.SystemViewCollection.ObserveReset().Subscribe(OnClearOvertips));
		m_SectorMapBottomHudConsoleView.CreateInputImpl(inputLayer, m_InformationWindowsMode, m_SpaceSystemInformationWindowConsoleView.InspectMode, delegate
		{
			FocusOnSystem(state: true, onCurrentSystem: true);
		});
		AddDisposable(m_ResourcesHint.Bind(inputLayer.AddButton(delegate
		{
			ShowResources();
		}, 11, InputActionEventType.ButtonJustReleased)));
		m_ResourcesHint.SetLabel(UIStrings.Instance.GlobalMap.ShowResources);
		AddDisposable(m_SystemInfoHint.Bind(inputLayer.AddButton(delegate
		{
			HandleShowInformationWindow();
		}, 19, m_SpaceSystemInformationWindowConsoleView.InspectMode.Not().ToReactiveProperty(), InputActionEventType.ButtonJustReleased)));
		m_SystemInfoHint.SetLabel(UIStrings.Instance.GlobalMap.SystemInfo);
		AddDisposable(m_SwitchCursorHint.Bind(inputLayer.AddButton(delegate
		{
			SwitchCursor(inputLayer);
		}, 18, InputActionEventType.ButtonJustReleased)));
		m_SwitchCursorHint.SetLabel(UIStrings.Instance.HUDTexts.Pointer);
		AddDisposable(m_EscMenuHint.Bind(inputLayer.AddButton(delegate
		{
		}, 16, InputActionEventType.ButtonJustReleased)));
		m_EscMenuHint.SetLabel(UIStrings.Instance.MainMenu.Settings);
		AddResourceInput();
		AddInformationWindowsInput(inputLayer);
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		if (!m_NavigationBehaviour.Entities.Any())
		{
			return;
		}
		if (m_InformationWindowsMode.Value)
		{
			EventBus.RaiseEvent(delegate(IGlobalMapInformationWindowsConsoleHandler h)
			{
				h.HandleChangeCurrentSystemInfoConsole(GetFocusSectorMapObject());
			});
		}
		OvertipSystemConsoleView overtipSystemConsoleView = entity as OvertipSystemConsoleView;
		if (overtipSystemConsoleView == null)
		{
			return;
		}
		SectorMapBottomHudVM instance = SectorMapBottomHudVM.Instance;
		BlueprintWarpRoutesSettings warpRoutesSettings = BlueprintWarhammerRoot.Instance.WarpRoutesSettings;
		if (!overtipSystemConsoleView.IsCurrentSystem())
		{
			SectorMapPassageEntity passage = overtipSystemConsoleView.GetPassage();
			if ((passage == null || passage.CurrentDifficulty != 0) && (overtipSystemConsoleView.GetPassage() != null || instance.CurrentValue.Value >= warpRoutesSettings.CreateNewPassageCost) && (overtipSystemConsoleView.GetPassage() == null || instance.CurrentValue.Value >= warpRoutesSettings.LowerPassageDifficultyCost))
			{
				goto IL_00ce;
			}
		}
		EventBus.RaiseEvent(delegate(IGlobalMapWillChangeNavigatorResourceEffectHandler h)
		{
			h.HandleWillChangeNavigatorResourceEffect(state: false, 0);
		});
		goto IL_00ce;
		IL_00ce:
		Vector3 position = overtipSystemConsoleView.GetSectorMapObject().Position;
		base.ViewModel.SectorMapArtView.SetCustomPointerLinesToPos(position);
		if (!UIUtilityGetRect.CheckObjectInRect(position, 35f, 35f))
		{
			CameraRig.Instance.ScrollTo(position);
		}
	}

	private void OnAddOvertip(OvertipSystemConsoleView overtip)
	{
		m_NavigationBehaviour.AddEntity(overtip);
		OvertipSystemConsoleView overtipSystemConsoleView = overtip.Or(null);
		if ((object)overtipSystemConsoleView != null)
		{
			SectorMapObjectEntity sectorMapObject = overtipSystemConsoleView.GetSectorMapObject();
			if (sectorMapObject != null)
			{
				sectorMapObject.View.Or(null)?.InstanceSystemPlanetDecalConsoleFocus();
			}
		}
		if (m_NavigationBehaviour.Focus.Value == null && overtip.IsCurrentSystem())
		{
			m_NavigationBehaviour.FocusOnEntityManual(overtip);
			base.ViewModel.SectorMapArtView.SetCustomPointerLinesToPos(overtip.GetSectorMapObject().Position);
		}
	}

	private void OnClearOvertips()
	{
		if (NavigationEntities.Any())
		{
			NavigationEntities.ForEach(delegate(OvertipSystemConsoleView e)
			{
				OvertipSystemConsoleView overtipSystemConsoleView = e.Or(null);
				if ((object)overtipSystemConsoleView != null)
				{
					SectorMapObjectEntity sectorMapObject = overtipSystemConsoleView.GetSectorMapObject();
					if (sectorMapObject != null)
					{
						sectorMapObject.View.Or(null)?.DestroySystemPlanetDecalConsoleFocus();
					}
				}
			});
		}
		m_NavigationBehaviour.Clear();
	}

	private void AddResourceInput()
	{
		AddDisposable(m_ResourcesNavigationBehavior = new GridConsoleNavigationBehaviour());
		m_ResourcesInputLayer = m_ResourcesNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "SpaceResources"
		});
		AddDisposable(m_CloseResourcesHint.Bind(m_ResourcesInputLayer.AddButton(delegate
		{
			CloseResources();
		}, 9, m_ResourcesMode)));
		m_CloseResourcesHint.SetLabel(UIStrings.Instance.GlobalMap.CloseResources);
		AddDisposable(m_ResourcesInputLayer.AddButton(delegate
		{
			CloseResources();
		}, 11, m_ResourcesMode, InputActionEventType.ButtonJustReleased));
	}

	private void AddInformationWindowsInput(InputLayer inputLayer)
	{
		AddDisposable(m_HorizontalDPadInformationWindowsHint.BindCustomAction(21, inputLayer, m_SpaceSystemInformationWindowConsoleView.InspectMode));
		m_HorizontalDPadInformationWindowsHint.SetLabel(UIStrings.Instance.GlobalMap.ChangeWindow);
		AddDisposable(m_VerticalDPadInformationWindowsHint.BindCustomAction(22, inputLayer, m_SpaceSystemInformationWindowConsoleView.InspectMode));
		m_VerticalDPadInformationWindowsHint.SetLabel(UIStrings.Instance.SettingsUI.Navigation);
		AddDisposable(m_ShowTooltipBigInfoWindowInformationWindowsHint.Bind(inputLayer.AddButton(delegate
		{
			m_SpaceSystemInformationWindowConsoleView.ShowTooltipInfo();
		}, 8, m_SpaceSystemInformationWindowConsoleView.InspectMode.And(m_SpaceSystemInformationWindowConsoleView.IsFocusedEntityWithTooltip).ToReactiveProperty())));
		m_ShowTooltipBigInfoWindowInformationWindowsHint.SetLabel(UIStrings.Instance.CommonTexts.Expand);
		AddDisposable(inputLayer.AddButton(delegate
		{
			HandleBackInformationWindow();
		}, 4, m_SpaceSystemInformationWindowConsoleView.InspectMode, InputActionEventType.ButtonJustReleased));
		AddDisposable(inputLayer.AddButton(delegate
		{
			m_AllSystemsInformationWindowConsoleView.SelectSystem();
		}, 5, m_SpaceSystemInformationWindowConsoleView.InspectMode.And(m_AllSystemsInformationWindowConsoleView.AllSystemsMode).ToReactiveProperty(), InputActionEventType.ButtonJustReleased));
		AddDisposable(m_CloseWindowInformationWindowsHint.Bind(inputLayer.AddButton(delegate
		{
			HandleBackInformationWindow();
		}, 9, m_SpaceSystemInformationWindowConsoleView.InspectMode)));
		m_CloseWindowInformationWindowsHint.SetLabel(UIStrings.Instance.CommonTexts.Back);
		m_SpaceSystemInformationWindowConsoleView.AddSystemInformationInput(inputLayer);
		m_AllSystemsInformationWindowConsoleView.AddSystemInformationInput(inputLayer, m_NavigationBehaviour);
	}

	private void HandleShowInformationWindow()
	{
		if (!m_SpaceSystemInformationWindowConsoleView.InspectMode.Value)
		{
			if (m_InformationWindowsMode.Value)
			{
				HandleHideInformationWindow();
				return;
			}
			ShowAdditionalHints();
			base.ViewModel.ShowVisitDialogBox(GetFocusSectorMapObject());
			m_InformationWindowsMode.Value = true;
		}
	}

	private void HandleBackInformationWindow()
	{
		if (m_SpaceSystemInformationWindowConsoleView.InspectMode.Value)
		{
			if (m_SpaceSystemInformationWindowConsoleView.IsOnFront())
			{
				HandleChangeInformationWindows();
				return;
			}
			m_AllSystemsInformationWindowConsoleView.HideAllSystemsMode();
			m_SpaceSystemInformationWindowConsoleView.HideInspectMode();
			FocusOnSystem(state: true, onCurrentSystem: false);
		}
	}

	private void HandleHideInformationWindow()
	{
		if (m_SpaceSystemInformationWindowConsoleView.InspectMode.Value)
		{
			if (!m_SpaceSystemInformationWindowConsoleView.IsOnFront())
			{
				HandleChangeInformationWindows();
			}
			m_SpaceSystemInformationWindowConsoleView.HideInspectMode();
		}
		base.ViewModel.HideVisitDialogBox();
		m_InformationWindowsMode.Value = false;
		TooltipHelper.HideTooltip();
	}

	private void HandleChangeInformationWindows()
	{
		base.ViewModel.ChangeVisitDialogBox();
	}

	private void ShowResources()
	{
		if (!(Game.Instance.CurrentMode != GameModeType.GlobalMap))
		{
			m_ResourcesMode.Value = true;
			SetResourcesNavigation();
			GamePad.Instance.PushLayer(m_ResourcesInputLayer);
			m_ResourcesNavigationBehavior.FocusOnFirstValidEntity();
		}
	}

	private void CloseResources()
	{
		if (!(Game.Instance.CurrentMode != GameModeType.GlobalMap))
		{
			m_ResourcesNavigationBehavior.UnFocusCurrentEntity();
			m_ResourcesMode.Value = false;
			TooltipHelper.HideTooltip();
			GamePad.Instance.PopLayer(m_ResourcesInputLayer);
		}
	}

	private void SetResourcesNavigation()
	{
		m_ResourcesNavigationBehavior.Clear();
		WidgetListMVVM widgetListResources = SystemMapSpaceResourcesPCView.Instance.WidgetListResources;
		SystemMapSpaceProfitFactorView systemMapSpaceProfitFactorViewPrefab = SystemMapSpaceResourcesPCView.Instance.SystemMapSpaceProfitFactorViewPrefab;
		List<IConsoleNavigationEntity> list = (from block in widgetListResources.Entries.OfType<SystemMapSpaceResourceView>()
			where block.gameObject.activeInHierarchy
			select block).Cast<IConsoleNavigationEntity>().ToList();
		if (systemMapSpaceProfitFactorViewPrefab != null && systemMapSpaceProfitFactorViewPrefab.gameObject.activeInHierarchy)
		{
			list.Add(systemMapSpaceProfitFactorViewPrefab);
		}
		if (Enumerable.Any(list))
		{
			m_ResourcesNavigationBehavior.SetEntitiesHorizontal(list);
		}
		if (m_ResourcesMode.Value)
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void SwitchCursor(InputLayer inputLayer)
	{
		SpaceMainInputLayer spaceMainInputLayer = (SpaceMainInputLayer)inputLayer;
		spaceMainInputLayer.StopMovement();
		spaceMainInputLayer.SwitchCursorEnabled();
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleHintRequest(null, shouldShow: false);
		});
		if (!m_NavigationBehaviour.Entities.Any())
		{
			return;
		}
		bool cursorEnabled = spaceMainInputLayer.CursorEnabled;
		foreach (IConsoleEntity entity in m_NavigationBehaviour.Entities)
		{
			OvertipSystemConsoleView overtipSystemConsoleView = entity as OvertipSystemConsoleView;
			if (overtipSystemConsoleView != null)
			{
				overtipSystemConsoleView.IsNavigationValid.Value = !cursorEnabled;
			}
		}
		if (!cursorEnabled)
		{
			m_NavigationBehaviour.Entities.ForEach(delegate(IConsoleEntity e)
			{
				OvertipSystemConsoleView overtipSystemConsoleView2 = e as OvertipSystemConsoleView;
				if (overtipSystemConsoleView2 != null)
				{
					overtipSystemConsoleView2.SetFocus(value: false);
				}
			});
		}
		base.ViewModel.SectorMapArtView.NeedShowSectorMapMouseLines = cursorEnabled;
		if (!cursorEnabled)
		{
			base.ViewModel.SectorMapArtView.SetCustomPointerLinesToPos(GetFocusSectorMapObject().Position);
		}
		if (cursorEnabled)
		{
			m_NavigationBehaviour.UnFocusCurrentEntity();
		}
		else
		{
			m_NavigationBehaviour.FocusOnCurrentEntity();
		}
	}

	private void FocusOnSystem(bool state, bool onCurrentSystem)
	{
		if (!state)
		{
			return;
		}
		IConsoleEntity consoleEntity = null;
		if (!onCurrentSystem)
		{
			consoleEntity = NavigationEntities.FirstOrDefault((OvertipSystemConsoleView e) => e.IsFocused);
		}
		if (consoleEntity == null)
		{
			consoleEntity = NavigationEntities?.FirstOrDefault((OvertipSystemConsoleView e) => e.IsCurrentSystem());
		}
		if (consoleEntity != null)
		{
			m_NavigationBehaviour.FocusOnEntityManual(consoleEntity);
		}
		else
		{
			m_NavigationBehaviour.FocusOnCurrentEntity();
		}
	}

	private void FocusOnTargetSystem(SectorMapObjectEntity sectorMapObject)
	{
		if (sectorMapObject != null)
		{
			OvertipSystemConsoleView overtipSystemConsoleView = NavigationEntities.FirstOrDefault((OvertipSystemConsoleView e) => e.GetSectorMapObject() == sectorMapObject);
			if (overtipSystemConsoleView != null)
			{
				m_NavigationBehaviour.FocusOnEntityManual(overtipSystemConsoleView);
			}
		}
	}

	private SectorMapObjectEntity GetFocusSectorMapObject()
	{
		OvertipSystemConsoleView overtipSystemConsoleView = m_NavigationBehaviour.CurrentEntity as OvertipSystemConsoleView;
		if (overtipSystemConsoleView == null)
		{
			overtipSystemConsoleView = m_NavigationBehaviour.Entities.FirstOrDefault((IConsoleEntity e) => e is OvertipSystemConsoleView overtipSystemConsoleView2 && overtipSystemConsoleView2.IsFocused) as OvertipSystemConsoleView;
		}
		return overtipSystemConsoleView.Or(null)?.GetSectorMapObject();
	}

	public void HandleChangeFocusOnTargetObjectConsole(SectorMapObjectEntity sectorMapObjectEntity)
	{
		FocusOnTargetSystem(sectorMapObjectEntity);
	}

	private void ShowAdditionalHints()
	{
		if (!(m_AdditionalHintsContainer == null))
		{
			if (m_DisappearTask != null)
			{
				m_DisappearTask.Dispose();
				m_DisappearTask = null;
			}
			m_AdditionalHintsContainer.AppearAnimation();
			m_DisappearTask = DelayedInvoker.InvokeInTime(delegate
			{
				m_AdditionalHintsContainer.DisappearAnimation();
			}, m_HintsDisappearTime);
		}
	}

	public void HandleCloseLoadingScreen()
	{
		FocusOnSystem(state: true, onCurrentSystem: true);
	}
}
