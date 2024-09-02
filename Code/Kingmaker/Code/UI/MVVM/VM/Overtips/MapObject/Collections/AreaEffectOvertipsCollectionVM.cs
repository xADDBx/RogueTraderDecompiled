using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject.Collections;

public class AreaEffectOvertipsCollectionVM : OvertipsCollectionVM<OvertipAreaEffectVM>, IReloadMechanicsHandler, ISubscriber, IAreaActivationHandler, IAreaHandler
{
	protected override IEnumerable<Entity> Entities => Game.Instance.State.AreaEffects.All.Where((AreaEffectEntity e) => e.IsInGame && e.Blueprint.NeedsTooltip);

	protected override bool OvertipGetter(OvertipAreaEffectVM vm, Entity entity)
	{
		return vm.AreaEffectEntity == entity as AreaEffectEntity;
	}

	public AreaEffectOvertipsCollectionVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		RescanEntities();
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		AreaEffectView areaEffectView = entityData.View?.GO.GetComponent<AreaEffectView>();
		if (areaEffectView != null && areaEffectView.IsInGame)
		{
			return areaEffectView.Data.Blueprint.NeedsTooltip;
		}
		return false;
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
