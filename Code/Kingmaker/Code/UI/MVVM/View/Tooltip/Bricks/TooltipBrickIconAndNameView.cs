using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickIconAndNameView : TooltipBaseBrickView<TooltipBrickIconAndNameVM>
{
	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	protected TextMeshProUGUI m_Title;

	[SerializeField]
	private LayoutElement m_IconContainer;

	[SerializeField]
	private GameObject m_Frame;

	[SerializeField]
	private int m_BigSize = 64;

	[SerializeField]
	private int m_MediumSize = 48;

	[SerializeField]
	private int m_SmallSize = 30;

	[SerializeField]
	private float m_DefaultFontSize = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 18f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.text = base.ViewModel.Line;
		int num = 0;
		switch (base.ViewModel.Type)
		{
		case TooltipBrickElementType.Big:
			num = m_BigSize;
			break;
		case TooltipBrickElementType.Medium:
			num = m_MediumSize;
			break;
		case TooltipBrickElementType.Small:
			num = m_SmallSize;
			break;
		}
		LayoutElement iconContainer = m_IconContainer;
		LayoutElement iconContainer2 = m_IconContainer;
		LayoutElement iconContainer3 = m_IconContainer;
		float num3 = (m_IconContainer.preferredWidth = num);
		float num5 = (iconContainer3.preferredHeight = num3);
		float minHeight = (iconContainer2.minWidth = num5);
		iconContainer.minHeight = minHeight;
		m_Frame.SetActive(base.ViewModel.Frame);
		if (base.ViewModel.Tooltip != null)
		{
			AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
		}
		m_Title.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
	}
}
