using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.SelectionTabs;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression.SelectionTabs;

public abstract class BaseCareerPathSelectionTabConsoleView<TViewModel> : BaseCareerPathSelectionTabCommonView<TViewModel>, ICareerPathSelectionTabConsoleView, ICareerPathSelectionTabView where TViewModel : class, IViewModel
{
	protected readonly ReactiveProperty<bool> ButtonActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ButtonVisible = new ReactiveProperty<bool>();

	private ConsoleHintDescription m_HintDescription;

	protected bool InputAdded;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(NextButtonLabel.Subscribe(delegate(string value)
		{
			m_HintDescription?.SetLabel(value);
		}));
		AddDisposable(ButtonActive.And(IsTabActiveProp).Subscribe(delegate(bool value)
		{
			m_ButtonVisible.Value = value;
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		InputAdded = false;
	}

	public virtual void ScrollMainTab(float value)
	{
	}

	public virtual void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
	}

	void ICareerPathSelectionTabView.Unbind()
	{
		Unbind();
	}
}
