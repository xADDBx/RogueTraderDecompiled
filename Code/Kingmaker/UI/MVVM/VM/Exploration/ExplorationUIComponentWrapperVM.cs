using UniRx;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public abstract class ExplorationUIComponentWrapperVM : ExplorationComponentBaseVM, IExplorationUIComponentWrapper
{
	public readonly ReactiveProperty<bool> ActiveOnScreen = new ReactiveProperty<bool>(initialValue: false);

	protected abstract ExplorationUISection ExplorationUISection { get; }

	public void SetActiveOnScreen(ExplorationUISection explorationUISection)
	{
		ActiveOnScreen.Value = (explorationUISection & ExplorationUISection) > ExplorationUISection.None;
	}
}
