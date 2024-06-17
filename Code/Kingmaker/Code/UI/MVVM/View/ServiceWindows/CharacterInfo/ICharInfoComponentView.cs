using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;

public interface ICharInfoComponentView
{
	bool IsBinded { get; }

	void BindSection(CharInfoComponentVM vm);

	void UnbindSection();
}
