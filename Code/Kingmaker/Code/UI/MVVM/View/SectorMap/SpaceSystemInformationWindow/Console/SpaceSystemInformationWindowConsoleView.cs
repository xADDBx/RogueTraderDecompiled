using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap.SpaceSystemInformationWindow.Console;

public class SpaceSystemInformationWindowConsoleView : SpaceSystemInformationWindowBaseView
{
	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_InteractionHint;

	[Header("Show Hide Inspect Move")]
	[SerializeField]
	private float m_ShowPosY = 155f;

	[SerializeField]
	private float m_ShowInspectPosX = 125f;

	[SerializeField]
	private float m_HideInspectPosX = 25f;

	[SerializeField]
	private float m_MoveBehindX = 100f;

	[SerializeField]
	private float m_MoveBehindY = 200f;

	[SerializeField]
	private float m_MoveWindowsSpeed = 0.5f;

	private GridConsoleNavigationBehaviour m_SystemInformationWindowBehavior;

	public readonly BoolReactiveProperty InspectMode = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_ShouldShowInspectModeHint = new BoolReactiveProperty();

	public readonly ReactiveProperty<bool> IsFocusedEntityWithTooltip = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowSystemWindow = new ReactiveProperty<bool>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(InspectMode.Subscribe(delegate(bool value)
		{
			m_ShouldShowInspectModeHint.Value = !value;
			Game.Instance.SectorMapController.IsInformationWindowInspectMode = value;
		}));
		AddDisposable(base.ViewModel.IsInformSystemOnTheFront.Skip(1).Subscribe(ChangeFrontWindowPosition));
		AddDisposable(base.ViewModel.ShowSystemWindow.Subscribe(delegate(bool value)
		{
			m_ShowSystemWindow.Value = value;
		}));
	}

	public void AddSystemInformationInput(InputLayer inputLayer)
	{
		AddDisposable(m_SystemInformationWindowBehavior = new GridConsoleNavigationBehaviour());
		m_SystemInformationWindowBehavior.GetInputLayer(inputLayer);
		AddDisposable(m_SystemInformationWindowBehavior.Focus.Subscribe(delegate(IConsoleEntity entity)
		{
			ScrollMenu(entity);
			if (base.ViewModel != null)
			{
				IsFocusedEntityWithTooltip.Value = base.ViewModel.IsInformSystemOnTheFront.Value && entity is PlanetInfoSpaceSystemInformationWindowView;
			}
		}));
		AddDisposable(m_InteractionHint.Bind(inputLayer.AddButton(delegate
		{
			ShowInspectMode();
		}, 19, m_ShouldShowInspectModeHint.And(m_ShowSystemWindow).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
		m_InteractionHint.SetLabel(UIStrings.Instance.GlobalMap.SystemInfo);
	}

	public void ScrollMenu(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRectExtended.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}

	private void ShowInspectMode()
	{
		GamePad.Instance.BaseLayer?.Unbind();
		InspectMode.Value = true;
		base.RectTransform.DOAnchorPosX(m_ShowInspectPosX, m_MoveWindowsSpeed);
		AddNavigation();
		EventBus.RaiseEvent(delegate(IGlobalMapInformationWindowsConsoleHandler h)
		{
			h.HandleShowAllSystemsInformationWindowConsole(base.ViewModel.SectorMapObjectEntity.Value);
		});
	}

	public void HideInspectMode()
	{
		TooltipHelper.HideTooltip();
		base.ViewModel.IsInformSystemOnTheFront.Value = true;
		InspectMode.Value = false;
		base.RectTransform.DOAnchorPosX(m_HideInspectPosX, m_MoveWindowsSpeed);
		ClearNavigation();
		GamePad.Instance.BaseLayer?.Bind();
		EventBus.RaiseEvent(delegate(IGlobalMapInformationWindowsConsoleHandler h)
		{
			h.HandleHideAllSystemsInformationWindowConsole();
		});
	}

	private void AddNavigation()
	{
		List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
		if (m_WidgetList.Entries != null && m_WidgetList.Entries.OfType<PlanetInfoSpaceSystemInformationWindowView>().Any())
		{
			list.AddRange((from block in m_WidgetList.Entries.OfType<PlanetInfoSpaceSystemInformationWindowView>()
				where block.gameObject.activeInHierarchy
				select block).Cast<IConsoleNavigationEntity>().ToList());
		}
		if (m_WidgetListOtherObjects.Entries != null && m_WidgetListOtherObjects.Entries.OfType<OtherObjectsInfoSpaceSystemInformationWindowView>().Any())
		{
			list.AddRange((from block in m_WidgetListOtherObjects.Entries.OfType<OtherObjectsInfoSpaceSystemInformationWindowView>()
				where block.gameObject.activeInHierarchy
				select block).Cast<IConsoleNavigationEntity>().ToList());
		}
		if (m_WidgetListAdditionalAnomalies.Entries != null && m_WidgetListAdditionalAnomalies.Entries.OfType<AdditionalAnomaliesInfoSpaceSystemInformationWindowView>().Any())
		{
			list.AddRange((from block in m_WidgetListAdditionalAnomalies.Entries.OfType<AdditionalAnomaliesInfoSpaceSystemInformationWindowView>()
				where block.gameObject.activeInHierarchy
				select block).Cast<IConsoleNavigationEntity>().ToList());
		}
		if (!list.Any())
		{
			return;
		}
		m_SystemInformationWindowBehavior.SetEntitiesVertical(list);
		foreach (PlanetInfoSpaceSystemInformationWindowView item in list.Where((IConsoleNavigationEntity e) => e is PlanetInfoSpaceSystemInformationWindowView).ToList().OfType<PlanetInfoSpaceSystemInformationWindowView>())
		{
			item.SetParentNavigation(m_SystemInformationWindowBehavior);
		}
		DelayedInvoker.InvokeInTime(delegate
		{
			m_SystemInformationWindowBehavior.FocusOnFirstValidEntity();
		}, m_MoveWindowsSpeed + 0.01f);
	}

	private void ClearNavigation()
	{
		TooltipHelper.HideTooltip();
		m_SystemInformationWindowBehavior.UnFocusCurrentEntity();
		m_SystemInformationWindowBehavior.Clear();
	}

	private void ChangeFrontWindowPosition(bool isFront)
	{
		Vector2 endValue = new Vector2(isFront ? m_ShowInspectPosX : m_MoveBehindX, isFront ? m_ShowPosY : m_MoveBehindY);
		base.RectTransform.DOAnchorPos(endValue, m_MoveWindowsSpeed);
		if (!isFront)
		{
			ClearNavigation();
		}
		else
		{
			AddNavigation();
		}
	}

	public bool IsOnFront()
	{
		return base.ViewModel.IsInformSystemOnTheFront.Value;
	}

	public void ShowTooltipInfo()
	{
		IConsoleEntity consoleEntity = m_SystemInformationWindowBehavior.Entities.FirstOrDefault((IConsoleEntity e) => e is PlanetInfoSpaceSystemInformationWindowView planetInfoSpaceSystemInformationWindowView && planetInfoSpaceSystemInformationWindowView.IsFocused);
		PlanetInfoSpaceSystemInformationWindowView planet = consoleEntity as PlanetInfoSpaceSystemInformationWindowView;
		if (!(planet == null))
		{
			EventBus.RaiseEvent(delegate(ITooltipHandler h)
			{
				h.HandleInfoRequest(planet.CurrentTooltipInfo, m_SystemInformationWindowBehavior, shouldNotHideLittleTooltip: true);
			});
		}
	}
}
