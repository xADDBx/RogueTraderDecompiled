using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickAbilityScoresConsoleView : TooltipBrickAbilityScoresView, IConsoleTooltipBrick, IMonoBehaviour
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public MonoBehaviour MonoBehaviour => (MonoBehaviour)m_NavigationBehaviour.DeepestNestedFocus;

	protected override void BindViewImplementation()
	{
		m_AbilityScoresBlockView.Bind(base.ViewModel.AbilityScoresBlock);
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
	}

	public IConsoleEntity GetConsoleEntity()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddRow((m_AbilityScoresBlockView as CharInfoAbilityScoresBlockConsoleView)?.AbilityScores);
		return m_NavigationBehaviour;
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
