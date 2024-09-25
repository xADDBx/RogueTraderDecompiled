using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap.Console;
using Kingmaker.Code.UI.MVVM.View.SectorMap.AllSystemsInformationWindow.Base;
using Kingmaker.Code.UI.MVVM.VM.SectorMap.AllSystemsInformationWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap.AllSystemsInformationWindow.Console;

public class AllSystemsInformationWindowConsoleView : AllSystemsInformationWindowBaseView
{
	[SerializeField]
	private SystemInfoAllSystemsInformationWindowConsoleView m_SystemInfoAllSystemsInformationWindowConsoleViewPrefab;

	[Header("Show Hide Inspect Move")]
	[SerializeField]
	private float m_ShowPosY = 200f;

	[SerializeField]
	private float m_MoveFrontX = 50f;

	[SerializeField]
	private float m_MoveFrontY = 155f;

	private GridConsoleNavigationBehaviour m_AllSystemsInformationWindowBehavior;

	public readonly BoolReactiveProperty AllSystemsMode = new BoolReactiveProperty();

	private ConsoleNavigationBehaviour m_ParentNavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsAllSystemsOnTheFront.Skip(1).Subscribe(ChangeFrontWindowPosition));
	}

	public void AddSystemInformationInput(InputLayer allSystemsInformationInputLayer, ConsoleNavigationBehaviour parentNavigationBehaviour)
	{
		m_ParentNavigationBehaviour = parentNavigationBehaviour;
		AddDisposable(m_AllSystemsInformationWindowBehavior = new GridConsoleNavigationBehaviour());
		m_AllSystemsInformationWindowBehavior.GetInputLayer(allSystemsInformationInputLayer);
		AddDisposable(m_AllSystemsInformationWindowBehavior.Focus.Subscribe(ScrollMenu));
		AddDisposable(allSystemsInformationInputLayer.AddButton(delegate
		{
			SelectSystem();
		}, 8, AllSystemsMode));
	}

	public void ScrollMenu(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRectExtended.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}

	protected override void DrawSystems()
	{
		base.DrawSystems();
		SystemInfoAllSystemsInformationWindowVM[] array = base.ViewModel.Systems.ToArray();
		if (array.Any())
		{
			m_SystemsWidgetList.DrawEntries(array, m_SystemInfoAllSystemsInformationWindowConsoleViewPrefab);
		}
	}

	private void ChangeFrontWindowPosition(bool isFront)
	{
		Vector2 endValue = new Vector2(isFront ? m_MoveFrontX : m_ShowPosX, isFront ? m_MoveFrontY : m_ShowPosY);
		base.RectTransform.DOAnchorPos(endValue, 0.5f);
		if (isFront)
		{
			ShowAllSystemsMode();
		}
		else
		{
			HideAllSystemsMode();
		}
	}

	private void ShowAllSystemsMode()
	{
		AddNavigation();
		AllSystemsMode.Value = true;
	}

	public void HideAllSystemsMode()
	{
		ShowFocusBorder(base.ViewModel.SectorMapObjectEntity.Value);
		base.ViewModel.IsAllSystemsOnTheFront.Value = false;
		TooltipHelper.HideTooltip();
		m_AllSystemsInformationWindowBehavior.UnFocusCurrentEntity();
		m_AllSystemsInformationWindowBehavior.Clear();
		AllSystemsMode.Value = false;
	}

	private void HideFocusBorder()
	{
		if (m_SystemsWidgetList.Entries == null)
		{
			return;
		}
		foreach (IWidgetView item in m_SystemsWidgetList.Entries.Where((IWidgetView e) => e is SystemInfoAllSystemsInformationWindowConsoleView).ToList())
		{
			SystemInfoAllSystemsInformationWindowConsoleView systemInfoAllSystemsInformationWindowConsoleView = item as SystemInfoAllSystemsInformationWindowConsoleView;
			if (systemInfoAllSystemsInformationWindowConsoleView != null)
			{
				systemInfoAllSystemsInformationWindowConsoleView.ShowFocusBorder(state: false);
			}
		}
	}

	private void ShowFocusBorder(SectorMapObjectEntity sectorMapObjectEntity)
	{
		if (m_SystemsWidgetList.Entries == null)
		{
			return;
		}
		SystemInfoAllSystemsInformationWindowConsoleView system = m_SystemsWidgetList.Entries.FirstOrDefault((IWidgetView e) => e is SystemInfoAllSystemsInformationWindowConsoleView systemInfoAllSystemsInformationWindowConsoleView && systemInfoAllSystemsInformationWindowConsoleView.GetCurrentObjectEntity() == sectorMapObjectEntity) as SystemInfoAllSystemsInformationWindowConsoleView;
		if (system == null)
		{
			return;
		}
		system.ShowFocusBorder(state: true);
		DelayedInvoker.InvokeInFrames(delegate
		{
			RectTransform rectTransform = system.transform as RectTransform;
			if (!m_ScrollRectExtended.IsInViewport(rectTransform))
			{
				m_ScrollRectExtended.ScrollToRectCenter(rectTransform, rectTransform);
			}
		}, 3);
	}

	public void SelectSystem()
	{
		SystemInfoAllSystemsInformationWindowConsoleView system = m_SystemsWidgetList.Entries.OfType<SystemInfoAllSystemsInformationWindowConsoleView>().FirstOrDefault((SystemInfoAllSystemsInformationWindowConsoleView s) => s.IsFocused.Value);
		if (!(system == null))
		{
			IConsoleEntity entity = m_ParentNavigationBehaviour.Entities.FirstOrDefault((IConsoleEntity s) => s is SystemInfoAllSystemsInformationWindowConsoleView systemInfoAllSystemsInformationWindowConsoleView && systemInfoAllSystemsInformationWindowConsoleView.GetCurrentObjectEntity() == system.GetCurrentObjectEntity());
			m_ParentNavigationBehaviour.FocusOnEntityManual(entity);
			EventBus.RaiseEvent(delegate(IGlobalMapInformationWindowsConsoleHandler h)
			{
				h.HandleChangeCurrentSystemInfoConsole(system.GetCurrentObjectEntity());
			});
			system.SetCameraOnSystem();
			EventBus.RaiseEvent(delegate(IGlobalMapInformationWindowsConsoleHandler h)
			{
				h.HandleChangeInformationWindowsConsole();
			});
			EventBus.RaiseEvent(delegate(IGlobalMapFocusOnTargetObjectConsoleHandler h)
			{
				h.HandleChangeFocusOnTargetObjectConsole(system.GetCurrentObjectEntity());
			});
		}
	}

	private void AddNavigation()
	{
		List<IConsoleNavigationEntity> list = (from block in m_SystemsWidgetList.Entries.OfType<SystemInfoAllSystemsInformationWindowConsoleView>()
			where block.gameObject.activeInHierarchy
			select block).Cast<IConsoleNavigationEntity>().ToList();
		if (!list.Any())
		{
			return;
		}
		m_AllSystemsInformationWindowBehavior.SetEntitiesVertical(list);
		OvertipSystemConsoleView focusedSystem = m_ParentNavigationBehaviour.Entities.FirstOrDefault((IConsoleEntity e) => e is OvertipSystemConsoleView overtipSystemConsoleView && overtipSystemConsoleView.IsFocused) as OvertipSystemConsoleView;
		IConsoleNavigationEntity consoleNavigationEntity = list.FirstOrDefault((IConsoleNavigationEntity ce) => focusedSystem != null && ce is SystemInfoAllSystemsInformationWindowConsoleView systemInfoAllSystemsInformationWindowConsoleView2 && systemInfoAllSystemsInformationWindowConsoleView2.GetCurrentObjectEntity() == focusedSystem.GetSectorMapObject());
		HideFocusBorder();
		if (consoleNavigationEntity != null)
		{
			m_AllSystemsInformationWindowBehavior.FocusOnEntityManual(consoleNavigationEntity);
		}
		else
		{
			consoleNavigationEntity = list.FirstOrDefault((IConsoleNavigationEntity e) => e is SystemInfoAllSystemsInformationWindowConsoleView systemInfoAllSystemsInformationWindowConsoleView && systemInfoAllSystemsInformationWindowConsoleView.GetCurrentObjectEntity() == Game.Instance.SectorMapController.CurrentStarSystem);
			if (consoleNavigationEntity == null)
			{
				m_AllSystemsInformationWindowBehavior.FocusOnFirstValidEntity();
				return;
			}
			m_AllSystemsInformationWindowBehavior.FocusOnEntityManual(consoleNavigationEntity);
		}
		RectTransform rectTransform = ((consoleNavigationEntity as MonoBehaviour) ?? (consoleNavigationEntity as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
		if (!m_ScrollRectExtended.IsInViewport(rectTransform))
		{
			m_ScrollRectExtended.ScrollToRectCenter(rectTransform, rectTransform);
		}
	}

	public bool IsOnFront()
	{
		return base.ViewModel.IsAllSystemsOnTheFront.Value;
	}

	protected override void ShowHideWindow(bool state)
	{
		base.ShowHideWindow(state);
		if (state)
		{
			ShowFocusBorder(base.ViewModel.SectorMapObjectEntity.Value);
		}
		else
		{
			HideFocusBorder();
		}
	}
}
