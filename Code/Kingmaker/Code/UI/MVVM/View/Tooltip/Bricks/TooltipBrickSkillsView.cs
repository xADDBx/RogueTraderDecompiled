using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickSkillsView : TooltipBaseBrickView<TooltipBrickSkillsVM>
{
	[SerializeField]
	protected CharInfoSkillsBlockCommonView m_AbilityScoresBlockView;

	protected override void BindViewImplementation()
	{
		m_AbilityScoresBlockView.Bind(base.ViewModel.AbilityScoresBlock);
	}
}
