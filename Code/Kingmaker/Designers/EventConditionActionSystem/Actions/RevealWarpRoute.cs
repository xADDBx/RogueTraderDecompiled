using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("f809d41697e94786a9733f2379d9fff2")]
public class RevealWarpRoute : GameAction
{
	[SerializeReference]
	public MechanicEntityEvaluator m_System1;

	[SerializeReference]
	public MechanicEntityEvaluator m_System2;

	public override string GetCaption()
	{
		return $"Reveal warp route {m_System1} <-> {m_System2}";
	}

	protected override void RunAction()
	{
		if (m_System1 == null || m_System2 == null)
		{
			return;
		}
		SectorMapObjectEntity system1Entity = m_System1.GetValue() as SectorMapObjectEntity;
		SectorMapObjectEntity system2Entity = m_System2.GetValue() as SectorMapObjectEntity;
		if (system1Entity == null || system2Entity == null)
		{
			return;
		}
		IEnumerable<SectorMapPassageEntity> list = from r in Game.Instance.State.Entities.All.OfType<SectorMapPassageEntity>()
			where (r.View.StarSystem1 == system1Entity.View && r.View.StarSystem2 == system2Entity.View) || (r.View.StarSystem1 == system2Entity.View && r.View.StarSystem2 == system1Entity.View)
			select r;
		if (list.EmptyIfNull().FirstOrDefault((SectorMapPassageEntity r) => r.IsExplored) == null)
		{
			SectorMapPassageEntity sectorMapPassageEntity = list.EmptyIfNull().First();
			if (sectorMapPassageEntity != null)
			{
				sectorMapPassageEntity.IsInGame = true;
				sectorMapPassageEntity.Explore();
			}
		}
	}
}
