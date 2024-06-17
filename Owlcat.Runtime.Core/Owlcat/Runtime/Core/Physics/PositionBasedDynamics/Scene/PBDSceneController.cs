using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

internal class PBDSceneController
{
	private HashSet<PBDBodyBase> m_Bodies = new HashSet<PBDBodyBase>();

	private HashSet<PBDBodyBase> m_SkipFirstFrameBodies = new HashSet<PBDBodyBase>();

	internal PBDSceneController()
	{
		PBD.OnBodyDataUpdated = (Action<HashSet<Body>>)Delegate.Combine(PBD.OnBodyDataUpdated, new Action<HashSet<Body>>(OnBodyDataUpdated));
		PBD.OnBeforeSimulationTick = (Action)Delegate.Combine(PBD.OnBeforeSimulationTick, new Action(OnBeforeSimulationTick));
	}

	~PBDSceneController()
	{
		PBD.OnBodyDataUpdated = (Action<HashSet<Body>>)Delegate.Remove(PBD.OnBodyDataUpdated, new Action<HashSet<Body>>(OnBodyDataUpdated));
	}

	internal void RegisterBody(PBDBodyBase body)
	{
		m_Bodies.Add(body);
		if (body.BodyInitializationMode == BodyInitializationMode.SkipFirstFrame)
		{
			m_SkipFirstFrameBodies.Add(body);
		}
	}

	internal void UnregisterBody(PBDBodyBase body)
	{
		m_Bodies.Remove(body);
		m_SkipFirstFrameBodies.Remove(body);
	}

	internal void Tick()
	{
		if (m_SkipFirstFrameBodies.Count > 0)
		{
			foreach (PBDBodyBase skipFirstFrameBody in m_SkipFirstFrameBodies)
			{
				skipFirstFrameBody.Initialize();
			}
			m_SkipFirstFrameBodies.Clear();
		}
		foreach (PBDBodyBase body in m_Bodies)
		{
			body.DoUpdate();
		}
	}

	private void OnBodyDataUpdated(HashSet<Body> bodies)
	{
		foreach (PBDBodyBase body2 in m_Bodies)
		{
			Body body = body2.GetBody();
			if (body != null && bodies.Contains(body))
			{
				body2.OnBodyDataUpdated();
			}
		}
	}

	private void OnBeforeSimulationTick()
	{
		foreach (PBDBodyBase body in m_Bodies)
		{
			if (!body.IsStatic)
			{
				body.OnBeforeSimulationTick();
			}
		}
	}
}
