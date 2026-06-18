using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.SectorMap;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("967a4c112a2392944bbd212d4fb9fd91")]
public class UnRevealStarSystems : PlayerUpgraderOnlyAction
{
	[ValidateNoNullEntries]
	[SerializeField]
	private BlueprintSectorMapPointStarSystem.Reference[] m_StarSystems;

	public override string GetDescription()
	{
		return "Сбрасывает у указанных систем сектора признаки разведки/посещения и снова прячет их.";
	}

	public override string GetCaption()
	{
		BlueprintSectorMapPointStarSystem.Reference[] starSystems = m_StarSystems;
		return $"UnReveal Star Systems ({((starSystems != null) ? starSystems.Length : 0)})";
	}

	protected override void RunActionOverride()
	{
		BlueprintSectorMapPointStarSystem.Reference[] starSystems = m_StarSystems;
		for (int i = 0; i < starSystems.Length; i++)
		{
			BlueprintSectorMapPointStarSystem blueprintSectorMapPointStarSystem = starSystems[i]?.Get();
			if (blueprintSectorMapPointStarSystem == null)
			{
				continue;
			}
			foreach (SectorMapObjectEntity sectorMapObject in Game.Instance.State.SectorMapObjects)
			{
				if (sectorMapObject.Blueprint == blueprintSectorMapPointStarSystem)
				{
					sectorMapObject.IsExplored = false;
					sectorMapObject.IsVisited = false;
					sectorMapObject.IsScannedFrom = false;
					sectorMapObject.IsHidden = true;
					sectorMapObject.IsInGame = false;
				}
			}
			UnExplorePassagesTo(blueprintSectorMapPointStarSystem);
		}
	}

	private static void UnExplorePassagesTo(BlueprintSectorMapPointStarSystem blueprint)
	{
		foreach (SectorMapPassageEntity item in Game.Instance.State.Entities.OfType<SectorMapPassageEntity>())
		{
			if (item.StarSystem1Blueprint == blueprint || item.StarSystem2Blueprint == blueprint)
			{
				item.CurrentExploreStatus = SectorMapPassageEntity.ExploreStatus.UnExplored;
				item.SectorMapScanFrom = null;
			}
		}
	}
}
