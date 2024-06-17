using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic.Parts.Crew;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities.Crew;

[TypeId("08e46dfc93144bd8ac752e649775e5b9")]
public class ContextActionMoveCrewToCrewQuarters : ContextAction
{
	[SerializeField]
	private float m_PercentToMove = 0.3f;

	[SerializeField]
	private ShipModuleType m_ModuleType;

	public override string GetCaption()
	{
		return $"Move {m_PercentToMove} % from module {m_ModuleType} to crew quarters and distribute";
	}

	public override void RunAction()
	{
		if (base.Target?.Entity is StarshipEntity starshipEntity)
		{
			IReadOnlyStarshipModuleCrewWrapper readOnlyCrewData = starshipEntity.Crew.GetReadOnlyCrewData(m_ModuleType);
			int count = (int)Math.Ceiling((float)readOnlyCrewData.GetAvailableToMoveCount() * m_PercentToMove);
			starshipEntity.Crew.MoveCrewToQuarters(readOnlyCrewData.ShipModuleType, count);
		}
	}
}
