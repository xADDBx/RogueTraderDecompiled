using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Career;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Console.Phases.Career;

public class CharGenCareerPhaseDetailedConsoleView : CharGenCareerPhaseDetailedView, IUpdateFocusHandler, ISubscriber
{
	[SerializeField]
	private ConsoleHint m_SelectHint;

	private readonly ReactiveProperty<ActivePhaseNavigation> m_ActivePhaseNavigation = new ReactiveProperty<ActivePhaseNavigation>(ActivePhaseNavigation.Content);

	private GridConsoleNavigationBehaviour m_Navigation;

	private GridConsoleNavigationBehaviour m_UnitProgressionNavigation;

	private readonly BoolReactiveProperty m_CanConfirm = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanDecline = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanFunc02 = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsInContent = new BoolReactiveProperty();

	private IDisposable m_DelayedNestedFocusUpdate;

	private readonly BoolReactiveProperty m_CanShowInfo = new BoolReactiveProperty();

	private bool m_HasTooltip;

	private TooltipConfig m_TooltipConfig;

	private IConsoleEntity m_ContentEntity;

	private IUpdateFocusHandler m_UpdateFocusHandlerImplementation;

	private UnitProgressionConsoleView UnitProgressionConsoleView => m_UnitProgressionView as UnitProgressionConsoleView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_TooltipConfig = new TooltipConfig
		{
			TooltipPlace = m_InfoView.GetComponent<RectTransform>(),
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		AddDisposable(m_UnitProgressionNavigation = new GridConsoleNavigationBehaviour());
		AddDisposable(m_CanDecline.Subscribe(delegate(bool value)
		{
			CanGoBackOnDecline.Value = !value;
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		TooltipHelper.HideTooltip();
		m_DelayedNestedFocusUpdate?.Dispose();
		m_DelayedNestedFocusUpdate = null;
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, BoolReactiveProperty isMainCharacter)
	{
		m_Navigation = navigationBehaviour;
		AddDisposable(m_ActivePhaseNavigation.Subscribe(UpdateActiveNavigation));
		AddDisposable(m_Navigation.DeepestFocusAsObservable.Subscribe(OnFocusEntity));
		AddDisposable(UnitProgressionConsoleView.NavigationBehaviour.DeepestFocusAsObservable.Subscribe());
		AddDisposable(inputLayer.AddAxis(Scroll, 3));
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			RefreshFocus();
		}, 8, m_CanConfirm.And(isMainCharacter).ToReactiveProperty());
		AddDisposable(m_SelectHint.Bind(inputBindStruct));
		AddDisposable(inputBindStruct);
		m_SelectHint.SetLabel(UIStrings.Instance.CommonTexts.Select);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			SwitchNavigation();
		}, 10, m_IsInContent.CombineLatest(base.ViewModel.UnitProgressionVM.State, (bool content, UnitProgressionWindowState state) => content && state == UnitProgressionWindowState.CareerPathList).ToReactiveProperty());
		AddDisposable(hintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Information));
		AddDisposable(inputBindStruct2);
		InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
		{
			RefreshFocus();
		}, 11, m_CanFunc02);
		AddDisposable(hintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CharGen.InspectCareer));
		AddDisposable(inputBindStruct3);
		InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
		{
			OnDeclineClick();
		}, 9, m_CanDecline);
		AddDisposable(hintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CommonTexts.Back));
		AddDisposable(inputBindStruct4);
		InputBindStruct inputBindStruct5 = inputLayer.AddButton(delegate
		{
			ToggleTooltip();
		}, 8, m_CanShowInfo);
		AddDisposable(hintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.CommonTexts.Information));
		AddDisposable(inputBindStruct5);
		UnitProgressionConsoleView.AddInput(ref inputLayer, ref m_UnitProgressionNavigation, hintsWidget);
		AddDisposable(base.ViewModel.UnitProgressionVM.PreselectedCareer.Subscribe(delegate
		{
			OnFocusEntity(UnitProgressionConsoleView.NavigationBehaviour.DeepestNestedFocus);
		}));
		m_Navigation.FocusOnFirstValidEntity();
	}

	private void SwitchNavigation()
	{
		switch (m_ActivePhaseNavigation.Value)
		{
		case ActivePhaseNavigation.Content:
			m_ActivePhaseNavigation.Value = ActivePhaseNavigation.SecondaryInfo;
			break;
		case ActivePhaseNavigation.SecondaryInfo:
			m_ActivePhaseNavigation.Value = ActivePhaseNavigation.Content;
			break;
		}
	}

	private void UpdateActiveNavigation(ActivePhaseNavigation activeNavigation)
	{
		TooltipHelper.HideTooltip();
		switch (activeNavigation)
		{
		case ActivePhaseNavigation.Content:
			SetContentNavigation();
			break;
		case ActivePhaseNavigation.SecondaryInfo:
			SetSecondaryInfoNavigation();
			break;
		}
		m_IsInContent.Value = m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content;
		m_CanDecline.Value = activeNavigation == ActivePhaseNavigation.SecondaryInfo;
	}

	private void SetContentNavigation()
	{
		IConsoleEntity contentEntity = m_ContentEntity;
		m_Navigation.Clear();
		m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_UnitProgressionNavigation);
		if (contentEntity != null)
		{
			m_UnitProgressionNavigation.FocusOnEntityManual(contentEntity);
			m_Navigation.FocusOnEntityManual(m_UnitProgressionNavigation);
		}
		else
		{
			m_Navigation.FocusOnFirstValidEntity();
		}
	}

	private void SetSecondaryInfoNavigation()
	{
		m_UnitProgressionNavigation.FocusOnEntityManual(m_Navigation.DeepestNestedFocus);
		m_Navigation.Clear();
		m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_InfoView.GetNavigationBehaviour());
		m_Navigation.FocusOnFirstValidEntity();
		OnFocusEntity(m_Navigation.DeepestNestedFocus);
	}

	private void RefreshFocus()
	{
		OnFocusEntity(m_Navigation.DeepestNestedFocus);
	}

	private void Scroll(InputActionEventData data, float y)
	{
		if (!(Mathf.Abs(data.player.GetAxis(3)) < Mathf.Abs(data.player.GetAxis(2))) && (!(y > 0f) || !m_InfoView.ScrollbarOnTop) && (!(y < 0f) || !m_InfoView.ScrollbarOnBottom))
		{
			m_InfoView.Scroll(y);
		}
	}

	private void OnDeclineClick()
	{
		SwitchNavigation();
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		bool flag = base.ViewModel.UnitProgressionVM.State.Value == UnitProgressionWindowState.CareerPathList;
		bool flag2 = flag && base.ViewModel.UnitProgressionVM.AllCareerPaths.Any((CareerPathVM path) => path.IsSelected.Value);
		bool flag3 = IsSelectedOrHighTier(entity);
		flag2 = flag2 && flag3;
		m_CanConfirm.Value = m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content && !flag2 && ((entity as IConfirmClickHandler)?.CanConfirmClick() ?? false);
		if (entity is TMPLinkNavigationEntity)
		{
			m_CanConfirm.Value = false;
		}
		m_CanFunc02.Value = (entity as IFunc02ClickHandler)?.CanFunc02Click() ?? false;
		CanGoNextOnConfirm.Value = flag2 && m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content;
		CanGoBackOnDecline.Value = flag && m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content;
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_HasTooltip = tooltipBaseTemplate != null;
		m_CanShowInfo.Value = m_HasTooltip && m_ActivePhaseNavigation.Value == ActivePhaseNavigation.SecondaryInfo;
		if (flag)
		{
			TooltipHelper.HideTooltip();
			if (m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Content)
			{
				base.ViewModel.InfoVM.SetTemplate(tooltipBaseTemplate);
				m_ContentEntity = entity;
			}
			else if (CharGenConsoleView.ShowTooltip && m_ActivePhaseNavigation.Value == ActivePhaseNavigation.SecondaryInfo)
			{
				MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
				if ((bool)monoBehaviour)
				{
					monoBehaviour.ShowConsoleTooltip(tooltipBaseTemplate, m_Navigation, m_TooltipConfig);
				}
			}
		}
		else if (base.ViewModel.InfoVM.CurrentTooltip != null)
		{
			base.ViewModel.InfoVM.SetTemplate(null);
		}
		string confirmClickHint = entity.GetConfirmClickHint();
		m_SelectHint.SetLabel((!string.IsNullOrEmpty(confirmClickHint)) ? confirmClickHint : ((string)UIStrings.Instance.CommonTexts.Select));
	}

	private void ToggleTooltip()
	{
		CharGenConsoleView.ShowTooltip = !CharGenConsoleView.ShowTooltip;
		OnFocusEntity(m_Navigation.DeepestNestedFocus);
	}

	private bool IsEntitySelected(IConsoleEntity entity)
	{
		if (!(entity is CareerPathListItemCommonView careerPathListItemCommonView))
		{
			return false;
		}
		return (careerPathListItemCommonView.GetViewModel() as CareerPathVM).IsSelected.Value;
	}

	private bool IsSelectedOrHighTier(IConsoleEntity entity)
	{
		if (!(entity is CareerPathListItemCommonView careerPathListItemCommonView))
		{
			return false;
		}
		CareerPathVM careerPathVM = careerPathListItemCommonView.GetViewModel() as CareerPathVM;
		if (careerPathVM == null || careerPathVM.CareerPath.Tier != 0 || !careerPathVM.IsSelected.Value)
		{
			if (careerPathVM == null)
			{
				return true;
			}
			return careerPathVM.CareerPath.Tier != CareerPathTier.One;
		}
		return true;
	}

	public void HandleFocus()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			OnFocusEntity(m_Navigation.DeepestNestedFocus);
		}, 1);
	}
}
