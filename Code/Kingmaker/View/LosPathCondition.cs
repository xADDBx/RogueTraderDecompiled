using System;
using Kingmaker.Pathfinding;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

public class LosPathCondition : ABPathEndingCondition
{
	private readonly bool m_CheckLos;

	private readonly float m_ApproachRadius;

	public LosPathCondition(ABPath path, float approachRadius, bool checkLos)
		: base(path)
	{
		m_CheckLos = checkLos;
		m_ApproachRadius = approachRadius;
	}

	public override bool TargetFound(PathNode node)
	{
		if (base.TargetFound(node))
		{
			return true;
		}
		Vector3 vector = (Vector3)node.node.position;
		if (GeometryUtils.SqrMechanicsDistance(vector, abPath.originalEndPoint) / Mathf.Pow(GraphParamsMechanicsCache.GridCellSize, 2f) > m_ApproachRadius * m_ApproachRadius)
		{
			return false;
		}
		try
		{
			if (m_CheckLos && LosCalculations.GetDirectLos(vector, abPath.originalEndPoint))
			{
				return false;
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return false;
		}
		return true;
	}
}
