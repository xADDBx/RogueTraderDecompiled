using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Utility.Performance;

public static class ObjectLimits
{
	public class Entry
	{
		public readonly Func<int> Getter;

		public readonly string Name;

		public readonly int Threshold;

		public Entry(string name, int threshold, [NotNull] Func<int> getter)
		{
			Getter = getter;
			Name = name;
			Threshold = threshold;
		}

		public static implicit operator Entry((string, int, Func<int>) value)
		{
			return new Entry(value.Item1, value.Item2, value.Item3);
		}
	}

	public static readonly Entry[] Entries = new Entry[9]
	{
		("TOTAL UNITS", 200, (Func<int>)(() => State.AllUnits.Count())),
		("NORMAL UNITS", 75, (Func<int>)(() => State.AllUnits.Count((AbstractUnitEntity i) => i is BaseUnitEntity && !i.IsExtra))),
		("EXTRA UNITS", 125, (Func<int>)(() => State.AllUnits.Count((AbstractUnitEntity i) => i is LightweightUnitEntity || i.IsExtra))),
		("AWAKE UNITS", 45, (Func<int>)(() => State.AllAwakeUnits.Count)),
		("COMBAT GROUPS", 10, (Func<int>)(() => Game.Instance.ReadyForCombatUnitGroups.Count)),
		("COMBAT UNITS", 35, (Func<int>)(() => State.AllUnits.Count((AbstractUnitEntity i) => i.IsInCombat))),
		("MAP OBJECTS", 100, (Func<int>)(() => State.MapObjects.Count((MapObjectEntity i) => !(i is ScriptZoneEntity)))),
		("SCRIPT ZONES", 50, (Func<int>)(() => State.ScriptZones.Count() + State.AreaEffects.Count())),
		("AREA EFFECTS", 25, (Func<int>)(() => State.AreaEffects.Count()))
	};

	private static PersistentState State => Game.Instance.State;
}
