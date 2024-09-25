using Kingmaker.Code.UI.MVVM;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases;

public abstract class CharGenPhaseDetailedView<TViewModel> : ViewBase<TViewModel>, ICharGenPhaseDetailedView, IInitializable where TViewModel : CharGenPhaseBaseVM
{
	[SerializeField]
	private FadeAnimator m_PageAnimator;

	protected readonly BoolReactiveProperty CanGoNextOnConfirm = new BoolReactiveProperty();

	protected readonly BoolReactiveProperty CanGoNextInMenu = new BoolReactiveProperty();

	protected readonly BoolReactiveProperty CanGoBackOnDecline = new BoolReactiveProperty(initialValue: true);

	protected PaperHints PaperHints;

	public bool HasYScrollBind => HasYScrollBindInternal;

	protected virtual bool HasYScrollBindInternal => true;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetPaperHints(PaperHints paperHints)
	{
		PaperHints = paperHints;
	}

	protected override void BindViewImplementation()
	{
		if (base.ViewModel == null)
		{
			DestroyViewImplementation();
		}
		else
		{
			Show();
		}
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private void Show()
	{
		if ((bool)m_PageAnimator)
		{
			m_PageAnimator.AppearAnimation();
		}
	}

	private void Hide()
	{
		if ((bool)m_PageAnimator)
		{
			m_PageAnimator.DisappearAnimation();
		}
	}

	public abstract void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, BoolReactiveProperty isMainCharacter);

	public IReadOnlyReactiveProperty<bool> GetCanGoNextOnConfirmProperty()
	{
		return CanGoNextOnConfirm;
	}

	public virtual IReadOnlyReactiveProperty<bool> CanGoNextInMenuProperty()
	{
		return CanGoNextOnConfirm;
	}

	public IReadOnlyReactiveProperty<bool> GetCanGoBackOnDeclineProperty()
	{
		return CanGoBackOnDecline;
	}

	public virtual bool PressConfirmOnPhase()
	{
		return base.ViewModel.IsCompletedAndAvailable.Value;
	}

	public virtual bool PressDeclineOnPhase()
	{
		return true;
	}

	void ICharGenPhaseDetailedView.Unbind()
	{
		Unbind();
	}
}
