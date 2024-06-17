using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Starships;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Visual.FX;
using Kingmaker.Visual.Particles;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("fb8a35db69c8bee4eb229e8802ffbeff")]
public class StarshipHoloFieldController : UnitFactComponentDelegate, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitCombatHandler, EntitySubscriber>, ITargetRulebookHandler<RuleStarshipRollAttack>, IRulebookHandler<RuleStarshipRollAttack>, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleStarshipPerformAttack>, IRulebookHandler<RuleStarshipPerformAttack>, IUnitDeathHandler, IDamageHandler, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public GameObject[] fxObjects;

		public int buffRank;

		public bool wasAttackedLastRound;
	}

	[SerializeField]
	private BlueprintBuffReference m_HoloFieldBuff;

	[SerializeField]
	private int restoreChargesPerTurn;

	[SerializeField]
	private bool dontRestoreChargesIfAttackedLastTurn;

	[SerializeField]
	private BlueprintAbilityFXSettings.Reference m_FXSettings;

	public BlueprintBuff HoloFieldBuff => m_HoloFieldBuff?.Get();

	private BlueprintAbilityFXSettings FXSettings => m_FXSettings;

	private Buff GetHoloBuff => base.Owner.Buffs.GetBuff(HoloFieldBuff);

	private VisualFXSettings[] PrefabList => FXSettings.VisualFXSettings.MechanicalEvents[0].Settings.FXs;

	private int MaxFXRank => PrefabList.Length / 2 - 1 - 1;

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (!isTurnBased || EventInvokerExtensions.MechanicEntity != base.Owner)
		{
			return;
		}
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (!dontRestoreChargesIfAttackedLastTurn || !componentData.wasAttackedLastRound)
		{
			Buff getHoloBuff = GetHoloBuff;
			if (getHoloBuff != null)
			{
				getHoloBuff.AddRank(restoreChargesPerTurn);
			}
			else
			{
				HoloOn(restoreChargesPerTurn);
			}
			UpdateFX();
		}
		componentData.wasAttackedLastRound = false;
	}

	public void HandleUnitJoinCombat()
	{
		HoloOn(HoloFieldBuff.MaxRank);
	}

	public void HandleUnitLeaveCombat()
	{
	}

	public void OnEventAboutToTrigger(RuleStarshipPerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RuleStarshipPerformAttack evt)
	{
		if (evt.Weapon.IsAEAmmo)
		{
			return;
		}
		if (dontRestoreChargesIfAttackedLastTurn)
		{
			RequestTransientData<ComponentData>().wasAttackedLastRound = true;
		}
		if (!evt.Result.IsHit() && evt.AttackRollRule.ResultTargetDisruptionMiss)
		{
			Buff getHoloBuff = GetHoloBuff;
			if (getHoloBuff != null)
			{
				int count = Mathf.RoundToInt(1f / SpacecombatDifficultyHelper.StarshipAvoidanceMod(evt.Target));
				getHoloBuff.RemoveRank(count);
				UpdateFX();
			}
		}
	}

	public void OnEventAboutToTrigger(RuleStarshipRollAttack evt)
	{
		Buff getHoloBuff = GetHoloBuff;
		if (getHoloBuff != null && !evt.Weapon.IsAEAmmo)
		{
			evt.BonusTargetDisruptionChance += 100 - 100 / (getHoloBuff.Rank + 1);
		}
	}

	public void OnEventDidTrigger(RuleStarshipRollAttack evt)
	{
	}

	private void HoloOn(int ranks)
	{
		Buff buff = base.Owner.Buffs.Add(HoloFieldBuff, base.Fact.MaybeContext, new BuffDuration(null, BuffEndCondition.CombatEnd));
		if (buff != null)
		{
			buff.AddRank(ranks - 1);
			UpdateFX();
		}
	}

	protected override void OnViewDidAttach()
	{
		UpdateFX();
	}

	private void UpdateFX()
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		int buffRank = componentData.buffRank;
		int num = (componentData.buffRank = GetHoloBuff?.Rank ?? 0);
		if (num > buffRank)
		{
			if (buffRank == 0)
			{
				StartFxIndex(0);
				StartFxIndex(1);
			}
			for (int i = buffRank; i < Math.Min(num, MaxFXRank); i++)
			{
				StartFxIndex(i + 2);
			}
		}
		if (num < buffRank)
		{
			for (int j = num; j < Math.Min(buffRank, MaxFXRank); j++)
			{
				KillFxIndex(j + 2);
			}
			if (num == 0)
			{
				KillFxIndex(1);
				KillFxIndex(0);
			}
		}
	}

	private void StartFxIndex(int id)
	{
		ComponentData componentData;
		ComponentData componentData2 = (componentData = RequestTransientData<ComponentData>());
		if (componentData.fxObjects == null)
		{
			componentData.fxObjects = new GameObject[PrefabList.Length];
		}
		int num = id * 2;
		componentData2.fxObjects[id * 2] = FxHelper.SpawnFxOnEntity(PrefabList[num].Prefab?.Load(), base.Owner.View);
	}

	private void KillFxIndex(int id)
	{
		GameObject gameObject = RequestTransientData<ComponentData>().fxObjects[id * 2];
		if (id == 0)
		{
			FxHelper.SpawnFxOnEntity(PrefabList[1].Prefab?.Load(), base.Owner.View);
		}
		else
		{
			Transform transform = gameObject.transform;
			Transform transform2 = gameObject.transform.Find("_SHIP_COPY_FX_ANI");
			if ((object)transform2 != null)
			{
				Transform child = transform2.GetChild(0);
				if ((object)child != null)
				{
					Transform transform3 = child.Find("_END");
					if ((object)transform3 != null)
					{
						transform = transform3;
					}
				}
			}
			FxHelper.SpawnFxOnPoint(PrefabList[3].Prefab?.Load(), transform.position, transform.rotation);
		}
		FxHelper.Destroy(gameObject);
	}

	public void HandleUnitDeath(AbstractUnitEntity baseUnitEntity)
	{
		if (baseUnitEntity == base.Owner)
		{
			Buff getHoloBuff = GetHoloBuff;
			if (getHoloBuff != null)
			{
				getHoloBuff.Remove();
				UpdateFX();
			}
		}
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		UpdateFX();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
