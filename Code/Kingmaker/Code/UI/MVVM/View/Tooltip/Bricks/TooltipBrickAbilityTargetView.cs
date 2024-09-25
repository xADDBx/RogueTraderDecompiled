using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickAbilityTargetView : TooltipBaseBrickView<TooltipBrickAbilityTargetVM>
{
	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	protected TextMeshProUGUI m_Title;

	[SerializeField]
	protected TextMeshProUGUI m_Text;

	[SerializeField]
	private LayoutElement m_IconContainer;

	[SerializeField]
	private int m_BigSize = 64;

	[SerializeField]
	private int m_MediumSize = 48;

	[SerializeField]
	private int m_SmallSize = 30;

	[SerializeField]
	private float m_DefaultFontSizeTitle = 18f;

	[SerializeField]
	private float m_DefaultFontSizeText = 22f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeTitle = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeText = 22f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.text = base.ViewModel.Label;
		m_Text.text = base.ViewModel.Text;
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_Title.fontSize = (isControllerMouse ? m_DefaultFontSizeTitle : m_DefaultConsoleFontSizeTitle) * FontMultiplier;
		m_Text.fontSize = (isControllerMouse ? m_DefaultFontSizeText : m_DefaultConsoleFontSizeText) * FontMultiplier;
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
		if (base.ViewModel.Tooltip != null)
		{
			AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig
			{
				PriorityPivots = new List<Vector2>
				{
					new Vector2(1f, 0.5f),
					new Vector2(0f, 0.5f)
				}
			}));
		}
	}
}
