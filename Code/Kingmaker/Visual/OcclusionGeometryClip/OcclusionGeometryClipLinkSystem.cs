using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.OcclusionGeometryClip;

internal static class OcclusionGeometryClipLinkSystem
{
	private const int kInvalidIndex = -1;

	private static readonly List<OcclusionGeometryClipLinkVolumeProxy> s_Volumes = new List<OcclusionGeometryClipLinkVolumeProxy>();

	private static readonly List<OcclusionGeometryClipLinkProxy> s_Proxies = new List<OcclusionGeometryClipLinkProxy>();

	public static void ResetVolume(OcclusionGeometryClipLinkVolumeProxy volume)
	{
		if (-1 != volume.IntrusiveIndex)
		{
			volume.ClearProxies();
			LinkVolume(volume);
		}
	}

	public static void AddVolume(OcclusionGeometryClipLinkVolumeProxy volume)
	{
		volume.IntrusiveIndex = s_Volumes.Count;
		s_Volumes.Add(volume);
		LinkVolume(volume);
	}

	public static void RemoveVolume(OcclusionGeometryClipLinkVolumeProxy volume)
	{
		int intrusiveIndex = volume.IntrusiveIndex;
		int num = s_Volumes.Count - 1;
		volume.IntrusiveIndex = -1;
		volume.ClearProxies();
		if (intrusiveIndex != num)
		{
			s_Volumes[intrusiveIndex] = s_Volumes[num];
			s_Volumes[intrusiveIndex].IntrusiveIndex = intrusiveIndex;
		}
		s_Volumes.RemoveAt(num);
	}

	public static void AddProxy(OcclusionGeometryClipLinkProxy proxy)
	{
		proxy.IntrusiveIndex = s_Proxies.Count;
		s_Proxies.Add(proxy);
		LinkProxy(proxy);
	}

	public static void RemoveProxy(OcclusionGeometryClipLinkProxy proxy)
	{
		int intrusiveIndex = proxy.IntrusiveIndex;
		int num = s_Proxies.Count - 1;
		proxy.IntrusiveIndex = -1;
		if (proxy.LinkedVolume != null)
		{
			proxy.LinkedVolume.RemoveProxy(proxy);
			proxy.LinkedVolume = null;
		}
		if (intrusiveIndex != num)
		{
			s_Proxies[intrusiveIndex] = s_Proxies[num];
			s_Proxies[intrusiveIndex].IntrusiveIndex = intrusiveIndex;
		}
		s_Proxies.RemoveAt(num);
	}

	private static void LinkVolume(OcclusionGeometryClipLinkVolumeProxy volume)
	{
		foreach (OcclusionGeometryClipLinkProxy s_Proxy in s_Proxies)
		{
			if (!(s_Proxy.LinkedVolume != null))
			{
				Vector3 position = s_Proxy.transform.position;
				Vector4 point = new Vector4(position.x, position.y, position.z, 1f);
				if (volume.Bounds.ContainsPoint(in point))
				{
					volume.AddProxy(s_Proxy);
				}
			}
		}
	}

	private static void LinkProxy(OcclusionGeometryClipLinkProxy proxy)
	{
		Vector3 position = proxy.transform.position;
		Vector4 point = new Vector4(position.x, position.y, position.z, 1f);
		foreach (OcclusionGeometryClipLinkVolumeProxy s_Volume in s_Volumes)
		{
			if (s_Volume.Bounds.ContainsPoint(in point))
			{
				s_Volume.AddProxy(proxy);
				break;
			}
		}
	}
}
