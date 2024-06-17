using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.CombatLog;

public class CombatLogBaseVM : VirtualListElementVMBase
{
	public readonly CombatLogMessage Message;

	public CombatLogBaseVM(CombatLogMessage message)
	{
		Message = message;
	}

	protected override void DisposeImplementation()
	{
	}
}
