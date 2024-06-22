using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap.PC;

public class OvertipRumourPCView : OvertipRumourView
{
	[SerializeField]
	private OwlcatButton m_InfoButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		string text = string.Empty;
		if (base.ViewModel.SectorMapRumour != null && !base.ViewModel.SectorMapRumour.View.HasParent)
		{
			text = base.ViewModel.SectorMapRumour.View.Blueprint.GetTitile();
		}
		else if (base.ViewModel.SectorMapRumourGroup != null)
		{
			List<string> values = (from b in base.ViewModel.SectorMapRumourGroup.View.ActiveQuestObjectives
				select b.GetTitile().Text into b
				where !string.IsNullOrWhiteSpace(b)
				select b).ToList();
			text = string.Join(Environment.NewLine, values);
		}
		AddDisposable(m_InfoButton.SetHint(text));
	}
}
