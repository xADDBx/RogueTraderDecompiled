using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject.Collections;

public class BaseMapObjectOvertipsCollectionVM<TOvertipVM> : OvertipsCollectionVM<TOvertipVM>, IMapObjectHandler, ISubscriber<IMapObjectEntity>, ISubscriber, IReloadMechanicsHandler, IAreaActivationHandler, IAreaHandler, ISurroundingInteractableObjectsCountHandler where TOvertipVM : BaseOvertipMapObjectVM
{
	protected override IEnumerable<Entity> Entities => Game.Instance.State.MapObjects.All;

	protected override bool OvertipGetter(TOvertipVM vm, Entity entity)
	{
		return vm.MapObjectEntity == entity as MapObjectEntity;
	}

	public BaseMapObjectOvertipsCollectionVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		RescanEntities();
	}

	protected MapObjectEntity GetMapObject()
	{
		return EventInvokerExtensions.MapObjectEntity;
	}

	protected MapObjectEntity GetRevealedMapObject()
	{
		MapObjectEntity mapObjectEntity = EventInvokerExtensions.MapObjectEntity;
		if (mapObjectEntity == null || !mapObjectEntity.IsRevealed)
		{
			return null;
		}
		return mapObjectEntity;
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		return true;
	}

	void IMapObjectHandler.HandleMapObjectSpawned()
	{
		AddEntity(EventInvokerExtensions.GetEntity<MapObjectEntity>());
	}

	void IMapObjectHandler.HandleMapObjectDestroyed()
	{
		RemoveEntity(EventInvokerExtensions.GetEntity<MapObjectEntity>());
	}

	void IReloadMechanicsHandler.OnBeforeMechanicsReload()
	{
		Clear();
	}

	void IReloadMechanicsHandler.OnMechanicsReloaded()
	{
		RescanEntities();
	}

	void IAreaActivationHandler.OnAreaActivated()
	{
		RescanEntities();
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
		Clear();
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		RescanEntities();
	}

	public void HandleSurroundingInteractableObjectsCountChanged(EntityViewBase entity, bool isInNavigation, bool isChosen)
	{
		GetOvertip(entity.Data.ToEntity())?.HandleSurroundingObjectsChanged(isInNavigation, isChosen);
	}
}
