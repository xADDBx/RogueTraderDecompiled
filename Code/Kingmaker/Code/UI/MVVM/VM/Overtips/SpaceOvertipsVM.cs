using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips;

public class SpaceOvertipsVM : OvertipsVM
{
	public readonly SystemMapOvertipsVM SystemMapOvertipsVM;

	public readonly SectorMapOvertipsVM SectorMapOvertipsVM;

	public readonly UnitOvertipsCollectionVM UnitOvertipsCollectionVM;

	public SpaceOvertipsVM()
	{
		AddDisposable(SystemMapOvertipsVM = new SystemMapOvertipsVM());
		AddDisposable(SectorMapOvertipsVM = new SectorMapOvertipsVM());
		AddDisposable(UnitOvertipsCollectionVM = new UnitOvertipsCollectionVM());
	}

	protected override void DisposeImplementation()
	{
	}

	public override void HandleOnShowBark(string text)
	{
		if (EventInvokerExtensions.Entity is BaseUnitEntity entity)
		{
			UnitOvertipsCollectionVM.ShowBark(entity, text);
		}
	}

	public override void HandleOnShowLinkedBark(string text, string encyclopediaLink)
	{
	}

	public override void HandleOnHideBark()
	{
		if (EventInvokerExtensions.Entity is BaseUnitEntity entity)
		{
			UnitOvertipsCollectionVM.HideBark(entity);
		}
	}
}
