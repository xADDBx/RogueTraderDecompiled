using Kingmaker.UI.MVVM.View.Colonization.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Console;

public class ColonyTraitsConsoleView : ColonyTraitsBaseView
{
	[SerializeField]
	private ColonyTraitConsoleView m_ColonyTraitConsoleView;

	protected override void DrawEntitiesImpl()
	{
		m_WidgetListTraits.DrawEntries(base.ViewModel.TraitsVMs, m_ColonyTraitConsoleView);
	}
}
