using System.Collections;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathList;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console;

public class UnitProgressionConsoleView : UnitProgressionCommonView, ICharInfoComponentConsoleView, ICharInfoComponentView, IConsoleNavigationOwner, IConsoleEntity, ICharInfoCanHookDecline, ICharInfoCanHookConfirm
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private ConsoleHintsWidget m_HintsWidget;

	private Coroutine m_AddInputCo;

	private bool m_InputAdded;

	private readonly BoolReactiveProperty m_CanHookDecline = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanFuncAdditional = new BoolReactiveProperty();

	private CareerPathsListsConsoleView CareerPathsListsConsoleView => m_CareerPathsListsCommonView as CareerPathsListsConsoleView;

	private CareerPathProgressionConsoleView CareerPathProgressionConsoleView => m_CareerPathProgressionCommonView as CareerPathProgressionConsoleView;

	public GridConsoleNavigationBehaviour NavigationBehaviour => m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateNavigation();
	}

	protected override void DestroyViewImplementation()
	{
		if (m_AddInputCo != null)
		{
			StopCoroutine(m_AddInputCo);
		}
		base.DestroyViewImplementation();
		m_NavigationBehaviour?.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
		m_AddInputCo = null;
		m_InputAdded = false;
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		m_InputLayer = inputLayer;
		m_HintsWidget = hintsWidget;
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
		if (base.ViewModel.CurrentCareer.Value != null)
		{
			CareerPathProgressionConsoleView.AddInput(ref inputLayer, ref hintsWidget);
		}
		else
		{
			CareerPathsListsConsoleView.AddInput(ref inputLayer, ref hintsWidget);
		}
		if (!m_InputAdded)
		{
			InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
			{
				OnFuncAdditionalClick();
			}, 17, m_CanFuncAdditional, InputActionEventType.ButtonJustReleased);
			AddDisposable(inputBindStruct);
			AddDisposable(hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CharacterSheet.ToggleFavorites));
			m_InputAdded = true;
		}
	}

	protected override void HandleState(UnitProgressionWindowState state)
	{
		base.HandleState(state);
		TooltipHelper.HideTooltip();
		UpdateNavigation();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		UpdateNavigation();
	}

	protected override void BindPathProgression(CareerPathVM careerPathVM)
	{
		base.BindPathProgression(careerPathVM);
		if (careerPathVM != null)
		{
			m_AddInputCo = StartCoroutine(AddCareerPathWhenHasInput());
		}
		m_CanHookDecline.Value = careerPathVM != null;
	}

	private IEnumerator AddCareerPathWhenHasInput()
	{
		while (m_InputLayer == null)
		{
			yield return null;
		}
		CareerPathProgressionConsoleView.AddInput(ref m_InputLayer, ref m_HintsWidget);
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		if (m_NavigationBehaviour != null)
		{
			bool isFocused = m_NavigationBehaviour.IsFocused;
			m_NavigationBehaviour.Clear();
			UnitProgressionWindowState value = base.ViewModel.State.Value;
			ConsoleNavigationBehaviour entity = value switch
			{
				UnitProgressionWindowState.CareerPathList => CareerPathsListsConsoleView.GetNavigationBehaviour(this), 
				UnitProgressionWindowState.CareerPathProgression => CareerPathProgressionConsoleView.GetNavigationBehaviour(this), 
				_ => throw new SwitchExpressionException(value), 
			};
			m_NavigationBehaviour.AddEntityVertical(entity);
			if (isFocused)
			{
				m_NavigationBehaviour.FocusOnEntityManual(entity);
			}
		}
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		m_CanFuncAdditional.Value = (entity as IFuncAdditionalClickHandler)?.CanFuncAdditionalClick() ?? false;
	}

	private void OnFuncAdditionalClick()
	{
		if (m_NavigationBehaviour.DeepestNestedFocus is IFuncAdditionalClickHandler funcAdditionalClickHandler)
		{
			funcAdditionalClickHandler.OnFuncAdditionalClick();
		}
	}

	public void EntityFocused(IConsoleEntity entity)
	{
		if (entity != null)
		{
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
		}
	}

	public IReadOnlyReactiveProperty<bool> GetCanHookDeclineProperty()
	{
		return m_CanHookDecline;
	}

	public IReadOnlyReactiveProperty<bool> GetCanHookConfirmProperty()
	{
		return CareerPathProgressionConsoleView.GetCanHookConfirmProperty();
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
