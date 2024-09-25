using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Controls.Selectable;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickItemHeaderView : TooltipBaseBrickView<TooltipBrickItemHeaderVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	[Tooltip("Has one of states of ItemHeaderType enum: Default, Header, Equipped, CanNotEquip")]
	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text);
		}
		base.BindViewImplementation();
		m_Text.text = base.ViewModel.Text;
		m_Selectable.SetActiveLayer(base.ViewModel.Type.ToString());
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}
}
