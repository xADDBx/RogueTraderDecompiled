using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject.Collections;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips;

public class SpaceOvertipsVM : OvertipsVM
{
	public readonly SystemMapOvertipsVM SystemMapOvertipsVM;

	public readonly SectorMapOvertipsVM SectorMapOvertipsVM;

	public readonly UnitOvertipsCollectionVM UnitOvertipsCollectionVM;

	public readonly AreaEffectOvertipsCollectionVM AreaEffectOvertipsCollectionVM;

	public SpaceOvertipsVM()
	{
		AddDisposable(SystemMapOvertipsVM = new SystemMapOvertipsVM());
		AddDisposable(SectorMapOvertipsVM = new SectorMapOvertipsVM());
		AddDisposable(UnitOvertipsCollectionVM = new UnitOvertipsCollectionVM());
		AddDisposable(AreaEffectOvertipsCollectionVM = new AreaEffectOvertipsCollectionVM());
	}

	protected override void DisposeImplementation()
	{
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
		if (EventInvokerExtensions.Entity is BaseUnitEntity entity)
		{
			UnitOvertipsCollectionVM.HideBark(entity);
		}
	}

	private void ShowBark(Entity entity, string text)
	{
		if (entity is BaseUnitEntity entity2)
		{
			UnitOvertipsCollectionVM.ShowBark(entity2, text);
		}
	}
}
