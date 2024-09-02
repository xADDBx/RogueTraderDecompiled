using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;
using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject.Collections;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips;

public class SurfaceOvertipsVM : OvertipsVM
{
	public UnitOvertipsCollectionVM UnitOvertipsCollectionVM;

	public LightweightUnitOvertipsCollectionVM LightweightUnitOvertipsCollectionVM;

	public MapObjectOvertipsVM MapObjectOvertipsVM;

	public LocatorOvertipsCollectionVM LocatorOvertipsCollectionVM;

	public SurfaceOvertipsVM()
	{
		AddDisposable(UnitOvertipsCollectionVM = new UnitOvertipsCollectionVM());
		AddDisposable(LightweightUnitOvertipsCollectionVM = new LightweightUnitOvertipsCollectionVM());
		AddDisposable(MapObjectOvertipsVM = new MapObjectOvertipsVM());
		AddDisposable(LocatorOvertipsCollectionVM = new LocatorOvertipsCollectionVM());
	}

	public override void HandleOnShowBark(string text)
	{
		ShowBark(EventInvokerExtensions.Entity, text);
	}

	public override void HandleOnShowBarkWithName(string text, string name, Color nameColor)
	{
		ShowBark(EventInvokerExtensions.Entity, text);
	}

	public override void HandleOnShowLinkedBark(string text, string encyclopediaLink)
	{
	}

	public override void HandleOnHideBark()
	{
		Entity entity = EventInvokerExtensions.Entity;
		if (!(entity is LightweightUnitEntity entity2))
		{
			if (!(entity is AbstractUnitEntity entity3))
			{
				if (entity is LocatorEntity entity4)
				{
					LocatorOvertipsCollectionVM.HideBark(entity4);
				}
				else
				{
					MapObjectOvertipsVM.HideBark(entity);
				}
			}
			else
			{
				UnitOvertipsCollectionVM.HideBark(entity3);
			}
		}
		else
		{
			LightweightUnitOvertipsCollectionVM.HideBark(entity2);
		}
	}

	private void ShowBark(Entity entity, string text)
	{
		if (!(entity is LightweightUnitEntity entity2))
		{
			if (!(entity is AbstractUnitEntity entity3))
			{
				if (entity is LocatorEntity entity4)
				{
					LocatorOvertipsCollectionVM.ShowBark(entity4, text);
				}
				else
				{
					MapObjectOvertipsVM.ShowBark(entity, text);
				}
			}
			else
			{
				UnitOvertipsCollectionVM.ShowBark(entity3, text);
			}
		}
		else
		{
			LightweightUnitOvertipsCollectionVM.ShowBark(entity2, text);
		}
	}
}
