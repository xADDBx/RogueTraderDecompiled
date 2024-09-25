using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.DialogSystem;

[Serializable]
public class CharacterSelection
{
	[Serializable]
	public enum Type
	{
		Clear,
		Keep,
		Random,
		Player,
		Manual,
		Companion,
		Capital
	}

	public Type SelectionType;

	public StatType[] ComparisonStats;

	[CanBeNull]
	public BaseUnitEntity SelectUnit(BlueprintAnswer answer, BaseUnitEntity manualSelection, bool forceManual = false)
	{
		List<BaseUnitEntity> party = Game.Instance.Player.Party;
		switch (SelectionType)
		{
		case Type.Clear:
		case Type.Capital:
			return null;
		case Type.Keep:
			return Game.Instance.DialogController.ActingUnit;
		case Type.Random:
		{
			BaseUnitEntity[] array = party.ToArray();
			return array[PFStatefulRandom.DialogSystem.Range(0, array.Length)];
		}
		case Type.Player:
			return Game.Instance.Player.MainCharacterEntity;
		case Type.Manual:
			if (forceManual)
			{
				return manualSelection;
			}
			if (ComparisonStats.Length >= 2 || ComparisonStats.Length == 0)
			{
				return manualSelection;
			}
			return Game.Instance.Player.Party.MaxBy((BaseUnitEntity c) => c.Stats.GetStat(ComparisonStats[0]));
		case Type.Companion:
		{
			BlueprintUnit blueprint = answer.GetComponent<ActingCompanion>()?.Companion;
			return Enumerable.FirstOrDefault(Game.Instance.Player.Party, (BaseUnitEntity u) => u.Blueprint == blueprint);
		}
		default:
			throw new NotImplementedException("Type " + SelectionType.ToString() + " is not supported");
		}
	}
}
