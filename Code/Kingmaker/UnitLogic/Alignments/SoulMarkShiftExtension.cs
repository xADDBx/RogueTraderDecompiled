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
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Alignments;

public static class SoulMarkShiftExtension
{
	private static readonly SoulMarkDirection[] AllDirections = (SoulMarkDirection[])Enum.GetValues(typeof(SoulMarkDirection));

	public static void ApplyShiftDialog(this ISoulMarkShiftProvider provider)
	{
		if (TryApplyShift(provider.SoulMarkShift, provider as BlueprintScriptableObject) && (provider is BlueprintAnswer || provider is BlueprintCue))
		{
			Game.Instance.DialogController.SoulMarkShifts.Add(provider.SoulMarkShift);
			EventBus.RaiseEvent(delegate(ISoulMarkShiftHandler h)
			{
				h.HandleSoulMarkShift(provider);
			});
		}
	}

	public static bool TryApplyShift(SoulMarkShift shift, BlueprintScriptableObject source)
	{
		if (shift.Value == 0 || shift.Direction == SoulMarkDirection.None)
		{
			return false;
		}
		SoulMark soulMark = GetSoulMark(shift.Direction);
		if (soulMark == null)
		{
			PFLog.Default.Error("No predefined soul marks on main character");
			return false;
		}
		int delayedSoulMarkValue = soulMark.Owner.GetDelayedSoulMarkValue(shift.Direction);
		int num = shift.Value + delayedSoulMarkValue;
		int blockingRankIndex = BlueprintRoot.Instance.WarhammerRoot.SoulMarksRoot.BlockingRankIndex;
		bool flag = false;
		SoulMarkDirection[] allDirections = AllDirections;
		foreach (SoulMarkDirection soulMarkDirection in allDirections)
		{
			if (soulMarkDirection != 0 && soulMarkDirection != shift.Direction)
			{
				SoulMark soulMark2 = GetSoulMark(soulMarkDirection);
				if (soulMark2 != null && GetSoulMarkRankIndex(soulMarkDirection, soulMark2.Rank) >= blockingRankIndex)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			if (GetSoulMarkRankIndex(shift.Direction, soulMark.Rank) >= blockingRankIndex)
			{
				return false;
			}
			List<int> rankThresholds = GetRankThresholds(shift.Direction);
			if (blockingRankIndex < rankThresholds.Count)
			{
				num = Math.Min(num, rankThresholds[blockingRankIndex] - 1 - soulMark.Rank);
				if (num <= 0)
				{
					return false;
				}
			}
		}
		if (source != null)
		{
			EntityFactSource item = new EntityFactSource(source, num);
			if (!soulMark.Sources.ToList().HasItem(item))
			{
				soulMark.AddSource(source, num);
				soulMark.AddRank(num);
				EventBus.RaiseEvent(delegate(ISoulMarkShiftRewardHandler h)
				{
					h.HandleSoulMarkShift(shift);
				});
				soulMark.Owner.FreeDelayedShift(shift.Direction);
				return true;
			}
		}
		return false;
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
		return GetSoulMarkFor(Game.Instance.Player.MainCharacterOriginalEntity, direction);
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

	private static List<int> GetRankThresholds(SoulMarkDirection direction)
	{
		BlueprintSoulMark baseSoulMarkFor = GetBaseSoulMarkFor(direction);
		if (baseSoulMarkFor == null)
		{
			return TempList.Get<int>();
		}
		List<int> list = baseSoulMarkFor.ComponentsArray.Select((BlueprintComponent c) => (c as RankChangedTrigger)?.RankValue.Value ?? (-1)).ToTempList();
		list.RemoveAll((int r) => r < 0);
		list.Sort((int r0, int r1) => (r0 >= r1) ? 1 : (-1));
		list.Insert(0, 0);
		return list;
	}

	public static int GetSoulMarkRankIndex(SoulMarkDirection direction, int fillValue)
	{
		List<int> rankThresholds = GetRankThresholds(direction);
		return rankThresholds.IndexOf(rankThresholds.LastOrDefault((int r) => fillValue >= r));
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
