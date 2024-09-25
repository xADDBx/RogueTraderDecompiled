using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public sealed class CombatHudPathRenderer : MonoBehaviour, IAreaHandler, ISubscriber
{
	[Serializable]
	public struct Config
	{
		public Material normalMaterial;

		public Material unableMaterial;

		public PathLineSettings lineSettings;

		public bool IsValid()
		{
			if (normalMaterial != null)
			{
				return unableMaterial != null;
			}
			return false;
		}
	}

	[Header("Components")]
	[SerializeField]
	private MeshFilter m_PathMeshFilter;

	[SerializeField]
	private MeshRenderer m_PathMeshRenderer;

	[Header("Settings")]
	[SerializeField]
	private Config m_SurfaceCombatConfig;

	[SerializeField]
	private Config m_SpaceCombatConfig;

	[SerializeField]
	private Vector3 m_MeshOffset;

	private PathService m_Service;

	private readonly CombatHubCollectionAreaSource m_Source = new CombatHubCollectionAreaSource();

	private readonly PathServiceRequestPool m_RequestPool = new PathServiceRequestPool();

	private bool m_UseSpaceCombatConfig;

	public bool PathShown { get; private set; }

	[UsedImplicitly]
	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
		if (m_Service != null)
		{
			m_Service.Dispose();
			m_Service = null;
		}
	}

	[UsedImplicitly]
	private void LateUpdate()
	{
		m_Service?.Update();
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		m_UseSpaceCombatConfig = (Game.Instance.State?.LoadedAreaState?.Blueprint?.IsShipArea).GetValueOrDefault();
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
	}

	public void Show(List<GraphNode> nodes, Transform progressTargetTransform, bool pathUnableStatus, Vector3 meshOffset)
	{
		CustomGridGraph graph = CombatHudGraphDataSource.FindGraph();
		if (EnsurePrerequisites(graph))
		{
			m_Source.Clear();
			if (nodes != null)
			{
				m_Source.AddRange(nodes);
			}
			m_Service.DiscardPendingRequest();
			PathServiceRequest pathServiceRequest = m_RequestPool.Get();
			pathServiceRequest.GridSettings = new GridSettings(graph);
			pathServiceRequest.Graph = graph;
			pathServiceRequest.PathLineSettings = GetConfig().lineSettings;
			pathServiceRequest.source = m_Source;
			pathServiceRequest.material = (pathUnableStatus ? GetConfig().unableMaterial : GetConfig().normalMaterial);
			pathServiceRequest.positionOffset = m_MeshOffset + meshOffset;
			pathServiceRequest.progressTrackingTransform = progressTargetTransform;
			m_Service.SetPendingRequest(pathServiceRequest);
			PathShown = !nodes.Empty();
		}
	}

	private bool EnsurePrerequisites(CustomGridGraph graph)
	{
		if (graph == null)
		{
			return false;
		}
		if (m_PathMeshFilter == null)
		{
			return false;
		}
		if (m_PathMeshRenderer == null)
		{
			return false;
		}
		if (!GetConfig().IsValid())
		{
			return false;
		}
		if (m_Service == null)
		{
			m_Service = new PathService(m_PathMeshFilter, m_PathMeshRenderer, m_RequestPool);
		}
		return true;
	}

	private ref Config GetConfig()
	{
		if (m_UseSpaceCombatConfig)
		{
			return ref m_SpaceCombatConfig;
		}
		return ref m_SurfaceCombatConfig;
	}
}
