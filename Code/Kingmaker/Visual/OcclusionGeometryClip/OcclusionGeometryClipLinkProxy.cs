using System;
using UnityEngine;

namespace Kingmaker.Visual.OcclusionGeometryClip;

public abstract class OcclusionGeometryClipLinkProxy : MonoBehaviour
{
	[NonSerialized]
	internal int IntrusiveIndex = -1;

	[NonSerialized]
	internal OcclusionGeometryClipLinkVolumeProxy LinkedVolume;

	public abstract void SetOpacity(float value);

	protected virtual void OnEnable()
	{
		OcclusionGeometryClipLinkSystem.AddProxy(this);
	}

	protected virtual void OnDisable()
	{
		OcclusionGeometryClipLinkSystem.RemoveProxy(this);
	}
}
