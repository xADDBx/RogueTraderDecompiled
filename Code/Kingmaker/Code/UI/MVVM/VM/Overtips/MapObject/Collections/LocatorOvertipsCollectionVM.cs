using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject.Collections;

public class LocatorOvertipsCollectionVM : OvertipsCollectionVM<OvertipLocatorVM>, IReloadMechanicsHandler, ISubscriber, IAreaActivationHandler, IAreaHandler
{
	protected override IEnumerable<Entity> Entities => Game.Instance.State.Entities.All.Where((Entity e) => e.IsInGame && e is LocatorEntity);

	protected override bool OvertipGetter(OvertipLocatorVM vm, Entity entity)
	{
		return vm.LocatorEntity == entity as LocatorEntity;
	}

	public LocatorOvertipsCollectionVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		RescanEntities();
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		LocatorView locatorView = entityData.View?.GO.GetComponent<LocatorView>();
		if (locatorView != null)
		{
			return locatorView.IsInGame;
		}
		return false;
	}

	public void ShowBark(Entity entity, string text)
	{
		GetOvertip(entity)?.ShowBark(text);
	}

	public void HideBark(Entity entity)
	{
		GetOvertip(entity)?.HideBark();
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
}
