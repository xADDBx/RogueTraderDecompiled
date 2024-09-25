using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;

public class TooltipBrickIconTextView : TooltipBaseBrickView<TooltipBrickIconTextVM>
{
	[SerializeField]
	private Image m_IconImage;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	protected override void BindViewImplementation()
	{
		m_IconImage.gameObject.SetActive(base.ViewModel.IsShowIcon);
		m_IconImage.color = Color.black;
		m_Text.text = base.ViewModel.Text;
	}

	protected override void DestroyViewImplementation()
	{
	}
}
