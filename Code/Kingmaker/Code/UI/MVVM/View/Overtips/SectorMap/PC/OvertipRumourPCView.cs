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
		BlueprintQuest quest = base.ViewModel.SectorMapRumour.View.Blueprint.Quest;
		m_InfoButton.SetHint(quest.Title, quest.Description);
	}
}
