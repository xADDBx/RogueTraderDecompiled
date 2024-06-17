namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.SelectionTabs;

public interface ICareerPathSelectionTabView
{
	void Initialize();

	bool IsTabActive();

	void UpdateState();

	void Unbind();
}
