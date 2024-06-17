using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities.Crew;

[TypeId("2ac906dbac5b4db8a0eb7115e433506b")]
public class ContextActionDistributeCrewQuarters : ContextAction
{
	[SerializeField]
	private int m_DistributeCount;

	public override string GetCaption()
	{
		return $"Distribute {m_DistributeCount} from crew quarters to modules";
	}

	public override void RunAction()
	{
		if (base.Target?.Entity is StarshipEntity starshipEntity)
		{
			starshipEntity.Crew.DistributeQuarters(m_DistributeCount);
		}
	}
}
