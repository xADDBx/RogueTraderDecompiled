using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores;

public class CharInfoLevelClassConsoleView : CharInfoLevelClassScoresPCView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		(m_AdditionalStatsView as ICharInfoComponentConsoleView)?.AddInput(ref inputLayer, ref navigationBehaviour, hintsWidget);
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
