using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ContextMenu;

public class ContextMenuVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly List<ContextMenuEntityVM> Entities = new List<ContextMenuEntityVM>();

	public RectTransform Owner;

	public ContextMenuVM(ContextMenuCollection collection)
	{
		foreach (ContextMenuCollectionEntity item in collection)
		{
			if (item.IsValid)
			{
				ContextMenuEntityVM contextMenuEntityVM = new ContextMenuEntityVM(item);
				AddDisposable(contextMenuEntityVM);
				Entities.Add(contextMenuEntityVM);
			}
		}
		Owner = collection.Owner;
	}

	protected override void DisposeImplementation()
	{
	}
}
