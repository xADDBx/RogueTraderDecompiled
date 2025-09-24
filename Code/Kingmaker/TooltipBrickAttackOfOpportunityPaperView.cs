using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using TMPro;
using UnityEngine;

namespace Kingmaker;

public class TooltipBrickAttackOfOpportunityPaperView : TooltipBaseBrickView<TooltipBrickAttackOfOpportunityPaperVM>
{
	[SerializeField]
	public TextMeshProUGUI m_Description;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Description.text = base.ViewModel.Description;
	}
}
