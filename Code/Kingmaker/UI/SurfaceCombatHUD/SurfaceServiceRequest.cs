using System;
using System.Collections.Generic;
using Kingmaker.Pathfinding;
using Unity.Collections;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public sealed class SurfaceServiceRequest : IDisposable
{
	private readonly List<AreaData> m_Areas = new List<AreaData>();

	private readonly List<MaterialData> m_Materials = new List<MaterialData>();

	private CommandBuffer m_CommandBuffer;

	public List<AreaData> Areas => m_Areas;

	public List<MaterialData> Materials => m_Materials;

	public CommandBuffer CommandBuffer => m_CommandBuffer;

	public Vector3 Offset { get; set; }

	public CustomGridGraph Graph { get; set; }

	public OutlineSettings OutlineSettings { get; set; }

	public FillSettings FillSettings { get; set; }

	public SurfaceServiceRequest()
	{
		m_CommandBuffer = new CommandBuffer(Allocator.Persistent);
	}

	public void Dispose()
	{
		m_CommandBuffer.Dispose();
	}

	public void Clear()
	{
		m_Areas.Clear();
		m_Materials.Clear();
		m_CommandBuffer.Clear();
		Offset = default(Vector3);
		Graph = null;
		OutlineSettings = default(OutlineSettings);
		FillSettings = default(FillSettings);
	}

	public int InsertMaterial(Material material, MaterialOverrides overrides)
	{
		MaterialData item = new MaterialData(material, overrides);
		int num = m_Materials.IndexOf(item);
		if (num < 0)
		{
			num = m_Materials.Count;
			m_Materials.Add(item);
		}
		return num;
	}
}
