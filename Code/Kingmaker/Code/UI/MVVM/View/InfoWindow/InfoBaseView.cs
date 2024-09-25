using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip;
using Kingmaker.Code.UI.MVVM.VM.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.InfoWindow;

public abstract class InfoBaseView<TInfoBaseVM> : ViewBase<TInfoBaseVM> where TInfoBaseVM : InfoBaseVM
{
	[Header("Containers")]
	[SerializeField]
	protected RectTransform m_HeaderContainer;

	[SerializeField]
	protected RectTransform m_BodyContainer;

	[SerializeField]
	protected RectTransform m_FooterContainer;

	[SerializeField]
	protected RectTransform m_HintContainer;

	[Header("Bricks")]
	[SerializeField]
	private TooltipBricksView m_BricksConfig;

	protected readonly List<MonoBehaviour> Bricks = new List<MonoBehaviour>();

	private readonly List<MonoBehaviour> m_BricksGroups = new List<MonoBehaviour>();

	private readonly List<IConsoleTooltipBrick> m_NavigationBricks = new List<IConsoleTooltipBrick>();

	protected override void BindViewImplementation()
	{
		SetPart(base.ViewModel.HeaderBricks, m_HeaderContainer);
		SetPart(base.ViewModel.BodyBricks, m_BodyContainer);
		SetPart(base.ViewModel.FooterBricks, m_FooterContainer);
		SetPart(base.ViewModel.HintBricks, m_HintContainer);
	}

	private void SetPart(IEnumerable<TooltipBaseBrickVM> bricks, RectTransform container)
	{
		container.Or(null)?.gameObject.SetActive(bricks.Any());
		TooltipBricksGroupView tooltipBricksGroupView = null;
		foreach (TooltipBaseBrickVM brick in bricks)
		{
			MonoBehaviour brickView = TooltipEngine.GetBrickView(m_BricksConfig, brick);
			IConsoleTooltipBrick consoleTooltipBrick = brickView as IConsoleTooltipBrick;
			if (brick is TooltipBricksGroupVM)
			{
				if (brickView != null)
				{
					tooltipBricksGroupView = (TooltipBricksGroupView)brickView;
					tooltipBricksGroupView.transform.SetParent(container, worldPositionStays: false);
					m_BricksGroups.Add(tooltipBricksGroupView);
					m_NavigationBricks.Add(tooltipBricksGroupView);
				}
				else
				{
					tooltipBricksGroupView = null;
				}
				continue;
			}
			if (tooltipBricksGroupView != null)
			{
				tooltipBricksGroupView.AddChild(brickView.transform as RectTransform);
				if (consoleTooltipBrick != null)
				{
					tooltipBricksGroupView.AddNavChild(consoleTooltipBrick.GetConsoleEntity());
				}
			}
			else
			{
				brickView.transform.SetParent(container, worldPositionStays: false);
				if (consoleTooltipBrick != null)
				{
					m_NavigationBricks.Add(consoleTooltipBrick);
				}
			}
			Bricks.Add(brickView);
		}
	}

	protected override void DestroyViewImplementation()
	{
		Bricks.ForEach(TooltipEngine.DestroyBrickView);
		Bricks.Clear();
		m_BricksGroups.ForEach(TooltipEngine.DestroyBrickView);
		m_BricksGroups.Clear();
		m_NavigationBricks.Clear();
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour(IConsoleNavigationOwner owner = null)
	{
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour(owner);
		gridConsoleNavigationBehaviour.AddColumn(m_NavigationBricks.Select((IConsoleTooltipBrick i) => i.GetConsoleEntity()).ToList());
		return gridConsoleNavigationBehaviour;
	}
}
