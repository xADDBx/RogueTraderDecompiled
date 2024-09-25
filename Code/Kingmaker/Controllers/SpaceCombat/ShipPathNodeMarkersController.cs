using Kingmaker.Controllers.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.UI.PathRenderer;
using UnityEngine;

namespace Kingmaker.Controllers.SpaceCombat;

public class ShipPathNodeMarkersController : IControllerTick, IController, IControllerStop
{
	private CustomGridNodeBase m_CurrentNode;

	private bool m_CurrentNodeChanged;

	private Vector3 PointerWorldCorrectedPosition
	{
		get
		{
			if (Game.Instance.ClickEventsController == null)
			{
				return Vector3.zero;
			}
			return Game.Instance.ClickEventsController.WorldPosition;
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		UpdateCurrentNode();
		if (m_CurrentNodeChanged)
		{
			ShipPathManager.Instance.UpdatePathNodeMarkers(m_CurrentNode);
			m_CurrentNodeChanged = false;
		}
	}

	public void OnStop()
	{
		m_CurrentNode = null;
		ShipPathManager.Instance.DisablePathNodeMarkers();
	}

	private void UpdateCurrentNode()
	{
		CustomGridNodeBase currentNode = m_CurrentNode;
		m_CurrentNode = PointerWorldCorrectedPosition.GetNearestNodeXZ();
		m_CurrentNodeChanged = m_CurrentNode != currentNode;
	}
}
