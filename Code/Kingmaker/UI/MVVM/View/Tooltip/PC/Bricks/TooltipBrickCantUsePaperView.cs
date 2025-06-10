using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickCantUsePaperView : TooltipBaseBrickView<TooltipBrickCantUsePaperVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	[SerializeField]
	protected Image m_AbilityIcon;

	[SerializeField]
	protected TextMeshProUGUI m_AbilityName;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Text.text = base.ViewModel.CantUseTitle;
		m_AbilityName.text = base.ViewModel.AbilityName;
		m_AbilityIcon.sprite = base.ViewModel.Icon;
	}
}
