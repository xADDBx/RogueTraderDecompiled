using Kingmaker.UI.MVVM.View.Colonization.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyTraitsPCView : ColonyTraitsBaseView
{
	[SerializeField]
	private ColonyTraitPCView m_ColonyTraitPCView;

	protected override void DrawEntitiesImpl()
	{
		m_WidgetListTraits.DrawEntries(base.ViewModel.TraitsVMs, m_ColonyTraitPCView);
	}
}
