using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickFeatureShortDescriptionView : TooltipBaseBrickView<TooltipBrickFeatureDescriptionVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_FrameIcon;

	[SerializeField]
	private Sprite m_DefaultFrame;

	[SerializeField]
	private TextMeshProUGUI m_Acronym;

	[SerializeField]
	private float m_DefaultFontSizeLabel = 24f;

	[SerializeField]
	private float m_DefaultFontSizeDescription = 20f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeLabel = 24f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeDescription = 20f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Label.text = base.ViewModel.Name;
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.color = base.ViewModel.IconColor;
		m_Acronym.text = base.ViewModel.Acronym;
		m_Description.text = base.ViewModel.Description;
		m_FrameIcon.sprite = ((base.ViewModel.SpecialFrame != null) ? base.ViewModel.SpecialFrame : m_DefaultFrame);
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig
		{
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f),
				new Vector2(0f, 0.5f)
			}
		}));
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_Label.fontSize = (isControllerMouse ? m_DefaultFontSizeLabel : m_DefaultConsoleFontSizeLabel) * FontMultiplier;
		m_Description.fontSize = (isControllerMouse ? m_DefaultFontSizeDescription : m_DefaultConsoleFontSizeDescription) * FontMultiplier;
	}
}
