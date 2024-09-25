using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Owlcat.Core.Overlays;

public class OverlayService : IService, IDisposable
{
	private class OverlayDrivingBehaviour : MonoBehaviour
	{
		private void OnGUI()
		{
			try
			{
				if (Event.current.type == EventType.Repaint)
				{
					Instance?.DrawCurrentOverlay();
				}
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
				Services.DisposeServiceInstance<OverlayService>();
			}
		}

		private void LateUpdate()
		{
			try
			{
				Instance?.SampleCurrentOverlay();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
				Services.DisposeServiceInstance<OverlayService>();
			}
		}

		private void OnApplicationQuit()
		{
			Instance?.SaveConfig();
		}
	}

	private static ServiceProxy<OverlayService> s_Proxy;

	private bool m_IsInitialized;

	private OverlayDrivingBehaviour m_Driver;

	private readonly List<Overlay> m_Overlays = new List<Overlay>();

	private Overlay m_CurrentOverlay;

	private ServiceConfig m_Config;

	public static OverlayService Instance
	{
		get
		{
			s_Proxy = ((s_Proxy?.Instance != null) ? s_Proxy : Services.GetProxy<OverlayService>());
			return s_Proxy?.Instance;
		}
	}

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public bool IsInitialized => m_IsInitialized;

	public bool DarkenBackground { get; set; }

	public Overlay Current => m_CurrentOverlay;

	internal IEnumerable<Overlay> All => m_Overlays;

	public static void EnsureOverlaysInitialized()
	{
		if (Instance == null)
		{
			Services.RegisterServiceInstance(new OverlayService());
		}
		Instance?.Initialize();
	}

	private void Initialize()
	{
		if (!IsInitialized)
		{
			m_Driver = new GameObject("[OverlayService]").AddComponent<OverlayDrivingBehaviour>();
			m_Driver.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			UnityEngine.Object.DontDestroyOnLoad(m_Driver);
			string @string = PlayerPrefs.GetString("owc_overlays_data");
			m_Config = JsonUtility.FromJson<ServiceConfig>(@string) ?? new ServiceConfig();
			DarkenBackground = m_Config.Darken;
			m_IsInitialized = true;
		}
	}

	public void RegisterOverlay(Overlay o)
	{
		int num = m_Overlays.FindIndex((Overlay old) => old.Name == o.Name);
		if (num != -1)
		{
			m_Overlays.RemoveAt(num);
		}
		m_Overlays.Add(o);
		if (m_Config.LastSelectedName == o.Name)
		{
			ToggleOverlay(o.Name);
		}
	}

	private void SetCurrent(Overlay o)
	{
		m_CurrentOverlay = o;
		m_Driver.enabled = m_CurrentOverlay != null;
		if (o == null)
		{
			return;
		}
		ServiceConfig.OverlayEntry overlayEntry = m_Config.OverlayData.FirstOrDefault((ServiceConfig.OverlayEntry d) => d.Name == o.Name);
		if (overlayEntry != null)
		{
			foreach (Graph graph in o.Graphs)
			{
				graph.Hidden = overlayEntry.HiddenGraphs.Contains(graph.Name);
			}
			m_Config.OverlayData.Remove(overlayEntry);
		}
		m_Config.LastSelectedName = "";
	}

	public Overlay Get(string name)
	{
		return m_Overlays.FirstOrDefault((Overlay o) => o.Name == name);
	}

	public void ToggleOverlay(string name)
	{
		Overlay overlay = Get(name);
		if (m_CurrentOverlay == overlay)
		{
			SetCurrent(null);
		}
		else
		{
			SetCurrent(overlay);
		}
	}

	public void Dispose()
	{
		if ((bool)m_Driver)
		{
			UnityEngine.Object.DestroyImmediate(m_Driver.gameObject);
		}
	}

	private void DrawCurrentOverlay()
	{
		m_CurrentOverlay?.Draw();
	}

	private void SampleCurrentOverlay()
	{
		m_CurrentOverlay?.SampleGraphs();
	}

	public void Next()
	{
		if (m_CurrentOverlay == null)
		{
			SetCurrent(m_Overlays.FirstOrDefault());
			return;
		}
		int index = m_Overlays.IndexOf(m_CurrentOverlay) + 1;
		SetCurrent(m_Overlays.ElementAtOrDefault(index));
	}

	private void SaveConfig()
	{
		string value = JsonUtility.ToJson(new ServiceConfig
		{
			LastSelectedName = (m_CurrentOverlay?.Name ?? ""),
			Darken = DarkenBackground,
			OverlayData = m_Overlays.Select((Overlay o) => new ServiceConfig.OverlayEntry
			{
				HiddenGraphs = (from g in o.Graphs
					where g.Hidden
					select g.Name).ToList(),
				Name = o.Name
			}).ToList()
		});
		PlayerPrefs.SetString("owc_overlays_data", value);
	}
}
