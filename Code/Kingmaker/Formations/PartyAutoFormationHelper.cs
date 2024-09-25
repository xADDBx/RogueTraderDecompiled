using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Formations;

public static class PartyAutoFormationHelper
{
	private enum UnitFormationPlace
	{
		None,
		MainTank,
		OffTank,
		SecondLine,
		BackLine
	}

	private class UnitFormationData
	{
		public readonly BaseUnitEntity Unit;

		public readonly int FormationAC;

		public UnitFormationPlace Place { get; set; }

		public UnitFormationData(BaseUnitEntity unit)
		{
			Unit = unit;
			FormationAC = GetFormationAC(unit);
		}
	}

	private const int MaxDifferenceBetweenMainAndOffTankAC = 4;

	private const int MinDifferenceBetweenOffTankAndOtherAC = 6;

	private const int MinUnitsCountInSecondLineForOffTank = 2;

	private static float SpaceX => BlueprintRoot.Instance.Formations.AutoFormation.SpaceX;

	private static float SpaceY => BlueprintRoot.Instance.Formations.AutoFormation.SpaceY;

	public static void Setup(PartyFormationAuto formation)
	{
		formation.Clear();
		List<UnitFormationData> list = Game.Instance.Player.PartyAndPets.Select((BaseUnitEntity u) => new UnitFormationData(u)).ToTempList();
		UnitFormationData unitFormationData = null;
		foreach (UnitFormationData item in list)
		{
			item.Place = (IsBackLineUnit(item.Unit) ? UnitFormationPlace.BackLine : UnitFormationPlace.SecondLine);
			if (IsSuitableTank(item, unitFormationData))
			{
				unitFormationData = item;
			}
		}
		if (unitFormationData == null)
		{
			PFLog.Default.ErrorWithReport("PartyAutoFormationHelper.Setup: mainTank == null");
			return;
		}
		unitFormationData.Place = UnitFormationPlace.MainTank;
		formation.InvalidTank = unitFormationData.FormationAC < GetRecommendedTankAC() || IsBackLineUnit(unitFormationData.Unit);
		UnitFormationData unitFormationData2 = null;
		if (!formation.InvalidTank)
		{
			unitFormationData2 = TryFindOffTank(list, unitFormationData);
			if (unitFormationData2 != null)
			{
				unitFormationData2.Place = UnitFormationPlace.OffTank;
			}
		}
		List<UnitFormationData> secondLine = list.Where((UnitFormationData u) => u.Place == UnitFormationPlace.SecondLine).ToTempList();
		List<UnitFormationData> backLine = list.Where((UnitFormationData u) => u.Place == UnitFormationPlace.BackLine).ToTempList();
		SetupPositions(formation, unitFormationData, unitFormationData2, secondLine, backLine);
	}

	private static bool IsBackLineUnit(BaseUnitEntity unit)
	{
		ItemEntityWeapon maybeWeapon = unit.Body.CurrentHandsEquipmentSet.PrimaryHand.MaybeWeapon;
		if (maybeWeapon != null && maybeWeapon.Blueprint.IsRanged && 0 == 0)
		{
			return true;
		}
		return false;
	}

	private static bool IsSuitableTank(UnitFormationData candidate, [CanBeNull] UnitFormationData existingCandidate)
	{
		if (existingCandidate == null)
		{
			return true;
		}
		if (candidate.FormationAC > existingCandidate.FormationAC)
		{
			return true;
		}
		if (existingCandidate.Place == UnitFormationPlace.BackLine && candidate.Place != UnitFormationPlace.BackLine)
		{
			return true;
		}
		if (candidate.FormationAC == existingCandidate.FormationAC && candidate.FormationAC == existingCandidate.FormationAC && (int)candidate.Unit.Health.HitPoints > (int)existingCandidate.Unit.Health.HitPoints)
		{
			return true;
		}
		return false;
	}

	[CanBeNull]
	private static UnitFormationData TryFindOffTank(List<UnitFormationData> unitDataList, [NotNull] UnitFormationData mainTank)
	{
		UnitFormationData unitFormationData = null;
		int num = 0;
		foreach (UnitFormationData unitData in unitDataList)
		{
			if (unitData.Place == UnitFormationPlace.SecondLine)
			{
				num++;
				if (IsSuitableTank(unitData, unitFormationData))
				{
					unitFormationData = unitData;
				}
			}
		}
		if (unitFormationData == null || num < 2 || unitFormationData.FormationAC < GetRecommendedTankAC())
		{
			return null;
		}
		if (mainTank.FormationAC - unitFormationData.FormationAC <= 4)
		{
			return unitFormationData;
		}
		foreach (UnitFormationData unitData2 in unitDataList)
		{
			if (unitData2.Place == UnitFormationPlace.SecondLine && unitFormationData != unitData2 && unitFormationData.FormationAC - unitData2.FormationAC < 6)
			{
				return null;
			}
		}
		return unitFormationData;
	}

	private static int GetFormationAC(BaseUnitEntity unit)
	{
		return 0;
	}

	private static int GetRecommendedTankAC()
	{
		return 0;
	}

	private static void SetupPositions(PartyFormationAuto formation, UnitFormationData mainTank, [CanBeNull] UnitFormationData offTank, List<UnitFormationData> secondLine, List<UnitFormationData> backLine)
	{
		Vector2 zero = Vector2.zero;
		formation.SetOffset(mainTank.Unit, Vector2.zero);
		zero -= new Vector2(0f, SpaceY);
		if (offTank != null)
		{
			formation.SetOffset(offTank.Unit, zero);
		}
		zero -= new Vector2(0f, SpaceY);
		SetupLinePositions(formation, zero, secondLine);
		if (secondLine.Count > 0)
		{
			zero -= new Vector2(0f, SpaceY);
		}
		SetupLinePositions(formation, zero, backLine);
	}

	private static void SetupLinePositions(PartyFormationAuto formation, Vector2 center, List<UnitFormationData> line)
	{
		float num = (0f - SpaceX) / 2f * (float)(line.Count - 1);
		foreach (UnitFormationData item in line)
		{
			Vector2 pos = center + new Vector2(num, 0f);
			formation.SetOffset(item.Unit, pos);
			num += SpaceX;
		}
	}

	[UsedImplicitly]
	[Cheat(Name = "update_auto_formation", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void UpdateAutoFormation()
	{
		Game.Instance.Player.FormationManager.UpdateAutoFormation();
	}
}
