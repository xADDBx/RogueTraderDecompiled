using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;

namespace Kingmaker.Code.UI.MVVM.View.SelectorWindow;

public class AbilitySelectorWindowConsoleView : SelectorWindowConsoleView<CharInfoFeatureConsoleView, CharInfoFeatureVM>
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Header.text = UIStrings.Instance.CharacterSheet.ChooseActiveAbilityFeatureGroupHint.Text;
	}
}
