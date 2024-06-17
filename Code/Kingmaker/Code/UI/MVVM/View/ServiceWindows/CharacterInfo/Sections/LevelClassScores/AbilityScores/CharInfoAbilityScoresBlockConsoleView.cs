using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;

public class CharInfoAbilityScoresBlockConsoleView : CharInfoAbilityScoresBlockBaseView
{
	public List<CharInfoAbilityScorePCView> AbilityScores => m_StatEntries;

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		gridConsoleNavigationBehaviour.AddColumn(AbilityScores);
		return gridConsoleNavigationBehaviour;
	}
}
