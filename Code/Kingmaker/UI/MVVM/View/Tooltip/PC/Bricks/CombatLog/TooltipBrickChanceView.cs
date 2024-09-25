using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog.Additional;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;

public class TooltipBrickChanceView : TooltipBrickCombatLogBaseView<TooltipBrickChanceVM>
{
	[Header("Slider")]
	[SerializeField]
	private RollSlider m_RollSlider;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_RollSlider.SetData(base.ViewModel.SufficientValue, base.ViewModel.CurrentValue);
	}
}
