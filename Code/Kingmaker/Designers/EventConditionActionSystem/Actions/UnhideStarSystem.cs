using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.SectorMap;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("21478aefd0db492089f79ea2cf10f07b")]
[PlayerUpgraderAllowed(false)]
public class UnhideStarSystem : GameAction
{
	[SerializeField]
	private BlueprintSectorMapPointStarSystem.Reference m_SectorMapPoint;

	[SerializeField]
	private bool m_ExploreSystem = true;

	private BlueprintSectorMapPointStarSystem SectorMapPoint => m_SectorMapPoint?.Get();

	public override string GetCaption()
	{
		return "Unhide " + SectorMapPoint.name;
	}

	public override void RunAction()
	{
		List<SectorMapObjectEntity> all = Game.Instance.State.SectorMapObjects.All;
		SectorMapObjectEntity sectorMapObjectEntity = all.FirstOrDefault((SectorMapObjectEntity obj) => obj.Blueprint == SectorMapPoint);
		if (sectorMapObjectEntity == null)
		{
			return;
		}
		sectorMapObjectEntity.IsInGame = true;
		sectorMapObjectEntity.IsHidden = false;
		if (!m_ExploreSystem)
		{
			return;
		}
		sectorMapObjectEntity.Explore();
		foreach (SectorMapPassageEntity item in Game.Instance.SectorMapController.AllPassagesForSystem(sectorMapObjectEntity))
		{
			item.Explore();
			BlueprintSectorMapPoint otherBlueprint = ((item.StarSystem1Blueprint == SectorMapPoint) ? item.StarSystem2Blueprint : item.StarSystem1Blueprint);
			all.FirstOrDefault((SectorMapObjectEntity obj) => obj.Blueprint == otherBlueprint)?.Explore();
		}
	}
}
