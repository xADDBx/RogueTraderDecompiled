using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap;

public abstract class OvertipRumourView : BaseOvertipView<OvertipEntityRumourVM>
{
	protected override bool CheckVisibility => base.ViewModel.IsVisible.Value;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		string text = "Empty_Name_Check_It";
		if (base.ViewModel.SectorMapRumour != null && !base.ViewModel.SectorMapRumour.View.HasParent)
		{
			text = base.ViewModel.SectorMapRumour.View.name + "_OvertipRumourView";
		}
		else if (base.ViewModel.SectorMapRumourGroup != null)
		{
			List<string> values = (from b in base.ViewModel.SectorMapRumourGroup.View.ActiveQuestObjectives
				select b.Quest.name.Replace("_rumour", "").Replace("_rumor", "") into b
				where !string.IsNullOrWhiteSpace(b)
				select b).ToList();
			text = string.Join("_", values) + "_OvertipGroupRumourView";
		}
		base.name = text;
	}
}
