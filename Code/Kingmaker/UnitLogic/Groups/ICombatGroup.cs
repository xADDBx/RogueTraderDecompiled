using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;

namespace Kingmaker.UnitLogic.Groups;

public interface ICombatGroup
{
	string Id { get; }

	UnitGroupMemory Memory { get; }

	bool IsPlayerParty { get; }

	CountableFlag IsInCombat { get; }

	ReadonlyList<BlueprintFaction> AttackFactions { get; }

	bool IsInFogOfWar { get; }

	int Count { get; }

	BaseUnitEntity this[int index] { get; }

	bool IsEnemy(UnitGroup group);

	bool IsEnemy(MechanicEntity unit);

	bool Any(Func<BaseUnitEntity, bool> pred);

	bool All(Func<BaseUnitEntity, bool> pred);

	IEnumerable<T> Select<T>(Func<BaseUnitEntity, T> select);

	void ForEach(Action<BaseUnitEntity> action);

	bool HasLOS(BaseUnitEntity unit);

	void UpdateAttackFactionsCache();

	void ResetFactionSet();

	UnitGroupEnumerator GetEnumerator();
}
