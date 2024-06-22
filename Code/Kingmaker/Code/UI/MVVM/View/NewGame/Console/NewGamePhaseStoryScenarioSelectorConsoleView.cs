using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Console;

public class NewGamePhaseStoryScenarioSelectorConsoleView : NewGamePhaseStoryScenarioSelectorBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private NewGamePhaseStoryScenarioEntityConsoleView m_ItemPrefab;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab));
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		if (m_WidgetList.Entries == null)
		{
			return list;
		}
		foreach (NewGamePhaseStoryScenarioEntityConsoleView item in m_WidgetList.Entries.OfType<NewGamePhaseStoryScenarioEntityConsoleView>())
		{
			list.Add(item);
			list.AddRange(item.GetNavigationEntities());
		}
		return list;
	}
}
