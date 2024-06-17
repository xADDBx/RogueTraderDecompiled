using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Alignments;

public static class SoulMarkShiftExtension
{
	public static void ApplyShiftDialog(this ISoulMarkShiftProvider provider)
	{
		ApplyShift(provider.SoulMarkShift, provider as BlueprintScriptableObject);
		if (provider is BlueprintAnswer || provider is BlueprintCue)
		{
			Game.Instance.DialogController.SoulMarkShifts.Add(provider.SoulMarkShift);
			EventBus.RaiseEvent(delegate(ISoulMarkShiftHandler h)
			{
				h.HandleSoulMarkShift(provider);
			});
		}
	}

	public static void ApplyShift(SoulMarkShift shift, BlueprintScriptableObject source)
	{
		if (shift.Value == 0 || shift.Direction == SoulMarkDirection.None)
		{
			return;
		}
		SoulMark soulMark = GetSoulMark(shift.Direction);
		if (soulMark == null)
		{
			throw new Exception("No predefined soul marks on main character");
		}
		int value = shift.Value;
		if (source != null)
		{
			EntityFactSource item = new EntityFactSource(source, value);
			if (!soulMark.Sources.ToList().HasItem(item))
			{
				soulMark.AddSource(source, value);
				soulMark.AddRank(value);
			}
		}
	}

	public static bool CheckShiftAtLeast(SoulMarkShift shift)
	{
		if (shift.Direction == SoulMarkDirection.None)
		{
			return true;
		}
		SoulMark soulMark = GetSoulMark(shift.Direction);
		if (shift.CheckByValue)
		{
			if (shift.Value == 0)
			{
				return true;
			}
			if (soulMark != null)
			{
				return soulMark.Rank >= shift.Value;
			}
			return false;
		}
		if (shift.CheckByRank)
		{
			if (shift.Rank == 0)
			{
				return true;
			}
			if (soulMark != null)
			{
				return GetSoulMarkRankIndex(shift.Direction, soulMark.Rank) >= shift.Rank;
			}
			return false;
		}
		return true;
	}

	public static SoulMark GetSoulMark(SoulMarkDirection direction)
	{
		return GetSoulMarkFor(Game.Instance.Player.MainCharacterEntity, direction);
	}

	public static SoulMark GetSoulMarkFor(BaseUnitEntity unit, SoulMarkDirection direction)
	{
		BlueprintSoulMark baseFactBlueprint = GetBaseSoulMarkFor(direction);
		if (baseFactBlueprint == null)
		{
			PFLog.Default.Error($"Base soulMark fact in blueprint root is missing for {direction}");
			return null;
		}
		SoulMark soulMark = unit.GetSoulMarks().FirstOrDefault((SoulMark sm) => sm.Blueprint == baseFactBlueprint);
		if (soulMark == null)
		{
			PFLog.Default.Error($"Base soulMark fact is missing for {direction} on {unit.Name}");
			return null;
		}
		return soulMark;
	}

	public static BlueprintSoulMark GetBaseSoulMarkFor(SoulMarkDirection direction)
	{
		BlueprintSoulMark obj = BlueprintRoot.Instance.WarhammerRoot.SoulMarksRoot.SoulMarksBaseFacts.FirstOrDefault((SoulMarkToFact sm) => sm.SoulMarkDirection == direction)?.SoulMarkBlueprint;
		if (obj == null)
		{
			PFLog.Default.Error($"Base soulMark fact in blueprint root is missing for {direction}");
		}
		return obj;
	}

	public static int GetSoulMarkRankIndex(SoulMarkDirection direction, int fillValue)
	{
		BlueprintSoulMark blueprintSoulMark = BlueprintRoot.Instance.WarhammerRoot.SoulMarksRoot.SoulMarksBaseFacts.FirstOrDefault((SoulMarkToFact sm) => sm.SoulMarkDirection == direction)?.SoulMarkBlueprint;
		if (blueprintSoulMark == null)
		{
			PFLog.Default.Error($"Base soulMark fact in blueprint root is missing for {direction}");
			return -1;
		}
		List<int> list = blueprintSoulMark.ComponentsArray.Select((BlueprintComponent c) => (c as RankChangedTrigger)?.RankValue.Value ?? (-1)).ToList();
		list.RemoveAll((int r) => r < 0);
		list.Sort((int r0, int r1) => (r0 >= r1) ? 1 : (-1));
		list.Insert(0, 0);
		return list.IndexOf(list.LastOrDefault((int r) => fillValue >= r));
	}

	public static SoulMarkDirection? GetSoulMarkDirection(this SoulMark soulMark)
	{
		return BlueprintRoot.Instance.WarhammerRoot.SoulMarksRoot.SoulMarksBaseFacts.FirstOrDefault((SoulMarkToFact sm) => sm.SoulMarkBlueprint == soulMark.Blueprint)?.SoulMarkDirection;
	}

	private static List<SoulMarkShift> AppliedShifts(SoulMarkDirection direction)
	{
		SoulMark soulMark = GetSoulMark(direction);
		List<SoulMarkShift> list = new List<SoulMarkShift>();
		if (soulMark == null)
		{
			return list;
		}
		foreach (EntityFactSource source in soulMark.Sources)
		{
			if (source.Blueprint is ISoulMarkShiftProvider soulMarkShiftProvider)
			{
				list.Add(soulMarkShiftProvider.SoulMarkShift);
			}
		}
		return list;
	}

	public static List<SoulMarkShift> AppliedShifts()
	{
		List<SoulMarkShift> first = AppliedShifts(SoulMarkDirection.Hope);
		List<SoulMarkShift> second = AppliedShifts(SoulMarkDirection.Faith);
		return Enumerable.Concat(second: AppliedShifts(SoulMarkDirection.Corruption), first: first.Concat(second)).ToList();
	}
}
