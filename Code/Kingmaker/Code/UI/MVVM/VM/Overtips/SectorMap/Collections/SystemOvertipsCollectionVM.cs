using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap.Collections;

public class SystemOvertipsCollectionVM : OvertipsCollectionVM<OvertipEntitySystemVM>, IAreaHandler, ISubscriber, IAreaActivationHandler, IAdditiveAreaSwitchHandler
{
	protected override IEnumerable<Entity> Entities => from obj in Game.Instance.SectorMapController.GetAllStarSystems()
		select obj.Data;

	protected override Func<OvertipEntitySystemVM, Entity, bool> OvertipGetter => (OvertipEntitySystemVM vm, Entity entity) => vm.SectorMapObject == entity as SectorMapObjectEntity;

	public SystemOvertipsCollectionVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		RescanEntities();
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		return true;
	}

	public void OnAreaBeginUnloading()
	{
		Clear();
	}

	public void OnAreaDidLoad()
	{
		RescanEntities();
	}

	public void OnAreaActivated()
	{
		RescanEntities();
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		Clear();
	}

	public void OnAdditiveAreaDidActivated()
	{
		RescanEntities();
	}
}
