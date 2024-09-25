using System;
using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject.Collections;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;

public class MapObjectOvertipsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly TransitionOvertipsCollectionVM TransitionOvertipsCollectionVM;

	public readonly MapInteractionObjectOvertipsCollectionVM MapInteractionObjectOvertipsCollectionVM;

	public readonly DestructibleObjectOvertipsCollectionVM DestructibleObjectOvertipsCollectionVM;

	public MapObjectOvertipsVM()
	{
		AddDisposable(TransitionOvertipsCollectionVM = new TransitionOvertipsCollectionVM());
		AddDisposable(MapInteractionObjectOvertipsCollectionVM = new MapInteractionObjectOvertipsCollectionVM());
		AddDisposable(DestructibleObjectOvertipsCollectionVM = new DestructibleObjectOvertipsCollectionVM());
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void ShowBark(Entity entity, string text)
	{
		MapInteractionObjectOvertipsCollectionVM.ShowBark(entity, text);
		DestructibleObjectOvertipsCollectionVM.ShowBark(entity, text);
	}

	public void HideBark(Entity entity)
	{
		MapInteractionObjectOvertipsCollectionVM.HideBark(entity);
		DestructibleObjectOvertipsCollectionVM.HideBark(entity);
	}
}
