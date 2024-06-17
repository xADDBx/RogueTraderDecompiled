using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.FactionsReputation;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.FactionsReputation;

public class CharInfoFactionsReputationPCView : CharInfoComponentView<CharInfoFactionsReputationVM>
{
	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharInfoFactionReputationItemPCView m_FactionReputationItemPCView;

	[SerializeField]
	private CharInfoProfitFactorItemBaseView ProfitFactorItemBaseView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		DrawEntities();
	}

	protected override void RefreshView()
	{
	}

	private void DrawEntities()
	{
		m_WidgetList.Clear();
		m_WidgetList.DrawMultiEntries(base.ViewModel.ScreenItems.ToArray(), new List<IWidgetView> { m_FactionReputationItemPCView, ProfitFactorItemBaseView });
	}
}
