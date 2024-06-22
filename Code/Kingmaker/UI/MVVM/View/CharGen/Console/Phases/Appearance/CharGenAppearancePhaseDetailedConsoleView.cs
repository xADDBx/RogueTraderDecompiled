using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Base;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Console.Phases.Appearance;

public class CharGenAppearancePhaseDetailedConsoleView : CharGenAppearancePhaseDetailedView, IConsoleNavigationEntity, IConsoleEntity, INavigationDirectionsHandler, INavigationVerticalDirectionsHandler, INavigationUpDirectionHandler, INavigationDownDirectionHandler, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, INavigationRightDirectionHandler, ICharGenAppearancePageComponentHandler, ISubscriber
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_SelectHint;

	[SerializeField]
	private ConsoleHint m_SwitchNavigationHint;

	private InputLayer m_InputLayer;

	private ConsoleHintsWidget m_HintsWidget;

	private GridConsoleNavigationBehaviour m_Navigation;

	private GridConsoleNavigationBehaviour m_ContentNavigation;

	private readonly BoolReactiveProperty m_CanConfirm = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanFunc01 = new BoolReactiveProperty();

	private bool m_ShouldSelectLastContentEntity;

	private ConsoleHintDescription m_Func01Hint;

	private readonly ReactiveProperty<ActivePhaseNavigation> m_ActivePhaseNavigation = new ReactiveProperty<ActivePhaseNavigation>(ActivePhaseNavigation.Content);

	private readonly BoolReactiveProperty m_CanDecline = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanSwitchNavigation = new BoolReactiveProperty(initialValue: true);

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_ContentNavigation = new GridConsoleNavigationBehaviour());
		AddDisposable(base.ViewModel.OnPageChanged.Subscribe(HandlePageChanged));
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, BoolReactiveProperty isMainCharacter)
	{
		m_InputLayer = inputLayer;
		m_HintsWidget = hintsWidget;
		m_Navigation = navigationBehaviour;
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			OnConfirmClick();
		}, 8, m_CanConfirm.And(isMainCharacter).ToReactiveProperty());
		AddDisposable(m_SelectHint.Bind(inputBindStruct));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			OnFunc01Click();
		}, 10, m_CanFunc01.And(isMainCharacter).ToReactiveProperty());
		AddDisposable(m_Func01Hint = hintsWidget.BindHint(inputBindStruct2, string.Empty) as ConsoleHintDescription);
		AddDisposable(inputBindStruct2);
		AddDisposable(m_ActivePhaseNavigation.Subscribe(UpdateActiveNavigation));
		if (PaperHints != null)
		{
			InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
			{
				base.ViewModel.GoPrevPage();
			}, 12, base.ViewModel.CurrentPageIsFirst.Not().And(isMainCharacter).ToReactiveProperty());
			AddDisposable(PaperHints.PageUpHint.Bind(inputBindStruct3));
			AddDisposable(inputBindStruct3);
			InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
			{
				base.ViewModel.GoNextPage();
			}, 13, base.ViewModel.CurrentPageIsLast.Not().And(isMainCharacter).ToReactiveProperty());
			AddDisposable(PaperHints.PageDownHint.Bind(inputBindStruct4));
			AddDisposable(inputBindStruct4);
		}
	}

	private void UpdateContentNavigation()
	{
		if (m_ContentNavigation == null || m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Menu)
		{
			return;
		}
		m_ContentNavigation.Clear();
		GridConsoleNavigationBehaviour vListNav = m_VirtualList.GetNavigationBehaviour();
		m_ContentNavigation.AddEntityHorizontal(vListNav);
		if (m_ShouldSelectLastContentEntity)
		{
			vListNav.FocusOnLastValidEntity();
			m_ShouldSelectLastContentEntity = false;
		}
		else
		{
			vListNav.FocusOnFirstValidEntity();
		}
		if (vListNav.CurrentEntity is VirtualListElement virtualListElement)
		{
			m_VirtualList.ScrollController.ForceScrollToElement(virtualListElement.Data);
		}
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_ContentNavigation.FocusOnEntityManual(vListNav);
			vListNav.FocusOnCurrentEntity();
			OnFocusEntity(m_ContentNavigation.DeepestNestedFocus);
			foreach (VirtualListElement element in m_VirtualList.Elements)
			{
				if (element.View is ICharGenAppearancePageComponent charGenAppearancePageComponent)
				{
					charGenAppearancePageComponent.AddInput(ref m_InputLayer, m_HintsWidget);
				}
			}
		}, 3);
	}

	private void OnFunc01Click()
	{
		m_ContentNavigation.OnFunc01Click();
		OnFocusEntity(m_ContentNavigation.DeepestNestedFocus);
	}

	protected virtual void OnFocusEntity(IConsoleEntity entity)
	{
		m_CanConfirm.Value = (entity as IConfirmClickHandler)?.CanConfirmClick() ?? false;
		CanGoNextOnConfirm.Value = !m_CanConfirm.Value;
		string text = (entity as IConfirmClickHandler)?.GetConfirmClickHint();
		if (string.IsNullOrEmpty(text))
		{
			text = UIStrings.Instance.CommonTexts.Select;
		}
		m_SelectHint.Or(null)?.SetLabel(text);
		m_CanFunc01.Value = (entity as IFunc01ClickHandler)?.CanFunc01Click() ?? false;
		string label = (entity as IFunc01ClickHandler)?.GetFunc01ClickHint();
		m_Func01Hint?.SetLabel(label);
	}

	public override bool PressConfirmOnPhase()
	{
		if (m_ActivePhaseNavigation.Value == ActivePhaseNavigation.Menu)
		{
			if (!base.ViewModel.GoNextPage())
			{
				return base.PressConfirmOnPhase();
			}
			return false;
		}
		if (m_ContentNavigation.CanConfirmClick())
		{
			m_ContentNavigation.OnConfirmClick();
		}
		else
		{
			if (SelectNextVirtualListEntity())
			{
				return false;
			}
			if (!base.ViewModel.GoNextPage())
			{
				return base.PressConfirmOnPhase();
			}
		}
		return false;
	}

	public override bool PressDeclineOnPhase()
	{
		if (SelectPrevVirtualListEntity())
		{
			return false;
		}
		if (!base.ViewModel.GoPrevPage())
		{
			return base.PressDeclineOnPhase();
		}
		return false;
	}

	protected override void HandlePageChanged(CharGenAppearancePageType pageType)
	{
		if (m_ActivePhaseNavigation.Value != 0)
		{
			m_VirtualList.ScrollController.ForceScrollToTop();
			base.HandlePageChanged(pageType);
			UpdateContentNavigation();
		}
	}

	public bool HandleUp()
	{
		bool flag = m_ContentNavigation.HandleUp();
		OnFocusEntity(m_ContentNavigation.DeepestNestedFocus);
		if (!flag && UINetUtility.IsControlMainCharacter())
		{
			m_ShouldSelectLastContentEntity = true;
			flag = base.ViewModel.GoPrevPage();
		}
		return flag;
	}

	public bool HandleDown()
	{
		bool flag = m_ContentNavigation.HandleDown();
		OnFocusEntity(m_ContentNavigation.DeepestNestedFocus);
		if (!flag && UINetUtility.IsControlMainCharacter())
		{
			flag = base.ViewModel.GoNextPage();
		}
		return flag;
	}

	public bool HandleLeft()
	{
		bool result = m_ContentNavigation.HandleLeft();
		OnFocusEntity(m_ContentNavigation.DeepestNestedFocus);
		return result;
	}

	public bool HandleRight()
	{
		bool result = m_ContentNavigation.HandleRight();
		OnFocusEntity(m_ContentNavigation.DeepestNestedFocus);
		return result;
	}

	private void OnConfirmClick()
	{
		m_ContentNavigation.OnConfirmClick();
		OnFocusEntity(m_ContentNavigation.DeepestNestedFocus);
	}

	private bool SelectNextVirtualListEntity()
	{
		GridConsoleNavigationBehaviour navigationBehaviour = m_VirtualList.GetNavigationBehaviour();
		if (navigationBehaviour.CurrentEntity == navigationBehaviour.Entities.LastOrDefault((IConsoleEntity i) => i.IsValid()))
		{
			return false;
		}
		int num = navigationBehaviour.Entities.IndexOf(navigationBehaviour.CurrentEntity);
		IConsoleEntity consoleEntity = navigationBehaviour.Entities.Skip(num + 1).FirstOrDefault((IConsoleEntity i) => i.IsValid());
		if (consoleEntity != null)
		{
			navigationBehaviour.FocusOnEntityManual(consoleEntity);
			if (consoleEntity is VirtualListElement virtualListElement)
			{
				m_VirtualList.ScrollController.ForceScrollToElement(virtualListElement.Data);
			}
			OnFocusEntity(navigationBehaviour.DeepestNestedFocus);
			return true;
		}
		return false;
	}

	private bool SelectPrevVirtualListEntity()
	{
		GridConsoleNavigationBehaviour navigationBehaviour = m_VirtualList.GetNavigationBehaviour();
		if (navigationBehaviour.CurrentEntity == navigationBehaviour.Entities.FirstOrDefault((IConsoleEntity i) => i.IsValid()))
		{
			return false;
		}
		int count = navigationBehaviour.Entities.IndexOf(navigationBehaviour.CurrentEntity);
		IConsoleEntity consoleEntity = navigationBehaviour.Entities.Take(count).LastOrDefault((IConsoleEntity i) => i.IsValid());
		if (consoleEntity != null)
		{
			navigationBehaviour.FocusOnEntityManual(consoleEntity);
			if (consoleEntity is VirtualListElement virtualListElement)
			{
				m_VirtualList.ScrollController.ForceScrollToElement(virtualListElement.Data);
			}
			OnFocusEntity(navigationBehaviour.DeepestNestedFocus);
			return true;
		}
		return false;
	}

	public void SetFocus(bool value)
	{
	}

	public bool IsValid()
	{
		return true;
	}

	private void SetMenuNavigation()
	{
		m_Navigation.Clear();
		m_ContentNavigation.Clear();
		if (!UINetUtility.IsControlMainCharacter())
		{
			return;
		}
		foreach (VirtualListElement element in m_VirtualList.Elements)
		{
			if (element.View is ICharGenAppearancePageComponent charGenAppearancePageComponent)
			{
				charGenAppearancePageComponent.RemoveInput();
			}
		}
		m_Navigation.AddColumn(m_PageSelectorView.GetNavigationEntities());
		m_Navigation.FocusOnEntityManual(m_PageSelectorView.GetSelectedEntity());
	}

	private void SetContentNavigation()
	{
		m_Navigation.Clear();
		UpdateContentNavigation();
		m_Navigation.AddEntityVertical(this);
		m_Navigation.FocusOnEntityManual(this);
	}

	private void UpdateActiveNavigation(ActivePhaseNavigation activeNavigation)
	{
		if (activeNavigation == ActivePhaseNavigation.Menu)
		{
			SetMenuNavigation();
			m_SwitchNavigationHint.Or(null)?.SetLabel(UIStrings.Instance.CharGen.SwitchToAppearance);
		}
		else
		{
			SetContentNavigation();
			m_SwitchNavigationHint.Or(null)?.SetLabel(UIStrings.Instance.CharGen.SwitchToPantograph);
		}
		m_CanDecline.Value = activeNavigation == ActivePhaseNavigation.Content;
		m_CanSwitchNavigation.Value = activeNavigation == ActivePhaseNavigation.Menu;
		HandlePageChanged(base.ViewModel.CurrentPageVM.Value.PageType);
		OnFocusEntity(m_Navigation.DeepestNestedFocus);
	}

	private void SwitchNavigation()
	{
		m_ActivePhaseNavigation.Value = ((m_ActivePhaseNavigation.Value != ActivePhaseNavigation.Content) ? ActivePhaseNavigation.Content : ActivePhaseNavigation.Menu);
	}
}
