using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Localization;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SelectorWindow;

public class SelectorWindowConsoleView<TEntityView, TEntityVM> : SelectorWindowBaseView<TEntityView, TEntityVM> where TEntityView : VirtualListElementViewBase<TEntityVM>, IHasTooltipTemplate where TEntityVM : SelectionGroupEntityVM
{
	[SerializeField]
	protected ConsoleHintsWidget m_ConsoleHintsWidget;

	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected InputLayer m_InputLayer;

	public ReactiveProperty<IConsoleEntity> SelectedEntity => m_NavigationBehaviour?.DeepestFocusAsObservable;

	protected virtual LocalizedString ConfirmText => UIStrings.Instance.CommonTexts.Accept;

	protected virtual bool ShouldCloseOnConfirm => true;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetupInput();
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator.DisappearAnimation();
	}

	private void SetupInput()
	{
		m_NavigationBehaviour = m_VirtualList.GetNavigationBehaviour();
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "SelectorWindowConsoleView"
		});
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(base.OnClose, 9), UIStrings.Instance.CommonTexts.Back));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(OnConfirm, 8, CanEquip), ConfirmText));
		m_InputLayer.AddAxis(Scroll, 3, repeat: true);
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(EntityFocused));
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
		AddDisposable(m_SortingComponent.PushView());
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_NavigationBehaviour.FocusOnFirstValidEntity();
		}, 1);
	}

	private void OnConfirm(InputActionEventData inputActionEventData)
	{
		TEntityVM entityVM = ((IHasViewModel)m_NavigationBehaviour.DeepestFocusAsObservable.Value).GetViewModel() as TEntityVM;
		base.ViewModel.Confirm(entityVM);
		if (ShouldCloseOnConfirm)
		{
			OnClose(inputActionEventData);
		}
	}

	private void Scroll(InputActionEventData obj, float x)
	{
		m_InfoSectionView.Scroll(x);
	}

	protected virtual void EntityFocused(IConsoleEntity entity)
	{
		base.ViewModel.InfoSectionVM.SetTemplate((entity as IHasTooltipTemplate)?.TooltipTemplate());
	}
}
