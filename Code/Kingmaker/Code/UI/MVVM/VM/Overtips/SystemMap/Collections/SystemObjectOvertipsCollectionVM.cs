using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap.Collections;

public class SystemObjectOvertipsCollectionVM : OvertipsCollectionVM<OvertipEntitySystemObjectVM>, IAreaHandler, ISubscriber, IAreaActivationHandler, IAdditiveAreaSwitchHandler
{
	protected override IEnumerable<Entity> Entities => Game.Instance.State.StarSystemObjects.All;

	protected override bool OvertipGetter(OvertipEntitySystemObjectVM vm, Entity entity)
	{
		return vm.SystemMapObject == entity as StarSystemObjectEntity;
	}

	public SystemObjectOvertipsCollectionVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		RescanEntities();
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		IEntityViewBase view = entityData.View;
		if (view == null)
		{
			return false;
		}
		if (view.GO.GetComponent<StarSystemObjectView>() != null && view.GO.GetComponent<AnomalyView>() == null && view.GO.GetComponent<PlanetView>() == null)
		{
			return view.IsInGame;
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
