using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class VeilThicknessLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventVeilChanged>
{
	public void HandleEvent(GameLogEventVeilChanged evt)
	{
		TooltipTemplateGlossary template = new TooltipTemplateGlossary("VeilThickness");
		if (evt.Delta != evt.NewValue)
		{
			GameLogContext.VeilThicknessDelta = evt.Delta;
			GameLogContext.VeilThicknessValue = evt.NewValue;
			AddMessage(new CombatLogMessage(LogThreadBase.Strings.VeilThicknessValueChanged.CreateCombatLogMessage(), template));
		}
	}
}
