using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.View.SelectorWindow;

public class AbilitySelectorWindowConsoleView : SelectorWindowConsoleView<FeatureSelectorSlotConsoleView, FeatureSelectorSlotVM>
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Header.text = UIStrings.Instance.CharacterSheet.ChooseActiveAbilityFeatureGroupHint.Text;
	}

	protected override void EntityFocused(IConsoleEntity entity)
	{
		base.EntityFocused(entity);
		if (entity != null)
		{
			FeatureSelectorSlotVM obj = (entity as IHasViewModel)?.GetViewModel() as FeatureSelectorSlotVM;
			(base.ViewModel as AbilitySelectorWindowVM)?.OnFeatureFocused?.Invoke(obj);
		}
	}
}
