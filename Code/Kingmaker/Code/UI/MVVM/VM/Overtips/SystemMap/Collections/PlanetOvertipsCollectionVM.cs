using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap.Collections;

public class PlanetOvertipsCollectionVM : OvertipsCollectionVM<OvertipEntityPlanetVM>, IAreaHandler, ISubscriber, IAreaActivationHandler, IAdditiveAreaSwitchHandler
{
	protected override IEnumerable<Entity> Entities => Game.Instance.State.StarSystemObjects.All;

	protected override bool OvertipGetter(OvertipEntityPlanetVM vm, Entity entity)
	{
		return vm.PlanetObject == entity as StarSystemObjectEntity;
	}

	public PlanetOvertipsCollectionVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		RescanEntities();
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		PlanetView planetView = entityData.View?.GO.GetComponent<PlanetView>();
		if (planetView != null)
		{
			return planetView.IsInGame;
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
