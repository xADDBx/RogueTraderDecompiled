using System.Collections.Generic;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Colonization.Events;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization;

public class ColonyEventNotificatorListPCView : ViewBase<ColonyEventsVM>
{
	[SerializeField]
	private ColonyEventNotificatorPCView m_ColonyEventNotificatorPCView;

	[SerializeField]
	private WidgetListMVVM m_WidgetListNotificators;

	protected override void BindViewImplementation()
	{
		DrawEntities();
		AddDisposable(base.ViewModel.UpdateEventsCommand.Subscribe(DrawEntities));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void DrawEntities()
	{
		ColonyEventVM colonyEventVM = base.ViewModel.EventsVMs.FirstOrDefault();
		if (colonyEventVM == null)
		{
			m_WidgetListNotificators.Clear();
			return;
		}
		List<ColonyEventVM> vmCollection = new List<ColonyEventVM> { colonyEventVM };
		m_WidgetListNotificators.DrawEntries(vmCollection, m_ColonyEventNotificatorPCView);
	}
}
