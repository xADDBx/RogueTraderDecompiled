using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("4676b57b7542493c80f1c2e7a1e6facf")]
public class SummonUnitsOnDeath : UnitFactComponentDelegate, IUnitDieHandler<EntitySubscriber>, IUnitDieHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitDieHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintUnit.Reference[] m_Summons = new BlueprintUnit.Reference[0];

	public bool AddToOwnersSummonPool = true;

	public ActionList SummonActions;

	public ReferenceArrayProxy<BlueprintUnit> Summons
	{
		get
		{
			BlueprintReference<BlueprintUnit>[] summons = m_Summons;
			return summons;
		}
	}

	public void OnUnitDie()
	{
		if (base.Owner.LifeState.IsManualDeath)
		{
			return;
		}
		List<BlueprintSummonPool> summonPools = null;
		if (AddToOwnersSummonPool)
		{
			summonPools = Game.Instance.SummonPools.GetPoolsForUnit(base.Owner).ToTempList();
		}
		foreach (BlueprintUnit summon in Summons)
		{
			Summon(summon, summonPools);
		}
	}

	private void Summon(BlueprintUnit blueprint, List<BlueprintSummonPool> summonPools)
	{
		BaseUnitEntity baseUnitEntity = Game.Instance.EntitySpawner.SpawnUnit(blueprint, base.Owner.Position, Quaternion.Euler(0f, base.Owner.Orientation, 0f), base.Owner.HoldingState);
		baseUnitEntity.CombatGroup.Id = base.Owner.CombatGroup.Id;
		baseUnitEntity.SnapToGrid();
		baseUnitEntity.GiveExperienceOnDeath = base.Owner.GiveExperienceOnDeath;
		if (base.Owner.SpawnFromPsychicPhenomena)
		{
			baseUnitEntity.MarkSpawnFromPsychicPhenomena();
		}
		if (AddToOwnersSummonPool)
		{
			foreach (BlueprintSummonPool summonPool in summonPools)
			{
				Game.Instance.SummonPools.Register(summonPool, baseUnitEntity);
			}
		}
		if (base.Owner.IsInCombat)
		{
			UnitCombatJoinController controller = Game.Instance.GetController<UnitCombatJoinController>(includeInactive: true);
			foreach (UnitGroupMemory.UnitInfo enemy in base.Owner.CombatGroup.Memory.Enemies)
			{
				controller?.StartScriptedCombat(baseUnitEntity, enemy.Unit);
			}
		}
		base.Fact.RunActionInContext(SummonActions, baseUnitEntity.ToITargetWrapper());
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
