using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using UnityEngine;

namespace Kingmaker.Visual.OcclusionGeometryClip;

[AddComponentMenu("")]
public sealed class OcclusionGeometryClipLinkVolumeProxy : MonoBehaviour, IRendererProxy
{
	[SerializeField]
	private PlaneBox m_Bounds;

	private readonly List<OcclusionGeometryClipLinkProxy> m_LinkedProxies;

	private OcclusionGeometryClipLinkVolumeProxy m_Prev;

	private OcclusionGeometryClipLinkVolumeProxy m_Next;

	private float m_Opacity;

	[NonSerialized]
	internal int IntrusiveIndex;

	public PlaneBox Bounds
	{
		get
		{
			return m_Bounds;
		}
		set
		{
			m_Bounds = value;
			OcclusionGeometryClipLinkSystem.ResetVolume(this);
		}
	}

	private void OnEnable()
	{
		OcclusionGeometryClipLinkSystem.AddVolume(this);
	}

	private void OnDisable()
	{
		OcclusionGeometryClipLinkSystem.RemoveVolume(this);
	}

	public void AddProxy(OcclusionGeometryClipLinkProxy proxy)
	{
		m_LinkedProxies.Add(proxy);
		proxy.LinkedVolume = this;
		proxy.SetOpacity(m_Opacity);
	}

	public void RemoveProxy(OcclusionGeometryClipLinkProxy proxy)
	{
		m_LinkedProxies.Remove(proxy);
		proxy.LinkedVolume = null;
		proxy.SetOpacity(1f);
	}

	public void ClearProxies()
	{
		foreach (OcclusionGeometryClipLinkProxy linkedProxy in m_LinkedProxies)
		{
			linkedProxy.SetOpacity(1f);
			linkedProxy.LinkedVolume = null;
		}
		m_LinkedProxies.Clear();
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
		foreach (OcclusionGeometryClipLinkProxy linkedProxy in m_LinkedProxies)
		{
			linkedProxy.SetOpacity(m_Opacity);
		}
	}

	public OcclusionGeometryClipLinkVolumeProxy()
	{
		Vector3 center = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		Vector3 size = Vector3.one;
		m_Bounds = new PlaneBox(in center, in rotation, in size);
		m_LinkedProxies = new List<OcclusionGeometryClipLinkProxy>();
		m_Opacity = 1f;
		IntrusiveIndex = -1;
		base._002Ector();
	}
}
