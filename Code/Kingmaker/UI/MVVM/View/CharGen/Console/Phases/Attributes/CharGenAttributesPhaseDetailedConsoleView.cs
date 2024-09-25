using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Attributes;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Console.Phases.Attributes;

public class CharGenAttributesPhaseDetailedConsoleView : CharGenAttributesPhaseDetailedView
{
	[Header("Description")]
	[SerializeField]
	private GameObject m_SecondaryInfoViewContainer;

	[SerializeField]
	private InfoSectionView m_SecondaryInfoView;

	private GridConsoleNavigationBehaviour m_Navigation;

	private GridConsoleNavigationBehaviour m_MenuNavigation;

	private readonly ReactiveProperty<ActivePhaseNavigation> m_ActivePhaseNavigation = new ReactiveProperty<ActivePhaseNavigation>(ActivePhaseNavigation.Menu);

	private readonly BoolReactiveProperty m_CanConfirm = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanDecline = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanShowInfo = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanSwitchNavigation = new BoolReactiveProperty();

	private bool m_HasTooltip;

	private bool m_ShowTooltip = true;

	private TooltipConfig m_TooltipConfig;

	private IConsoleEntity m_PreviousEntity;

	private ActivePhaseNavigation m_PreviousState;

	private bool m_VerticalEntitiesAdded;

	private CharInfoSkillsBlockConsoleView CharInfoSkillsBlockConsoleView => m_CharInfoSkillsBlockView as CharInfoSkillsBlockConsoleView;

	public override void Initialize()
	{
		base.Initialize();
		if ((bool)m_SecondaryInfoViewContainer)
		{
			m_SecondaryInfoViewContainer.SetActive(value: false);
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_VerticalEntitiesAdded = false;
		if ((bool)m_SecondaryInfoView)
		{
			m_SecondaryInfoView.Bind(base.ViewModel.SecondaryInfoVM);
		}
		m_TooltipConfig = new TooltipConfig
		{
			TooltipPlace = m_SecondaryInfoView.GetComponent<RectTransform>(),
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		AddDisposable(base.ViewModel.IsCompletedAndAvailable.CombineLatest(m_ActivePhaseNavigation, (bool isCompleted, ActivePhaseNavigation navigation) => new { isCompleted, navigation }).Subscribe(value =>
		{
			CanGoNextOnConfirm.Value = value.isCompleted && value.navigation == ActivePhaseNavigation.Menu;
		}));
		AddDisposable(m_CanDecline.Subscribe(delegate(bool value)
		{
			CanGoBackOnDecline.Value = !value;
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.OnUpdateState, delegate
		{
			OnFocusChanged(m_Navigation?.DeepestNestedFocus);
		}));
		if ((bool)m_SecondaryInfoViewContainer)
		{
			m_SecondaryInfoViewContainer.SetActive(value: true);
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_VerticalEntitiesAdded = false;
		base.DestroyViewImplementation();
		TooltipHelper.HideTooltip();
		TooltipHelper.HideInfo();
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, BoolReactiveProperty isMainCharacter)
	{
		m_Navigation = navigationBehaviour;
		AddDisposable(m_MenuNavigation = new GridConsoleNavigationBehaviour());
		AddDisposable(isMainCharacter.Subscribe(delegate(bool value)
		{
			if (value && !m_VerticalEntitiesAdded)
			{
				m_MenuNavigation.SetEntitiesVertical(m_CharGenAttributesPhaseSelectorView.GetNavigationEntities());
				m_VerticalEntitiesAdded = true;
			}
		}));
		if (isMainCharacter.Value)
		{
			m_MenuNavigation.SetEntitiesVertical(m_CharGenAttributesPhaseSelectorView.GetNavigationEntities());
			m_VerticalEntitiesAdded = true;
		}
		AddDisposable(m_ActivePhaseNavigation.Subscribe(UpdateActiveNavigation));
		AddDisposable(m_Navigation.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
		AddDisposable(inputLayer.AddAxis(Scroll, 3));
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			SwitchNavigation();
		}, 10, m_CanSwitchNavigation);
		AddDisposable(hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CharGen.Skills));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			ForceSetNavigation(ActivePhaseNavigation.SecondaryInfo);
		}, 11, m_CanSwitchNavigation);
		AddDisposable(hintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Information));
		AddDisposable(inputBindStruct2);
		InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
		{
			OnDeclineClick();
		}, 9, m_CanDecline);
		AddDisposable(hintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CommonTexts.Back));
		AddDisposable(inputBindStruct3);
		InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
		{
			ToggleTooltip();
		}, 8, m_CanShowInfo);
		AddDisposable(hintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CommonTexts.Information));
		AddDisposable(inputBindStruct4);
		InputBindStruct inputBindStruct5 = inputLayer.AddButton(delegate
		{
			SwitchNavigation();
		}, 8, m_ActivePhaseNavigation.Select((ActivePhaseNavigation f) => f == ActivePhaseNavigation.Content).ToReactiveProperty());
		AddDisposable(hintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.CommonTexts.Information));
		AddDisposable(inputBindStruct5);
		AddInputToPaperHints(ref inputLayer, ref isMainCharacter);
	}

	private void AddInputToPaperHints(ref InputLayer inputLayer, ref BoolReactiveProperty isMainCharacter)
	{
		if (PaperHints != null)
		{
			InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
			{
				base.ViewModel.GoPrevPage();
				RefreshMenuFocus();
			}, 12, base.ViewModel.CurrentPageIsFirst.CombineLatest(m_ActivePhaseNavigation, (bool isFirst, ActivePhaseNavigation navigation) => !isFirst && navigation == ActivePhaseNavigation.Menu).And(isMainCharacter).ToReactiveProperty());
			AddDisposable(PaperHints.PageUpHint.Bind(inputBindStruct));
			AddDisposable(inputBindStruct);
			InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
			{
				base.ViewModel.GoNextPage();
				RefreshMenuFocus();
			}, 13, base.ViewModel.CurrentPageIsLast.CombineLatest(m_ActivePhaseNavigation, (bool isLast, ActivePhaseNavigation navigation) => !isLast && navigation == ActivePhaseNavigation.Menu).And(isMainCharacter).ToReactiveProperty());
			AddDisposable(PaperHints.PageDownHint.Bind(inputBindStruct2));
			AddDisposable(inputBindStruct2);
		}
	}

	private void RefreshMenuFocus()
	{
		m_MenuNavigation.FocusOnEntityManual(m_CharGenAttributesPhaseSelectorView.GetSelectedEntity());
	}

	private void SetMenuNavigation()
	{
		m_Navigation.Clear();
		if (UINetUtility.IsControlMainCharacter())
		{
			m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_MenuNavigation);
			m_MenuNavigation.FocusOnEntityManual(m_CharGenAttributesPhaseSelectorView.GetSelectedEntity());
			m_Navigation.FocusOnEntityManual(m_MenuNavigation);
		}
	}

	private void SetContentNavigation()
	{
		IConsoleEntity previousEntity = m_PreviousEntity;
		m_Navigation.Clear();
		m_Navigation.AddColumn<IConsoleEntity>(CharInfoSkillsBlockConsoleView.GetConsoleEntity(1));
		if (m_PreviousState == ActivePhaseNavigation.SecondaryInfo)
		{
			m_Navigation.FocusOnEntityManual(previousEntity);
		}
		else
		{
			m_Navigation.FocusOnFirstValidEntity();
		}
	}

	private void SetSecondaryInfoNavigation()
	{
		m_Navigation.Clear();
		m_Navigation.AddColumn<GridConsoleNavigationBehaviour>(m_SecondaryInfoView.GetNavigationBehaviour());
		m_Navigation.FocusOnFirstValidEntity();
		OnFocusChanged(m_Navigation.DeepestNestedFocus);
	}

	private void UpdateActiveNavigation(ActivePhaseNavigation activeNavigation)
	{
		TooltipHelper.HideTooltip();
		switch (activeNavigation)
		{
		case ActivePhaseNavigation.Menu:
			SetMenuNavigation();
			break;
		case ActivePhaseNavigation.Content:
			SetContentNavigation();
			break;
		case ActivePhaseNavigation.SecondaryInfo:
			SetSecondaryInfoNavigation();
			break;
		}
		m_CanDecline.Value = activeNavigation == ActivePhaseNavigation.Content || activeNavigation == ActivePhaseNavigation.SecondaryInfo;
		m_CanSwitchNavigation.Value = activeNavigation == ActivePhaseNavigation.Menu;
	}

	private void SwitchNavigation(bool forward = true)
	{
		m_PreviousState = m_ActivePhaseNavigation.Value;
		switch (m_ActivePhaseNavigation.Value)
		{
		case ActivePhaseNavigation.Menu:
			m_ActivePhaseNavigation.Value = ActivePhaseNavigation.Content;
			break;
		case ActivePhaseNavigation.Content:
			m_ActivePhaseNavigation.Value = (forward ? ActivePhaseNavigation.SecondaryInfo : ActivePhaseNavigation.Menu);
			break;
		case ActivePhaseNavigation.SecondaryInfo:
			m_ActivePhaseNavigation.Value = ActivePhaseNavigation.Content;
			break;
		}
	}

	private void ForceSetNavigation(ActivePhaseNavigation forcedNavigation)
	{
		m_PreviousState = m_ActivePhaseNavigation.Value;
		m_ActivePhaseNavigation.Value = forcedNavigation;
	}

	private void Scroll(InputActionEventData data, float y)
	{
		if (!(Mathf.Abs(data.player.GetAxis(3)) < Mathf.Abs(data.player.GetAxis(2))) && (bool)m_SecondaryInfoView && (!(y > 0f) || !m_SecondaryInfoView.ScrollbarOnTop) && (!(y < 0f) || !m_SecondaryInfoView.ScrollbarOnBottom))
		{
			m_SecondaryInfoView.Scroll(y);
		}
	}

	private void OnDeclineClick()
	{
		if (m_ActivePhaseNavigation.Value == ActivePhaseNavigation.SecondaryInfo)
		{
			ForceSetNavigation(m_PreviousState);
		}
		else
		{
			SwitchNavigation(forward: false);
		}
	}

	protected virtual void OnFocusChanged(IConsoleEntity entity)
	{
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		TooltipHelper.HideTooltip();
		m_HasTooltip = tooltipBaseTemplate != null;
		m_CanShowInfo.Value = m_HasTooltip && m_ActivePhaseNavigation.Value == ActivePhaseNavigation.SecondaryInfo;
		ActivePhaseNavigation value = m_ActivePhaseNavigation.Value;
		if (value == ActivePhaseNavigation.Content || value == ActivePhaseNavigation.Menu)
		{
			m_CanConfirm.Value = !base.ViewModel.IsCompletedAndAvailable.Value;
			base.ViewModel.SecondaryInfoVM.SetTemplate(tooltipBaseTemplate);
			m_PreviousEntity = entity;
			return;
		}
		m_CanConfirm.Value = false;
		if (m_ShowTooltip)
		{
			MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
			if ((bool)monoBehaviour)
			{
				monoBehaviour.ShowConsoleTooltip(tooltipBaseTemplate, m_Navigation, m_TooltipConfig);
			}
		}
	}

	private void ToggleTooltip()
	{
		m_ShowTooltip = !m_ShowTooltip;
		OnFocusChanged(m_Navigation.DeepestNestedFocus);
	}
}
