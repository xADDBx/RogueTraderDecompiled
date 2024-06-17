using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap.Collections;

public class RumoursOvertipsCollectionVM : OvertipsCollectionVM<OvertipEntityRumourVM>, IAreaHandler, ISubscriber, IAreaActivationHandler, IAdditiveAreaSwitchHandler
{
	protected override IEnumerable<Entity> Entities => Game.Instance.LoadedAreaState?.AllEntityData.OfType<SectorMapRumourEntity>();

	protected override Func<OvertipEntityRumourVM, Entity, bool> OvertipGetter => (OvertipEntityRumourVM vm, Entity entity) => vm.SectorMapRumour == entity as SectorMapRumourEntity;

	public RumoursOvertipsCollectionVM()
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
