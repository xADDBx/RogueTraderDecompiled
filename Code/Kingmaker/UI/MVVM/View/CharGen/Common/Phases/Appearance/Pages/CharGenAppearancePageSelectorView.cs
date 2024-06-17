using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.VirtualListSystem;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Pages;

public class CharGenAppearancePageSelectorView : SelectionGroupRadioView<CharGenAppearancePageVM, CharGenAppearancePageMenuItemView>
{
	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return VirtualList.GetNavigationBehaviour().Entities.Select((IConsoleEntity e) => e as IConsoleNavigationEntity).ToList();
	}

	public IConsoleNavigationEntity GetSelectedEntity()
	{
		int index = VirtualList.GetNavigationBehaviour().Entities.Select((IConsoleEntity e) => (e as VirtualListElement)?.Data).FindIndex((IVirtualListElementData i) => (i as CharGenAppearancePageVM)?.IsSelected.Value ?? false);
		return VirtualList.GetNavigationBehaviour().Entities.ElementAt(index) as IConsoleNavigationEntity;
	}
}
