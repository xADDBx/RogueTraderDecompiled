using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

public interface IHasNeighbours
{
	void SetNeighbours(List<IFloatConsoleNavigationEntity> entities);
}
