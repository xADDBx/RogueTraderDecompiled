using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog.Additional;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.Utility.DotNetExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;

public class TooltipBrickTriggeredAutoView : TooltipBaseBrickView<TooltipBrickTriggeredAutoVM>
{
	[SerializeField]
	private TextMeshProUGUI m_TriggeredAutoText;

	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private ReasonBuffItemView m_ReasonBuffItemView;

	[SerializeField]
	private Image m_ResultSignImage;

	[Header("Sprites")]
	[SerializeField]
	private Sprite m_ResultSignSuccessSprite;

	[SerializeField]
	private Sprite m_ResultSignFailedSprite;

	[Header("Colors")]
	[SerializeField]
	private Color m_OrangeColor;

	[SerializeField]
	private Color m_LightColor;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		m_TextHelper = new AccessibilityTextHelper(m_TriggeredAutoText);
		m_TriggeredAutoText.text = base.ViewModel.TriggeredAutoText;
		if (base.ViewModel.ReasonBuffItems.AnyItem())
		{
			AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.ReasonBuffItems, m_ReasonBuffItemView));
		}
		m_ResultSignImage.sprite = (base.ViewModel.IsSuccess ? m_ResultSignSuccessSprite : m_ResultSignFailedSprite);
		m_ResultSignImage.color = (base.ViewModel.IsSuccess ? m_OrangeColor : m_LightColor);
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}
}
