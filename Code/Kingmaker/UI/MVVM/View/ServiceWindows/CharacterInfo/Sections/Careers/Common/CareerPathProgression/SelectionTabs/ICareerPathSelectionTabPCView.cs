using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.SelectionTabs;

public interface ICareerPathSelectionTabPCView : ICareerPathSelectionTabView
{
	bool CanCommit { get; }

	void SetButtonsBlock(CareerButtonsBlock buttonsBlock);
}
