using Kingmaker.Blueprints;
using Kingmaker.UI.Common.DebugInformation;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.PC;

public class OvertipUnitPCView : OvertipUnitView, IHasBlueprintInfo
{
	public BlueprintScriptableObject Blueprint => base.ViewModel.Unit.Blueprint;
}
