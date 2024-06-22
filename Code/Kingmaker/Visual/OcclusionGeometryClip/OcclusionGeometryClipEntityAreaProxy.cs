using System.Collections.Generic;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using UnityEngine;

namespace Kingmaker.Visual.OcclusionGeometryClip;

[AddComponentMenu("")]
public sealed class OcclusionGeometryClipEntityAreaProxy : MonoBehaviour, IRendererProxy
{
	[SerializeField]
	private PlaneBox m_Bounds;

	private readonly List<OcclusionGeometryClipEntityProxy> m_Entities;

	private OcclusionGeometryClipEntityAreaProxy m_Prev;

	private OcclusionGeometryClipEntityAreaProxy m_Next;

	private float m_Opacity;

	internal int RegistryIndex;

	public PlaneBox Bounds
	{
		get
		{
			return m_Bounds;
		}
		set
		{
			m_Bounds = value;
			OcclusionGeometryClipEntitySystem.UpdateArea(this);
		}
	}

	private void OnEnable()
	{
		OcclusionGeometryClipEntitySystem.AddArea(this);
	}

	private void OnDisable()
	{
		OcclusionGeometryClipEntitySystem.RemoveArea(this);
	}

	public void AddEntity(OcclusionGeometryClipEntityProxy entity)
	{
		m_Entities.Add(entity);
		entity.SetOpacity(m_Opacity);
	}

	public void RemoveEntity(OcclusionGeometryClipEntityProxy entity)
	{
		m_Entities.Remove(entity);
		entity.SetOpacity(1f);
	}

	public float GetOpacity()
	{
		return m_Opacity;
	}

	public void SetOpacity(float value)
	{
		if (m_Opacity == value)
		{
			return;
		}
		m_Opacity = value;
		foreach (OcclusionGeometryClipEntityProxy entity in m_Entities)
		{
			entity.SetOpacity(m_Opacity);
		}
	}

	public OcclusionGeometryClipEntityAreaProxy()
	{
		Vector3 center = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		Vector3 size = Vector3.one;
		m_Bounds = new PlaneBox(in center, in rotation, in size);
		m_Entities = new List<OcclusionGeometryClipEntityProxy>();
		m_Opacity = 1f;
		RegistryIndex = -1;
		base._002Ector();
	}
}
