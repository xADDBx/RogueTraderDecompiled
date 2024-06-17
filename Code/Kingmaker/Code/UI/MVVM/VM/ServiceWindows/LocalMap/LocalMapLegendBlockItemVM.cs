using System;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap;

public class LocalMapLegendBlockItemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly Sprite ItemSprite;

	public readonly string ItemLabel;

	public LocalMapLegendBlockItemVM(Sprite itemSprite, string itemLabel)
	{
		ItemSprite = itemSprite;
		ItemLabel = itemLabel;
	}

	protected override void DisposeImplementation()
	{
	}
}
