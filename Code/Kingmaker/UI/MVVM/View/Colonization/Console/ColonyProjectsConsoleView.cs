using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Console;

public class ColonyProjectsConsoleView : ColonyProjectsBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private ColonyProjectsNavigationConsoleView m_Navigation;

	[SerializeField]
	[UsedImplicitly]
	private ColonyProjectsPageConsoleView m_Page;

	[SerializeField]
	[UsedImplicitly]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	protected override void InitializeImpl()
	{
		m_Page.Initialize();
		m_Navigation.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_Navigation.Bind(base.ViewModel.NavigationVM);
		m_Page.Bind(base.ViewModel.ColonyProjectPageVM);
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour.Focus.Subscribe(m_Navigation.ScrollMenu));
		AddDisposable(m_PageNavigationBehavior.Focus.Subscribe(m_Page.ScrollList));
	}

	protected override void DestroyViewImplementation()
	{
		m_ShowTooltip = false;
		base.DestroyViewImplementation();
	}

	protected override void UpdateNavigationImpl()
	{
		m_NavigationBehaviour.SetEntitiesVertical(m_Navigation.GetNavigationEntities());
	}

	protected override void CreateInputImpl()
	{
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			HandleStartProject();
		}, 10, base.ViewModel.StartAvailable, InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.ColonyProjectsTexts.StartProjectButton));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			HandleShowBlockedProjects();
		}, 11), UIStrings.Instance.ColonyProjectsTexts.ShowBlockedProjectsButton, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			HandleShowFinishedProjects();
		}, 17), UIStrings.Instance.ColonyProjectsTexts.ShowFinishedProjectsButton, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			HandleClose();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ShowPage();
		}, 8), UIStrings.Instance.CommonTexts.Information));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_PageInputLayer.AddButton(delegate
		{
			ClosePage();
		}, 9, m_PageMode), UIStrings.Instance.CommonTexts.Back, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_PageInputLayer.AddButton(ToggleTooltip, 19, m_PageMode, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.Information));
		AddDisposable(m_InputLayer.AddAxis(Scroll, 3));
	}

	private void Scroll(InputActionEventData data, float y)
	{
		if (!(Mathf.Abs(data.player.GetAxis(3)) < Mathf.Abs(data.player.GetAxis(2))))
		{
			m_Page.RewardsScrollRect.Scroll(y, smooth: true);
		}
	}

	protected override void SetPageNavigationImpl()
	{
		List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
		list.AddRange(m_Page.GetRewardsNavigationEntities());
		list.AddRange(m_Page.GetRequirementsNavigationEntities());
		m_PageNavigationBehavior.SetEntitiesVertical(list);
	}

	protected override void OnPageFocusChangedImpl(IConsoleEntity entity)
	{
		if (entity is IColonyProjectsDetailsEntity colonyProjectsDetailsEntity)
		{
			ShowTooltip(colonyProjectsDetailsEntity);
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip = !RootUIContext.Instance.TooltipIsShown;
		OnPageFocusChangedImpl(m_PageNavigationBehavior.DeepestNestedFocus);
	}

	private void ShowTooltip(IColonyProjectsDetailsEntity colonyProjectsDetailsEntity)
	{
		if (m_ShowTooltip)
		{
			colonyProjectsDetailsEntity.ShowTooltip();
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
	}
}
