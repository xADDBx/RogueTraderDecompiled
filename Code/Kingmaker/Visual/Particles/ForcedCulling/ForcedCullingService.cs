using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Updatables;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.Visual.Particles.ForcedCulling;

public class ForcedCullingService : RegisteredObjectBase, IService, IUpdatable, IDisposable
{
	private class UncullEverythingScope : IDisposable
	{
		private readonly List<ForcedCullingRadius> m_ToTurnOff = new List<ForcedCullingRadius>();

		public UncullEverythingScope(ForcedCullingService service)
		{
			foreach (ForcedCullingRadius item in service.m_Statics.Concat(service.m_Dynamics))
			{
				if (!service.m_CullingGroup.IsVisible(item.SphereIndex))
				{
					m_ToTurnOff.Add(item);
				}
				item.SetVisible(visible: true);
			}
		}

		public void Dispose()
		{
			foreach (ForcedCullingRadius item in m_ToTurnOff)
			{
				item.SetVisible(visible: false);
			}
		}
	}

	private static ServiceProxy<ForcedCullingService> s_Proxy;

	private readonly List<ForcedCullingRadius> m_Statics = new List<ForcedCullingRadius>();

	private readonly List<ForcedCullingRadius> m_Dynamics = new List<ForcedCullingRadius>();

	private readonly List<ForcedCullingRadius> m_JustAdded = new List<ForcedCullingRadius>();

	private BoundingSphere[] m_Spheres = new BoundingSphere[512];

	private ForcedCullingRadius[] m_SphereToObject = new ForcedCullingRadius[512];

	private int m_TotalCount;

	private readonly Stack<int> m_ArrayHoles = new Stack<int>();

	private CullingGroup m_CullingGroup = new CullingGroup();

	private bool m_IsDirty;

	private bool m_CheatDisabled;

	private Camera m_CullingCamera;

	public static ForcedCullingService Instance
	{
		get
		{
			if (s_Proxy == null)
			{
				Services.RegisterServiceInstance(new ForcedCullingService());
			}
			s_Proxy = ((s_Proxy?.Instance != null) ? s_Proxy : Services.GetProxy<ForcedCullingService>());
			return s_Proxy?.Instance;
		}
	}

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	[Cheat(Name = "forced_culling_disabled")]
	public static bool CheatDisabled
	{
		get
		{
			return Instance.m_CheatDisabled;
		}
		set
		{
			Instance.m_CheatDisabled = value;
			ForcedCullingRadius[] sphereToObject = Instance.m_SphereToObject;
			foreach (ForcedCullingRadius forcedCullingRadius in sphereToObject)
			{
				if ((bool)forcedCullingRadius)
				{
					forcedCullingRadius.SetVisible(value || Instance.m_CullingGroup.IsVisible(forcedCullingRadius.SphereIndex));
				}
			}
		}
	}

	private ForcedCullingService()
	{
		CullingGroup cullingGroup = m_CullingGroup;
		cullingGroup.onStateChanged = (CullingGroup.StateChanged)Delegate.Combine(cullingGroup.onStateChanged, new CullingGroup.StateChanged(OnCullingStateChange));
		Enable();
	}

	public void DoUpdate()
	{
		if (m_CheatDisabled)
		{
			return;
		}
		if (m_CullingCamera != Game.GetCamera())
		{
			m_CullingCamera = Game.GetCamera();
			m_IsDirty = true;
		}
		if (m_IsDirty)
		{
			m_IsDirty = false;
			if (m_ArrayHoles.Count > m_Spheres.Length / 3)
			{
				m_TotalCount = 0;
				foreach (ForcedCullingRadius @static in m_Statics)
				{
					@static.ChangeIndex(m_TotalCount++, m_Spheres);
					m_SphereToObject[@static.SphereIndex] = @static;
				}
				foreach (ForcedCullingRadius dynamic in m_Dynamics)
				{
					dynamic.ChangeIndex(m_TotalCount++, m_Spheres);
					m_SphereToObject[dynamic.SphereIndex] = dynamic;
				}
				m_ArrayHoles.Clear();
			}
			if (m_CullingCamera == null)
			{
				ForcedCullingRadius[] sphereToObject = Instance.m_SphereToObject;
				foreach (ForcedCullingRadius forcedCullingRadius in sphereToObject)
				{
					if ((bool)forcedCullingRadius)
					{
						forcedCullingRadius.SetVisible(visible: true);
					}
				}
				return;
			}
			m_CullingGroup.targetCamera = m_CullingCamera;
			m_CullingGroup.SetBoundingSpheres(m_Spheres);
			m_CullingGroup.SetBoundingSphereCount(m_TotalCount);
			foreach (ForcedCullingRadius item in m_JustAdded)
			{
				if (item != null)
				{
					item.SetVisible(m_CullingGroup.IsVisible(item.SphereIndex));
				}
			}
			m_JustAdded.Clear();
			return;
		}
		foreach (ForcedCullingRadius dynamic2 in m_Dynamics)
		{
			dynamic2.UpdateBounds(m_Spheres);
		}
	}

	public void Add(ForcedCullingRadius fcr)
	{
		(fcr.Static ? m_Statics : m_Dynamics).Add(fcr);
		int num = ((m_ArrayHoles.Count > 0) ? m_ArrayHoles.Pop() : m_TotalCount++);
		if (num >= m_Spheres.Length)
		{
			Array.Resize(ref m_Spheres, m_Spheres.Length * 2);
			Array.Resize(ref m_SphereToObject, m_Spheres.Length * 2);
		}
		m_SphereToObject[num] = fcr;
		fcr.Init(num);
		fcr.UpdateBounds(m_Spheres);
		m_JustAdded.Add(fcr);
		m_IsDirty = true;
	}

	public void Remove(ForcedCullingRadius fcr)
	{
		(fcr.Static ? m_Statics : m_Dynamics).Remove(fcr);
		m_ArrayHoles.Push(fcr.SphereIndex);
		m_SphereToObject[fcr.SphereIndex] = null;
		m_JustAdded.Remove(fcr);
		m_IsDirty = true;
	}

	public void Move(ForcedCullingRadius fcr)
	{
		List<ForcedCullingRadius> list = (fcr.Static ? m_Dynamics : m_Statics);
		List<ForcedCullingRadius> list2 = (fcr.Static ? m_Statics : m_Dynamics);
		int num = list.IndexOf(fcr);
		if (num >= 0)
		{
			list.RemoveAt(num);
			list2.Add(fcr);
		}
	}

	private void OnCullingStateChange(CullingGroupEvent sphere)
	{
		if (!m_CheatDisabled)
		{
			m_SphereToObject[sphere.index]?.SetVisible(sphere.isVisible);
		}
	}

	public void Dispose()
	{
		m_CullingGroup.Dispose();
	}

	public BoundingSphere GetSphere(int ind)
	{
		return m_Spheres[ind];
	}

	public IDisposable UncullEverything()
	{
		return new UncullEverythingScope(this);
	}

	public bool IsVisible(int sphereIndex)
	{
		return Instance.m_CullingGroup.IsVisible(sphereIndex);
	}
}
