using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap.Collections;

public class AnomalyOvertipsCollectionVM : OvertipsCollectionVM<OvertipEntityAnomalyVM>, IAreaHandler, ISubscriber, IAreaActivationHandler, IAdditiveAreaSwitchHandler
{
	protected override IEnumerable<Entity> Entities => Game.Instance.State.StarSystemObjects.All;

	protected override bool OvertipGetter(OvertipEntityAnomalyVM vm, Entity entity)
	{
		return vm.SystemMapObject == entity as MapObjectEntity;
	}

	public AnomalyOvertipsCollectionVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		RescanEntities();
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		AnomalyView anomalyView = entityData.View?.GO.GetComponent<AnomalyView>();
		if (anomalyView != null)
		{
			return anomalyView.IsInGame;
		}
		return false;
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
