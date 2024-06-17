using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap;

public class LocalMapLegendBlockVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly AutoDisposingList<LocalMapLegendBlockItemVM> LocalMapItemsVMs = new AutoDisposingList<LocalMapLegendBlockItemVM>();

	public LocalMapLegendBlockVM()
	{
		AddItems();
	}

	private void AddItems()
	{
		LocalMapItemsVMs.Clear();
		List<LocalMapLegendBlockItemInfo> localMapLegendBlockItemInfo = UIConfig.Instance.BlueprintUILocalMapLegend.LocalMapLegendBlockItemInfo;
		if (localMapLegendBlockItemInfo.Any())
		{
			localMapLegendBlockItemInfo.ForEach(delegate(LocalMapLegendBlockItemInfo block)
			{
				LocalMapItemsVMs.Add(new LocalMapLegendBlockItemVM(block.Sprite.Load(), block.Description));
			});
		}
	}

	protected override void DisposeImplementation()
	{
	}
}
