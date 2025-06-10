using System;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Core.Overlays;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kingmaker.Utility;

public class FPSCounter : MonoBehaviour
{
	[Range(1f, 60f)]
	public int updatesPerSecond = 10;

	[Tooltip("Offset to max system used memory to display warning. In megabytes.")]
	public int systemMemoryWarnOffsetMb = 700;

	public bool Clear;

	private int m_TickCount;

	private float m_DeltaTimeAccumulator;

	private float m_FPS;

	private int m_FPSChecksCount;

	private float m_FPSTotal;

	private float m_FPSMax;

	private float m_FPSMin = 100000000f;

	private float m_FPSMedian;

	private float m_MS;

	private float m_MSTotal;

	private float m_MSMax;

	private float m_MSMin = 100000000f;

	private float m_MSMedian;

	private int m_TotalUnits;

	private int m_CombatUnits;

	private long m_MaxSystemMemory;

	private void Start()
	{
		if (!BuildModeUtility.IsDevelopment)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		m_TickCount = 0;
		m_DeltaTimeAccumulator = 0f;
		m_FPS = 0f;
		Overlay o = new Overlay("FPS", new Label("FPS", () => m_FPS.ToString("0.00")), new Label("FPS MIN", () => m_FPSMin.ToString("0.00")), new Label("FPS MAX", () => m_FPSMax.ToString("0.00")), new Label("FPS MED", () => m_FPSMedian.ToString("0.00"))
		{
			AddSeparator = true
		}, new Label("MS", () => m_MS.ToString("0.00")), new Label("MS MIN", () => m_MSMin.ToString("0.00")), new Label("MS MAX", () => m_MSMax.ToString("0.00")), new Label("MS MED", () => m_MSMedian.ToString("0.00"))
		{
			AddSeparator = true
		}, new Label("GFX MEM", () => MemString(Profiler.GetAllocatedMemoryForGraphicsDriver())), new Label("Native MEM", () => MemString(Profiler.GetTotalAllocatedMemoryLong()) + " + " + MemString(Profiler.GetTotalUnusedReservedMemoryLong()) + " = " + MemString(Profiler.GetTotalReservedMemoryLong())), new Label("Script MEM", () => MemString(Profiler.GetMonoUsedSizeLong()) + " + " + MemString(Profiler.GetMonoHeapSizeLong() - Profiler.GetMonoUsedSizeLong()) + " = " + MemString(Profiler.GetMonoHeapSizeLong())), MemoryLabelWithLimit("SYSTEM MEM", delegate
		{
			MemoryUsageHelper.MemoryStatsProvider stats = MemoryUsageHelper.Stats;
			return (Current: stats.SystemMemoryUsed, Max: stats.SystemMemoryLimit, Peak: stats.SystemMemoryUsedPeak);
		}, systemMemoryWarnOffsetMb * 1024 * 1024), new Label("NODE INDEX", GetNodeUnderPointerInfo), new Label("PLAYER INPUT", GetPlayerInputLockStatus));
		OverlayService.Instance?.RegisterOverlay(o);
		m_MaxSystemMemory = MemoryUsageHelper.Stats.SystemMemoryLimit;
	}

	private Label MemoryLabelWithLimit(string name, Func<(long Current, long Max, long Peak)> valueGetter, int warnOffset, int errorOffset = 0)
	{
		return new Label(name, delegate
		{
			var (bytes, bytes2, num3) = valueGetter();
			return (num3 <= 0) ? (MemString(bytes) + " / " + MemString(bytes2)) : (MemString(bytes) + " (" + MemString(num3) + ") / " + MemString(bytes2));
		}, delegate
		{
			var (num, num2, _) = valueGetter();
			if (num >= num2 - errorOffset)
			{
				return Label.Severity.Error;
			}
			return (num >= num2 - warnOffset) ? Label.Severity.Warning : Label.Severity.Info;
		});
	}

	private string GetSystemUsedMemoryText()
	{
		return MemString(MemoryUsageHelper.Stats.SystemMemoryUsed) + " / " + MemString(m_MaxSystemMemory);
	}

	private Label.Severity GetSystemUsedMemorySeverity()
	{
		long systemMemoryUsed = MemoryUsageHelper.Stats.SystemMemoryUsed;
		if (systemMemoryUsed >= m_MaxSystemMemory)
		{
			return Label.Severity.Error;
		}
		if (systemMemoryUsed < m_MaxSystemMemory - systemMemoryWarnOffsetMb * 1024 * 1024)
		{
			return Label.Severity.Info;
		}
		return Label.Severity.Warning;
	}

	private string GetNodeUnderPointerInfo()
	{
		PointerController controller = Game.Instance.GetController<PointerController>();
		if (controller == null)
		{
			return "(-, -)";
		}
		CustomGridNodeBase nearestNodeXZUnwalkable = controller.WorldPosition.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable == null)
		{
			return "(-, -)";
		}
		return $"({nearestNodeXZUnwalkable.XCoordinateInGrid}, {nearestNodeXZUnwalkable.ZCoordinateInGrid})";
	}

	private string GetPlayerInputLockStatus()
	{
		if (!Game.Instance.PlayerInputInCombatController.IsLocked)
		{
			return "";
		}
		return "LOCKED";
	}

	private static string MemString(long bytes)
	{
		return ((float)bytes / 1024f / 1024f).ToString("#.0");
	}

	private void Update()
	{
		m_TickCount++;
		m_DeltaTimeAccumulator += Time.unscaledDeltaTime;
		if (m_DeltaTimeAccumulator > 1f / (float)updatesPerSecond)
		{
			m_FPS = (float)m_TickCount / m_DeltaTimeAccumulator;
			m_TickCount = 0;
			m_DeltaTimeAccumulator -= 1f / (float)updatesPerSecond;
			m_MS = m_DeltaTimeAccumulator * 1000f;
			if (m_FPS < m_FPSMin)
			{
				m_FPSMin = m_FPS;
			}
			if (m_FPS > m_FPSMax)
			{
				m_FPSMax = m_FPS;
			}
			if (m_MS < m_MSMin)
			{
				m_MSMin = m_MS;
			}
			if (m_MS > m_MSMax)
			{
				m_MSMax = m_MS;
			}
			m_FPSTotal += m_FPS;
			m_MSTotal += m_MS;
			if (m_FPSChecksCount > 0)
			{
				m_FPSMedian = m_FPSTotal / (float)m_FPSChecksCount;
				m_MSMedian = m_MSTotal / (float)m_FPSChecksCount;
			}
			if (Clear)
			{
				m_FPSMax = 0f;
				m_FPSMin = 100000000f;
				m_MSMax = 0f;
				m_MSMin = 100000000f;
				m_FPSChecksCount = 0;
				m_FPSTotal = 0f;
				m_MSTotal = 0f;
				Clear = false;
			}
			m_FPSChecksCount++;
			m_TotalUnits = 0;
			m_CombatUnits = 0;
			foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
			{
				m_TotalUnits++;
				if (allUnit.IsInCombat)
				{
					m_CombatUnits++;
				}
			}
		}
		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
		{
			Clear = true;
		}
		if (Input.GetKeyDown(KeyCode.F11))
		{
			OverlayService.Instance?.Next();
		}
	}
}
