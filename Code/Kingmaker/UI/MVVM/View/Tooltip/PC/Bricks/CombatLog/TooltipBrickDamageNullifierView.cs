using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog.Additional;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;

public class TooltipBrickDamageNullifierView : TooltipBaseBrickView<TooltipBrickDamageNullifierVM>
{
	[SerializeField]
	private TextMeshProUGUI m_HeaderText;

	[SerializeField]
	private RollSlider m_RollSlider;

	[SerializeField]
	private TextMeshProUGUI m_ResultValueText;

	[SerializeField]
	private TextMeshProUGUI m_ReasonsText;

	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private ReasonBuffItemView m_ReasonBuffItemView;

	[SerializeField]
	private TextMeshProUGUI m_ResultText;

	protected override void BindViewImplementation()
	{
		m_HeaderText.text = GameLogStrings.Instance.TooltipBrickStrings.IncomingDamageNullifier;
		m_RollSlider.SetData(base.ViewModel.ChanceRoll, base.ViewModel.ResultRoll);
		TextMeshProUGUI resultValueText = m_ResultValueText;
		int resultValue = base.ViewModel.ResultValue;
		resultValueText.text = "=" + resultValue;
		m_ReasonsText.text = base.ViewModel.ReasonText;
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.ReasonBuffItems, m_ReasonBuffItemView));
		m_ResultText.text = base.ViewModel.ResultText;
	}
}
