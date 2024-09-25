using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Exploration;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationResourceListBaseView<TExplorationResourceView> : ViewBase<ExplorationResourceListVM> where TExplorationResourceView : ExplorationResourceBaseView
{
	[SerializeField]
	private WidgetListMVVM m_WidgetListResources;

	[SerializeField]
	private TExplorationResourceView m_PlanetResourcePrefab;

	[SerializeField]
	private TextMeshProUGUI m_PlanetResourcesLabel;

	protected override void BindViewImplementation()
	{
		DrawEntities();
		AddDisposable(base.ViewModel.UpdateResources.Subscribe(DrawEntities));
	}

	private void DrawEntities()
	{
		m_WidgetListResources.DrawEntries(base.ViewModel.CurrentResourcesVMs, m_PlanetResourcePrefab);
		m_PlanetResourcesLabel.text = (base.ViewModel.CurrentResourcesVMs.Empty() ? UIStrings.Instance.ExplorationTexts.ExploObjectResourcesEmpty.Text : UIStrings.Instance.ExplorationTexts.ExploObjectResources.Text);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		if (!base.ViewModel.HasColony)
		{
			return m_WidgetListResources.GetNavigationEntities();
		}
		return null;
	}
}
