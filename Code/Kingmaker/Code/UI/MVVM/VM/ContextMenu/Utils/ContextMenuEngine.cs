using Kingmaker.Code.UI.MVVM.View.ContextMenu;
using Kingmaker.Code.UI.MVVM.View.ContextMenu.Common;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;

public static class ContextMenuEngine
{
	public static MonoBehaviour GetEntityView(ContextMenuEntitiesView entitiesView, ContextMenuEntityVM entityVM)
	{
		if (entityVM.IsHeader)
		{
			ContextMenuHeaderView widget = WidgetFactory.GetWidget(entitiesView.ContextMenuHeaderView);
			widget.Bind(entityVM);
			return widget;
		}
		if (!entityVM.IsSeparator)
		{
			ContextMenuEntityView widget2 = WidgetFactory.GetWidget(entitiesView.ContextMenuEntityView);
			widget2.Bind(entityVM);
			return widget2;
		}
		ContextMenuSeparatorView widget3 = WidgetFactory.GetWidget(entitiesView.ContextMenuSeparatorView);
		widget3.Bind(entityVM);
		return widget3;
	}
}
