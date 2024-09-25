using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Utility.Performance;

public static class ObjectLimits
{
	public class Entry
	{
		public readonly Func<int> Getter;

		public readonly string Name;

		private readonly int m_Threshold;

		[CanBeNull]
		private readonly Func<int> m_ThresholdGetter;

		public int Threshold => m_ThresholdGetter?.Invoke() ?? m_Threshold;

		public Entry(string name, int threshold, [NotNull] Func<int> getter)
		{
			Getter = getter;
			Name = name;
			m_Threshold = threshold;
		}

		public Entry(string name, [NotNull] Func<int> thresholdGetter, [NotNull] Func<int> getter)
		{
			Getter = getter;
			Name = name;
			m_ThresholdGetter = thresholdGetter;
		}

		public static implicit operator Entry((string, int, Func<int>) value)
		{
			return new Entry(value.Item1, value.Item2, value.Item3);
		}

		public static implicit operator Entry((string, Func<int>, Func<int>) value)
		{
			return new Entry(value.Item1, value.Item2, value.Item3);
		}
	}

	private const int TotalUnitsThreshold = 200;

	private const int NormalUnitsThreshold = 75;

	public static readonly Entry[] Entries = new Entry[13]
	{
		("TOTAL UNITS", 200, (Func<int>)(() => State.AllUnits.Count())),
		("NORMAL UNITS", 75, (Func<int>)GetNormalUnitsCount),
		("EXTRA UNITS", (Func<int>)GetExtraUnitsThreshold, (Func<int>)GetExtraUnitsCount),
		("AWAKE UNITS", 45, (Func<int>)(() => State.AllAwakeUnits.Count)),
		("COMBAT GROUPS", 10, (Func<int>)(() => Game.Instance.ReadyForCombatUnitGroups.Count)),
		("COMBAT UNITS", 35, (Func<int>)(() => State.AllUnits.Count((AbstractUnitEntity i) => i.IsInCombat))),
		("MAP OBJECTS", 100, (Func<int>)(() => State.MapObjects.Count((MapObjectEntity i) => !(i is ScriptZoneEntity)))),
		("SCRIPT ZONES", 50, (Func<int>)(() => State.ScriptZones.Count() + State.AreaEffects.Count())),
		("AREA EFFECTS", 25, (Func<int>)(() => State.AreaEffects.Count())),
		("AREA CR", 1000, (Func<int>)(() => Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0)),
		("AREA CR OVERRIDES", 1, (Func<int>)(() => GetAreaCROverridesStatus())),
		("CROWD UNITS", 1000, (Func<int>)(() => Game.Instance.GpuCrowdController.CountAllCrowdsUnits(withShadows: false))),
		("CROWD UNITS W/SHADOWS", 500, (Func<int>)(() => Game.Instance.GpuCrowdController.CountAllCrowdsUnits(withShadows: true)))
	};

	private static PersistentState State => Game.Instance.State;

	private static int GetAreaCROverridesStatus()
	{
		Player player = Game.Instance.Player;
		AreaCROverrideManager areaCROverrideManager = ((player != null && player.IsInitialized) ? player.AreaCROverrideManager : null);
		if (areaCROverrideManager == null || Game.Instance.CurrentlyLoadedArea == null || !areaCROverrideManager.Contains(Game.Instance.CurrentlyLoadedArea.AssetGuid))
		{
			return 0;
		}
		return 1;
	}

	private static int GetNormalUnitsCount()
	{
		return State.AllUnits.Count((AbstractUnitEntity i) => i is BaseUnitEntity && !i.IsExtra);
	}

	private static int GetExtraUnitsCount()
	{
		return State.AllUnits.Count((AbstractUnitEntity i) => i is LightweightUnitEntity || i.IsExtra);
	}

	private static int GetExtraUnitsThreshold()
	{
		return 200 - Math.Min(75, GetNormalUnitsCount());
	}
}
