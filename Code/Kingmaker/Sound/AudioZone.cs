using System.Collections.Generic;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Kingmaker.Sound;

public class AudioZone : RegisteredBehaviour
{
	public Bounds Bounds;

	private readonly List<AudioObject> m_ObjectsInside = new List<AudioObject>();

	private bool m_ListenerInside;

	private AudioEnvironment m_Environment;

	public IEnumerable<AudioObject> ObjectsInside => m_ObjectsInside;

	public bool ListenerInside => m_ListenerInside;

	protected override void OnEnabled()
	{
		base.OnEnabled();
		m_Environment = GetComponent<AudioEnvironment>();
		foreach (AudioObject item in ObjectRegistry<AudioObject>.Instance.EmptyIfNull())
		{
			if (item.ShouldUpdateZones)
			{
				UpdateGameObj(item);
			}
		}
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		foreach (AudioObject item in m_ObjectsInside)
		{
			if ((bool)item)
			{
				item.RemoveAuxSend(m_Environment);
			}
		}
		m_ObjectsInside.Clear();
		if (m_ListenerInside)
		{
			RaiseListenerEvent(isInside: false);
			m_ListenerInside = false;
		}
	}

	public void UpdateGameObj(AudioObject obj)
	{
		if ((bool)m_Environment)
		{
			bool flag = Bounds.Contains(base.transform.InverseTransformPoint(obj.transform.position));
			bool flag2 = m_ObjectsInside.Contains(obj);
			if (flag && !flag2)
			{
				Add(obj);
			}
			if (flag2 && !flag)
			{
				Remove(obj);
			}
		}
	}

	public void OnDisableObject(AudioObject obj)
	{
		if ((bool)m_Environment)
		{
			m_ObjectsInside.Remove(obj);
		}
	}

	private void Add(AudioObject akGameObj)
	{
		m_ObjectsInside.Add(akGameObj);
		akGameObj.AddAuxSend(m_Environment);
	}

	private void Remove(AudioObject akGameObj)
	{
		m_ObjectsInside.Remove(akGameObj);
		akGameObj.RemoveAuxSend(m_Environment);
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = new Color(0.81f, 1f, 0.19f);
		Gizmos.DrawWireCube(Bounds.center, Bounds.size);
	}

	public static void UpdateListenerPosition(Transform akAudioListener)
	{
		if (LoadingProcess.Instance.IsLoadingInProcess)
		{
			return;
		}
		foreach (AudioZone item in ObjectRegistry<AudioZone>.Instance.EmptyIfNull())
		{
			UpdateListenerPosition(akAudioListener, item);
		}
	}

	internal static void RemoveListenerFromAllZones()
	{
		foreach (AudioZone item in ObjectRegistry<AudioZone>.Instance.EmptyIfNull())
		{
			if (item.m_ListenerInside)
			{
				item.m_ListenerInside = false;
				item.RaiseListenerEvent(isInside: false);
			}
		}
	}

	private static void UpdateListenerPosition(Transform akAudioListener, AudioZone z)
	{
		bool flag = z.Bounds.Contains(z.transform.InverseTransformPoint(akAudioListener.position));
		if (flag != z.m_ListenerInside)
		{
			z.m_ListenerInside = flag;
			z.RaiseListenerEvent(flag);
		}
	}

	private void RaiseListenerEvent(bool isInside)
	{
		EventBus.RaiseEvent(delegate(IAudioZoneHandler h)
		{
			h.HandleListenerZone(this, isInside);
		});
	}
}
