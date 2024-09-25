using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

public class UnitMovementAgentStarSystem : UnitMovementAgentBase
{
	private StarSystemShip m_StarSystemShip;

	public override bool AvoidanceDisabled => true;

	public override Vector3 Position
	{
		get
		{
			return m_StarSystemShip.Position;
		}
		set
		{
			m_StarSystemShip.Position = value;
		}
	}

	public void Init([NotNull] GameObject owner, [NotNull] StarSystemShip starSystemShip)
	{
		m_StarSystemShip = starSystemShip;
		Init(owner);
	}

	public override void TickMovement(float deltaTime)
	{
		if (!(Game.Instance.CurrentMode != GameModeType.StarSystem))
		{
			base.TickMovement(deltaTime);
		}
	}

	protected override Vector3 MoveInternal(Vector3 currentPos, Vector3 shift, float movementCorpulence)
	{
		return currentPos + shift;
	}

	protected override void StartMovingWithPath(ForcedPath p, bool forcedPath, bool requestedNewPath)
	{
		if (requestedNewPath)
		{
			m_LastTurnAngle = 0f;
			m_FirstTick = true;
		}
		if (m_Destination.HasValue)
		{
			m_IsInForceMode = forcedPath;
			base.Path = p;
			SetWaypoint(1);
		}
	}

	public List<Vector3> GetRemainingPath()
	{
		List<Vector3> list = new List<Vector3> { Position };
		if (base.Path?.vectorPath != null)
		{
			for (int i = m_NextPointIndex; i < base.Path.vectorPath.Count; i++)
			{
				list.Add(base.Path.vectorPath[i]);
			}
		}
		return list;
	}
}
