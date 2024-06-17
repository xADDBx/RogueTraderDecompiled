using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips;

public class SurfaceOvertipsVM : OvertipsVM
{
	public UnitOvertipsCollectionVM UnitOvertipsCollectionVM;

	public LightweightUnitOvertipsCollectionVM LightweightUnitOvertipsCollectionVM;

	public MapObjectOvertipsVM MapObjectOvertipsVM;

	public SurfaceOvertipsVM()
	{
		AddDisposable(UnitOvertipsCollectionVM = new UnitOvertipsCollectionVM());
		AddDisposable(LightweightUnitOvertipsCollectionVM = new LightweightUnitOvertipsCollectionVM());
		AddDisposable(MapObjectOvertipsVM = new MapObjectOvertipsVM());
	}

	public override void HandleOnShowBark(string text)
	{
		Entity entity = EventInvokerExtensions.Entity;
		if (!(entity is LightweightUnitEntity entity2))
		{
			if (entity is AbstractUnitEntity entity3)
			{
				UnitOvertipsCollectionVM.ShowBark(entity3, text);
			}
			else
			{
				MapObjectOvertipsVM.ShowBark(entity, text);
			}
		}
		else
		{
			LightweightUnitOvertipsCollectionVM.ShowBark(entity2, text);
		}
	}

	public override void HandleOnShowLinkedBark(string text, string encyclopediaLink)
	{
	}

	public override void HandleOnHideBark()
	{
		Entity entity = EventInvokerExtensions.Entity;
		if (!(entity is LightweightUnitEntity entity2))
		{
			if (entity is AbstractUnitEntity entity3)
			{
				UnitOvertipsCollectionVM.HideBark(entity3);
			}
			else
			{
				MapObjectOvertipsVM.HideBark(entity);
			}
		}
		else
		{
			LightweightUnitOvertipsCollectionVM.HideBark(entity2);
		}
	}
}
