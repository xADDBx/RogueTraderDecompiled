using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;

namespace Kingmaker.UI.MVVM.VM.CombatLog;

public class CombatLogSeparatorVM : CombatLogBaseVM
{
	public CombatLogSeparatorVM(CombatLogMessage message)
		: base(message)
	{
	}

	protected override void DisposeImplementation()
	{
	}
}
