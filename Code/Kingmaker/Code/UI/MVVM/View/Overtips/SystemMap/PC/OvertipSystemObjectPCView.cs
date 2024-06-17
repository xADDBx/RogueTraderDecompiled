using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap.PC;

public class OvertipSystemObjectPCView : OvertipSystemObjectView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		BlueprintMechanicEntityFact blueprint = base.ViewModel.SystemMapObject.View.Data.Blueprint;
		if (!blueprint.Name.IsNullOrEmpty())
		{
			AddDisposable(m_SystemObjectButton.SetHint(blueprint.Name));
		}
	}
}
