using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.Tooltip;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickWidgetView : TooltipBaseBrickView<TooltipBrickWidgetVM>
{
	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[Header("Bricks")]
	[SerializeField]
	private TooltipBricksView m_BricksConfig;

	[SerializeField]
	private TooltipBrickTextView m_TooltipBrickTextView;

	protected override void BindViewImplementation()
	{
		if (base.ViewModel.TooltipBrickTextVM != null)
		{
			m_TooltipBrickTextView.Bind(base.ViewModel.TooltipBrickTextVM);
		}
		AddDisposable(base.ViewModel.Bricks.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}));
		DrawEntities();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_WidgetList.Clear();
	}

	private void DrawEntities()
	{
		m_WidgetList.Clear();
		List<TooltipBaseBrickVM> list = base.ViewModel.CollectBricksVM();
		bool flag = list.Any();
		m_TooltipBrickTextView.gameObject.SetActive(!flag);
		if (flag)
		{
			TooltipBaseBrickVM vm = list.FirstOrDefault();
			MonoBehaviour brickView = TooltipEngine.GetBrickView(m_BricksConfig, vm);
			if (!(brickView is IWidgetView entryPrefab))
			{
				UberDebug.LogError("Error: Brick " + brickView.name + " is not IWidgetView");
				TooltipEngine.DestroyBrickView(brickView);
			}
			else
			{
				m_WidgetList.DrawEntries(list, entryPrefab);
				TooltipEngine.DestroyBrickView(brickView);
			}
		}
	}
}
