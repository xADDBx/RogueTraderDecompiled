using System.Text;
using Owlcat.Runtime.Visual.IndirectRendering;
using UnityEngine;
using UnityEngine.Profiling;

namespace Owlcat.Runtime.Visual.RenderPipeline.Debugging;

public class RuntimeProfiler : MonoBehaviour
{
	internal class RecorderEntry
	{
		public string Name;

		public float Time;

		public int Count;

		public float AvgTime;

		public float AvgCount;

		public float AccTime;

		public int AccCount;

		public Recorder Recorder;
	}

	private const int k_AverageFrameCount = 64;

	private int m_frameCount;

	private float m_AccDeltaTime;

	private float m_AvgDeltaTime;

	private StringBuilder m_StringBuilder = new StringBuilder();

	[SerializeField]
	private bool m_ShowOnlyFps;

	private RecorderEntry[] m_RecordersList = new RecorderEntry[5]
	{
		new RecorderEntry
		{
			Name = "RenderLoop.Draw"
		},
		new RecorderEntry
		{
			Name = "Shadows.Draw"
		},
		new RecorderEntry
		{
			Name = "RenderLoopNewBatcher.Draw"
		},
		new RecorderEntry
		{
			Name = "ShadowLoopNewBatcher.Draw"
		},
		new RecorderEntry
		{
			Name = "RenderLoopDevice.Idle"
		}
	};

	private void Awake()
	{
		for (int i = 0; i < m_RecordersList.Length; i++)
		{
			Sampler sampler = Sampler.Get(m_RecordersList[i].Name);
			if (sampler != null)
			{
				m_RecordersList[i].Recorder = sampler.GetRecorder();
			}
		}
	}

	private void Update()
	{
		for (int i = 0; i < m_RecordersList.Length; i++)
		{
			m_RecordersList[i].Time = (float)m_RecordersList[i].Recorder.elapsedNanoseconds / 1000000f;
			m_RecordersList[i].Count = m_RecordersList[i].Recorder.sampleBlockCount;
			m_RecordersList[i].AccTime += m_RecordersList[i].Time;
			m_RecordersList[i].AccCount += m_RecordersList[i].Count;
		}
		m_AccDeltaTime += Time.deltaTime;
		m_frameCount++;
		if (m_frameCount >= 64)
		{
			for (int j = 0; j < m_RecordersList.Length; j++)
			{
				m_RecordersList[j].AvgTime = m_RecordersList[j].AccTime * (1f / 64f);
				m_RecordersList[j].AvgCount = (float)m_RecordersList[j].AccCount * (1f / 64f);
				m_RecordersList[j].AccTime = 0f;
				m_RecordersList[j].AccCount = 0;
			}
			m_AvgDeltaTime = m_AccDeltaTime / 64f;
			m_AccDeltaTime = 0f;
			m_frameCount = 0;
		}
	}

	private void OnGUI()
	{
		GUI.skin.label.fontSize = 17;
		GUI.color = new Color(1f, 1f, 1f, 1f);
		float width = 800f;
		float height = 24 + (m_RecordersList.Length + 10) * 18 + 8 + 200;
		if (m_ShowOnlyFps)
		{
			width = 200f;
			height = 60f;
		}
		GUILayout.BeginArea(new Rect(32f, 50f, width, height), "Runtime Profiler", GUI.skin.window);
		string text = $"<b>{1f / m_AvgDeltaTime:F2} FPS ({Time.deltaTime * 1000f:F2}ms)</b>\n";
		if (!m_ShowOnlyFps)
		{
			for (int i = 0; i < m_RecordersList.Length; i++)
			{
				text += $"{m_RecordersList[i].AvgTime:F2}ms (*{m_RecordersList[i].AvgCount:F2})\t({m_RecordersList[i].Time:F2}ms *{m_RecordersList[i].Count:F2})\t<b>{m_RecordersList[i].Name}</b>\n";
			}
		}
		GUILayout.Label(text);
		if (!m_ShowOnlyFps)
		{
			m_StringBuilder.Clear();
			m_StringBuilder.AppendLine($"Gfx device type: {SystemInfo.graphicsDeviceType}");
			m_StringBuilder.AppendLine("Gfx device name: " + SystemInfo.graphicsDeviceName);
			m_StringBuilder.AppendLine($"Color space: {QualitySettings.activeColorSpace}");
			m_StringBuilder.AppendLine($"Allocated Mem For GfxDriver {Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024 / 1024}");
			m_StringBuilder.AppendLine($"Total Allocated Mem {Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024}");
			m_StringBuilder.AppendLine($"Total Reserved Mem {Profiler.GetTotalReservedMemoryLong() / 1024 / 1024}");
			m_StringBuilder.AppendLine($"Total Unused Reserved Mem {Profiler.GetTotalUnusedReservedMemoryLong() / 1024 / 1024}");
			m_StringBuilder.AppendLine();
			IndirectRenderingSystem.Instance.GetStats().BuildString(m_StringBuilder);
			GUILayout.BeginHorizontal();
			GUILayout.Label(m_StringBuilder.ToString());
			GUILayout.EndHorizontal();
		}
		GUILayout.EndArea();
	}
}
