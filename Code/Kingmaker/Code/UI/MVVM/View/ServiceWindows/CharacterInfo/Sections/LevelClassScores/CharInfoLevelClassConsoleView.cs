using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores;

public class CharInfoLevelClassConsoleView : CharInfoLevelClassScoresPCView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		if (m_AdditionalStatsView is InventoryDollAdditionalStatsConsoleView inventoryDollAdditionalStatsConsoleView)
		{
			inventoryDollAdditionalStatsConsoleView.AddInput(ref inputLayer, ref navigationBehaviour, hintsWidget);
		}
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		if (m_AdditionalStatsView is InventoryDollAdditionalStatsConsoleView inventoryDollAdditionalStatsConsoleView)
		{
			return inventoryDollAdditionalStatsConsoleView.GetNavigation();
		}
		return null;
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
