using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Console;

public class NewGamePhaseStoryScenarioEntityConsoleView : NewGamePhaseStoryScenarioEntityBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private NewGamePhaseStoryScenarioEntityIntegralDlcConsoleView m_ItemPrefab;

	protected override void DrawEntitiesImpl()
	{
		base.DrawEntitiesImpl();
		m_WidgetList.DrawEntries(base.ViewModel.IntegralDlcVms, m_ItemPrefab);
	}

	protected override void OnChangeSelectedStateImpl()
	{
		base.OnChangeSelectedStateImpl();
		base.ViewModel.SelectMe();
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		if (m_WidgetList.Entries == null)
		{
			return list;
		}
		list.AddRange(m_WidgetList.Entries.OfType<NewGamePhaseStoryScenarioEntityIntegralDlcConsoleView>());
		return list;
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		base.ViewModel.SetSelected(value);
	}
}
