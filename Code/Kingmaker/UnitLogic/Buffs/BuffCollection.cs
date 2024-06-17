using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class BuffCollection : MechanicEntityFactsCollection<Buff>
{
	public class RemoveByRank : ContextData<RemoveByRank>
	{
		protected override void Reset()
		{
		}
	}

	private bool m_Disabled;

	public AbstractUnitEntity Owner => base.Manager?.Owner as AbstractUnitEntity;

	public IEnumerator<Buff> GetEnumerator()
	{
		return base.Enumerable.GetEnumerator();
	}

	[CanBeNull]
	public Buff Add(BlueprintBuff blueprint, MechanicEntity caster, BuffDuration duration)
	{
		return Add(blueprint, caster, null, duration);
	}

	[CanBeNull]
	public Buff Add(BlueprintBuff blueprint)
	{
		return Add(blueprint, null, null, null);
	}

	[CanBeNull]
	public Buff Add(BlueprintBuff blueprint, MechanicsContext parentContext, BuffDuration duration = default(BuffDuration))
	{
		return Add(blueprint, null, parentContext, duration);
	}

	[CanBeNull]
	public Buff Add(BlueprintBuff blueprint, BuffDuration duration)
	{
		return Add(blueprint, null, null, duration);
	}

	[CanBeNull]
	public Buff Add([CanBeNull] BlueprintBuff blueprint, MechanicEntity caster, MechanicsContext parentContext = null, BuffDuration duration = default(BuffDuration))
	{
		if (blueprint == null)
		{
			return null;
		}
		if (Owner is LightweightUnitEntity lightweightUnitEntity)
		{
			lightweightUnitEntity.PlayBuffFx(blueprint);
			return null;
		}
		if (caster == null)
		{
			caster = Owner;
		}
		MechanicsContext context = parentContext?.CloneFor(blueprint, Owner) ?? new MechanicsContext(caster, Owner, blueprint);
		return base.Manager.Add(new Buff(blueprint, context, duration));
	}

	protected override Buff PrepareFactForAttach(Buff fact)
	{
		if (m_Disabled)
		{
			return null;
		}
		BlueprintBuff blueprint = fact.Blueprint;
		if (!blueprint.StayOnDeath && !(Owner is BaseUnitEntity { State: null }) && Owner.LifeState.IsDead)
		{
			return null;
		}
		MechanicsContext context = fact.Context;
		RuleCalculateCanApplyBuff ruleCalculateCanApplyBuff = Rulebook.Trigger(new RuleCalculateCanApplyBuff(Owner, context, fact));
		if (!ruleCalculateCanApplyBuff.CanApply)
		{
			return null;
		}
		BuffDuration duration = ruleCalculateCanApplyBuff.Duration;
		if (blueprint.Stacking != StackingType.Stack)
		{
			Buff buff = GetBuff(blueprint);
			if (buff != null)
			{
				switch (blueprint.Stacking)
				{
				case StackingType.Replace:
					base.Manager.Remove(buff);
					break;
				case StackingType.Prolong:
					buff.Prolong(duration.Rounds);
					return buff;
				case StackingType.Ignore:
					return buff;
				case StackingType.Poison:
					if (duration.IsPermanent)
					{
						buff.MakePermanent();
					}
					else
					{
						Rounds value = duration.Rounds.Value / 2;
						buff.IncreaseDuration(value);
					}
					return buff;
				case StackingType.Summ:
					buff.IncreaseDuration(duration.Rounds);
					return buff;
				case StackingType.Rank:
					OnBuffRankAdd(buff);
					return buff;
				case StackingType.HighestByProperty:
					if (context[blueprint.PriorityProperty] >= buff.Context[blueprint.PriorityProperty])
					{
						base.Manager.Remove(buff);
						break;
					}
					return buff;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		fact.SetDuration(duration);
		return fact;
	}

	protected override Buff PrepareFactForDetach(Buff fact)
	{
		if (!ContextData<RemoveByRank>.Current || fact.GetRank() <= 1)
		{
			return fact;
		}
		BaseUnitEntity baseUnitEntity = ContextData<CasterUnitData>.Current?.Unit;
		if (baseUnitEntity != null && fact.Context.MaybeCaster != baseUnitEntity)
		{
			return null;
		}
		fact.RemoveRank();
		return null;
	}

	protected override void OnFactDidAttach(Buff fact)
	{
		base.OnFactDidAttach(fact);
		EventBus.RaiseEvent((IBaseUnitEntity)fact.Owner, (Action<IUnitBuffHandler>)delegate(IUnitBuffHandler h)
		{
			h.HandleBuffDidAdded(fact);
		}, isCheckRuntime: true);
	}

	protected override void OnFactWillDetach(Buff fact)
	{
		base.OnFactWillDetach(fact);
		fact.OnRemove();
		EventBus.RaiseEvent((IBaseUnitEntity)fact.Owner, (Action<IUnitBuffHandler>)delegate(IUnitBuffHandler h)
		{
			h.HandleBuffDidRemoved(fact);
		}, isCheckRuntime: true);
	}

	private void OnBuffRankAdd(Buff fact)
	{
		fact.AddRank();
	}

	public Buff GetBuff(BlueprintBuff blueprint)
	{
		foreach (Buff item in base.Enumerable)
		{
			if (item.Blueprint == blueprint)
			{
				return item;
			}
		}
		return null;
	}

	public void SetupPreview(BaseUnitEntity owner)
	{
		m_Disabled = true;
		foreach (Buff item in base.RawFacts.ToTempList())
		{
			item.Deactivate();
		}
	}

	public void SpawnBuffsFxs()
	{
		foreach (Buff rawFact in base.RawFacts)
		{
			rawFact.SpawnParticleEffect();
		}
	}

	public void RemoveBuffsOnResurrect()
	{
		List<Buff> list = TempList.Get<Buff>();
		foreach (Buff rawFact in base.RawFacts)
		{
			if (rawFact.Blueprint.RemoveOnResurrect)
			{
				list.Add(rawFact);
			}
		}
		foreach (Buff item in list)
		{
			Remove(item);
		}
	}

	public void OnCombatEnded()
	{
	}
}
