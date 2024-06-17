using System;
using System.Linq;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.CoverVisualiserSystem;

public class GridVisualizer : MonoBehaviour, ITurnBasedModeHandler, ISubscriber
{
	private Renderer[] Renderers = new Renderer[0];

	private void OnEnable()
	{
		Renderers = GetComponentsInChildren<Renderer>();
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Combine(AstarPath.OnGraphsUpdated, new OnScanDelegate(Rebuild));
		EventBus.Subscribe(this);
		HandleTurnBasedModeSwitched(Game.Instance.TurnController.TurnBasedModeActive);
	}

	private void OnDisable()
	{
		Renderers = Array.Empty<Renderer>();
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Remove(AstarPath.OnGraphsUpdated, new OnScanDelegate(Rebuild));
		EventBus.Unsubscribe(this);
		HandleTurnBasedModeSwitched(isTurnBased: false);
	}

	private void Rebuild(AstarPath path)
	{
		CustomGridGraph customGridGraph = AstarPath.active.graphs.OfType<CustomGridGraph>().FirstOrDefault();
		if (customGridGraph != null)
		{
			Vector2 size = customGridGraph.size;
			base.gameObject.transform.localPosition = customGridGraph.center;
			base.gameObject.transform.localScale = new Vector3(size.x, 1000f, size.y);
			base.gameObject.transform.rotation = Quaternion.Euler(customGridGraph.rotation);
			Renderer[] renderers = Renderers;
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].material.mainTextureScale = new Vector2(customGridGraph.width, customGridGraph.depth);
			}
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased && AstarPath.active != null)
		{
			Rebuild(AstarPath.active);
		}
		Renderer[] renderers = Renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].enabled = isTurnBased;
		}
	}
}
