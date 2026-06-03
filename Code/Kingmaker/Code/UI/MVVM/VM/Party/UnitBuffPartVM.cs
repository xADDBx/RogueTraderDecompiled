using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine.Pool;

namespace Kingmaker.Code.UI.MVVM.VM.Party;

public class UnitBuffPartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IUnitBuffHandler<EntitySubscriber>, IEventTag<IUnitBuffHandler, EntitySubscriber>, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEntitySubscriber, IApplyAbilityEffectHandler, IUnitDeathHandler, IOnUnitChangeSetMarkBuffMap
{
	public ReactiveCommand<((BlueprintCareerPath, BuffVM), bool)> DrawUniqueBuff = new ReactiveCommand<((BlueprintCareerPath, BuffVM), bool)>();

	public ReactiveCommand<List<BuffVM>> RedrawBuffs = new ReactiveCommand<List<BuffVM>>();

	private IDisposable m_Subscription;

	private BlueprintCareerPath m_HunterCareerPath;

	public readonly ReactiveCollection<BuffVM> Buffs = new ReactiveCollection<BuffVM>();

	private readonly Dictionary<BlueprintBuff, BuffVM> m_CollapsedBuffs = new Dictionary<BlueprintBuff, BuffVM>();

	private Dictionary<MechanicEntity, BuffVM> m_HuntThePreyMarkTargetMap = new Dictionary<MechanicEntity, BuffVM>();

	public MechanicEntity Unit { get; private set; }

	public BlueprintCareerPath UnitCareerPath { get; private set; }

	public IEntity GetSubscribingEntity()
	{
		return Unit;
	}

	public bool EntityIsDeadOrUnconscious()
	{
		if (Unit != null && !Unit.IsDisposed)
		{
			return Unit.IsDeadOrUnconscious;
		}
		return true;
	}

	public UnitBuffPartVM(MechanicEntity unit)
	{
		if (unit != null)
		{
			SetUnitData(unit);
		}
		else
		{
			ClearBuffs();
		}
		m_HunterCareerPath = UIConfig.Instance.HunterCareerPath.Get();
	}

	protected override void DisposeImplementation()
	{
		m_Subscription?.Dispose();
		m_Subscription = null;
		ClearBuffs();
	}

	public void SetUnitData(MechanicEntity unit)
	{
		m_Subscription?.Dispose();
		Unit = unit;
		UnitCareerPath = (from f in Unit?.Facts.GetAll((Feature f) => f.Blueprint is BlueprintCareerPath blueprintCareerPath && blueprintCareerPath.Tier == CareerPathTier.Two)
			select f.Blueprint as BlueprintCareerPath).FirstOrDefault();
		if (unit != null)
		{
			m_Subscription = EventBus.Subscribe(this);
		}
		UpdateData();
	}

	private void ClearBuffs()
	{
		Buffs.ForEach(delegate(BuffVM vm)
		{
			vm.Dispose();
		});
		Buffs.Clear();
		m_CollapsedBuffs.Clear();
	}

	public void HandleBuffDidAdded(Buff buff)
	{
		if (Unit != null && Unit == buff.Owner && !buff.Hidden && buff.Icon != null)
		{
			AddBuff(buff);
		}
	}

	private void AddBuff(Buff buff)
	{
		if (buff.Blueprint.NeedCollapseStack)
		{
			if (!m_CollapsedBuffs.TryGetValue(buff.Blueprint, out var value))
			{
				m_CollapsedBuffs.Add(buff.Blueprint, value = new BuffVM(buff));
				Buffs.Add(value);
			}
			value.AdditionalSources.Add(buff);
		}
		else
		{
			Buffs.Add(new BuffVM(buff));
		}
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
		if (Unit != null && buff.Owner != null && Unit == buff.Owner)
		{
			BuffVM buffVM = Buffs.FirstOrDefault((BuffVM b) => b.Buff == buff);
			if (buffVM != null)
			{
				buffVM.Dispose();
				Buffs.Remove(buffVM);
				m_CollapsedBuffs.Remove(buff.Blueprint);
			}
		}
	}

	public void UpdateData()
	{
		using (ProfileScope.New("UnitBuffPartVM.UpdateData()"))
		{
			if (Unit == null)
			{
				ClearBuffs();
				return;
			}
			HashSet<Buff> unitBuffsSet;
			using (CollectionPool<HashSet<Buff>, Buff>.Get(out unitBuffsSet))
			{
				IEnumerable<Buff> range = Unit.Buffs.Enumerable.Where((Buff buff) => !buff.Hidden && buff.Icon != null);
				unitBuffsSet.AddRange(range);
				Buffs.RemoveAll(delegate(BuffVM buffVm)
				{
					if (!unitBuffsSet.Contains(buffVm.Buff))
					{
						m_CollapsedBuffs.Remove(buffVm.Buff.Blueprint);
						buffVm.Dispose();
						return true;
					}
					return false;
				});
				foreach (BuffVM buff in Buffs)
				{
					unitBuffsSet.Remove(buff.Buff);
					unitBuffsSet.RemoveRange(buff.AdditionalSources);
				}
				foreach (Buff item in unitBuffsSet)
				{
					AddBuff(item);
				}
			}
		}
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
	}

	public void HandleBuffIsSuppressedChanged(Buff buff)
	{
		if (buff.IsSuppressed)
		{
			HandleBuffDidRemoved(buff);
		}
		else
		{
			HandleBuffDidAdded(buff);
		}
	}

	public void SortBuffs()
	{
		List<BuffVM> list = Buffs.OrderBy((BuffVM b) => b.SortOrder).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			Buffs.Move(Buffs.IndexOf(list[i]), i);
		}
	}

	public BlueprintCareerPath GetCareerPath()
	{
		return UnitCareerPath;
	}

	public void OnAbilityEffectApplied(AbilityExecutionContext context)
	{
	}

	public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		MechanicEntity targetEntity = target.Target.Entity;
		if (targetEntity == null)
		{
			return;
		}
		BlueprintCareerPath unitCareerPath = UnitCareerPath;
		if (unitCareerPath == null || unitCareerPath.ImportantCareerBuffs == null || UnitCareerPath != m_HunterCareerPath)
		{
			return;
		}
		foreach (Buff item in targetEntity.Buffs.Enumerable)
		{
			if (item.Context.SourceAbilityContext != context)
			{
				continue;
			}
			foreach (BlueprintBuffReference importantCareerBuff in UnitCareerPath.ImportantCareerBuffs)
			{
				if (importantCareerBuff.Get() == item.Blueprint)
				{
					BuffVM buffVM = new BuffVM(item);
					DrawUniqueBuff.Execute(((UnitCareerPath, buffVM), true));
					m_HuntThePreyMarkTargetMap.TryAdd(targetEntity, buffVM);
					EventBus.RaiseEvent(delegate(ISetHuntThePreyMarkBuffsMap h)
					{
						h.HandleBuffsAdded(targetEntity, buffVM);
					});
				}
			}
		}
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (m_HuntThePreyMarkTargetMap.Keys.Contains(unitEntity))
		{
			EventBus.RaiseEvent(delegate(ISetHuntThePreyMarkBuffsMap h)
			{
				h.HandleBuffsRemoved(unitEntity);
			});
			m_HuntThePreyMarkTargetMap.Remove(unitEntity);
			RedrawBuffs.Execute(m_HuntThePreyMarkTargetMap.Values.ToList());
		}
	}

	public void HandleUnitChange(Dictionary<MechanicEntity, BuffVM> map)
	{
		m_HuntThePreyMarkTargetMap = map;
		RedrawBuffs.Execute(m_HuntThePreyMarkTargetMap.Values.ToList());
	}
}
