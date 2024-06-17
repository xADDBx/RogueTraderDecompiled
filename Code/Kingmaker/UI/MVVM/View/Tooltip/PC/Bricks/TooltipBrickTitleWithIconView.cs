using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickTitleWithIconView : TooltipBaseBrickView<TooltipBrickTitleWithIconVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private Image m_Icon;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Label);
		}
		base.BindViewImplementation();
		m_Label.text = base.ViewModel.Name;
		m_Icon.sprite = base.ViewModel.Icon;
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig
		{
			PriorityPivots = new List<Vector2>
			{
				new Vector2(0f, 0.7f),
				new Vector2(0f, 0.5f)
			}
		}));
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}
}
