using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap.Collections;

public class RumoursOvertipsCollectionVM : OvertipsCollectionVM<OvertipEntityRumourVM>, IAreaHandler, ISubscriber, IAreaActivationHandler, IAdditiveAreaSwitchHandler
{
	protected override IEnumerable<Entity> Entities => Game.Instance.LoadedAreaState?.AllEntityData.Where((Entity i) => i is SectorMapRumourEntity || i is SectorMapRumourGroupView.SectorMapRumourGroupEntity);

	protected override bool OvertipGetter(OvertipEntityRumourVM vm, Entity entity)
	{
		if (vm.SectorMapRumour != null && !vm.SectorMapRumour.View.HasParent)
		{
			return vm.SectorMapRumour == entity as SectorMapRumourEntity;
		}
		if (vm.SectorMapRumourGroup != null)
		{
			return vm.SectorMapRumourGroup == entity as SectorMapRumourGroupView.SectorMapRumourGroupEntity;
		}
		return false;
	}

	public RumoursOvertipsCollectionVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		RescanEntities();
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		if (entityData is SectorMapRumourGroupView.SectorMapRumourGroupEntity sectorMapRumourGroupEntity && !sectorMapRumourGroupEntity.View.ActiveQuestObjectives.Empty())
		{
			return true;
		}
		if (!(entityData is SectorMapRumourEntity sectorMapRumourEntity))
		{
			return false;
		}
		if (sectorMapRumourEntity.View != null)
		{
			return !sectorMapRumourEntity.View.HasParent;
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
