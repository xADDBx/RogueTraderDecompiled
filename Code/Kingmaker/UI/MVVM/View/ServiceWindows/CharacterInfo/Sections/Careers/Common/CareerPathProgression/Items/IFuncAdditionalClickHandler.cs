using Owlcat.Runtime.UI.ConsoleTools;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

public interface IFuncAdditionalClickHandler : IConsoleEntity
{
	bool CanFuncAdditionalClick();

	void OnFuncAdditionalClick();

	string GetFuncAdditionalClickHint();
}
