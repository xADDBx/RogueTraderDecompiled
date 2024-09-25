using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("07881642342e41fc85fc10eb70eb3261")]
public class RemoveDuplicateRoutes : GameAction
{
	public override string GetCaption()
	{
		return "Remove duplicate routes";
	}

	protected override void RunAction()
	{
		IEnumerable<SectorMapPassageEntity> source = Game.Instance.State.Entities.All.OfType<SectorMapPassageEntity>();
		List<SectorMapPassageEntity> list = new List<SectorMapPassageEntity>();
		IEnumerable<SectorMapObjectEntity> enumerable = Game.Instance.State.Entities.All.OfType<SectorMapObjectEntity>();
		foreach (SectorMapObjectEntity system1 in enumerable)
		{
			foreach (SectorMapObjectEntity system2 in enumerable)
			{
				if (system1 == system2)
				{
					continue;
				}
				List<SectorMapPassageEntity> list2 = (from r in source
					where (r.View.StarSystem1Entity == system1 && r.View.StarSystem2Entity == system2) || (r.View.StarSystem1Entity == system2 && r.View.StarSystem2Entity == system1)
					where r.IsExplored
					select r).EmptyIfNull().ToList();
				if (list2.Count <= 1)
				{
					continue;
				}
				SectorMapPassageEntity sectorMapPassageEntity = list2[0];
				foreach (SectorMapPassageEntity item in list2)
				{
					if (item.CurrentDifficulty < sectorMapPassageEntity.CurrentDifficulty)
					{
						sectorMapPassageEntity = item;
					}
				}
				foreach (SectorMapPassageEntity item2 in list2)
				{
					if (item2 != sectorMapPassageEntity)
					{
						list.Add(item2);
					}
				}
			}
		}
		foreach (SectorMapPassageEntity item3 in list)
		{
			item3.CurrentExploreStatus = SectorMapPassageEntity.ExploreStatus.UnExplored;
			item3.IsInGame = false;
		}
		Game.Instance.SectorMapController.RecalculatePassages();
	}
}
