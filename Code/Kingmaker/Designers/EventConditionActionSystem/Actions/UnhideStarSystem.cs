using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("21478aefd0db492089f79ea2cf10f07b")]
[PlayerUpgraderAllowed(false)]
[KDB("Экшен для пометки системы \"раскрытой\" (доступной для сканов)")]
public class UnhideStarSystem : GameAction
{
	[SerializeField]
	private BlueprintSectorMapPointStarSystem.Reference m_SectorMapPoint;

	[SerializeField]
	[Tooltip("If true, will also make system explored")]
	[KDB("Если галка установлена - то также заэксплорим саму систему")]
	private bool m_ExploreSystem = true;

	[SerializeField]
	[ShowIf("m_ExploreSystem")]
	[Tooltip("If true, will also explore all passages between this system and all connected systems")]
	[KDB("Если галка установлена - то также заэксплорим существующие пути до соседних систем")]
	private bool m_ExploreSystemPassagesAsWell = true;

	private BlueprintSectorMapPointStarSystem SectorMapPoint => m_SectorMapPoint?.Get();

	public override string GetCaption()
	{
		return "Unhide " + SectorMapPoint.name;
	}

	protected override void RunAction()
	{
		SectorMapObjectEntity sectorMapObjectEntity = Game.Instance.State.SectorMapObjects.All.FirstOrDefault((SectorMapObjectEntity obj) => obj.Blueprint == SectorMapPoint);
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
		if (!m_ExploreSystemPassagesAsWell)
		{
			return;
		}
		foreach (SectorMapPassageEntity item in Game.Instance.SectorMapController.AllPassagesForSystem(sectorMapObjectEntity))
		{
			item.Explore();
		}
	}
}
