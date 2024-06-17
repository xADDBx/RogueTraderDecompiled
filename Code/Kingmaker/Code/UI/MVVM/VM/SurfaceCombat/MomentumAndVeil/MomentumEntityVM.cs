using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SurfaceCombat.MomentumAndVeil;

public class MomentumEntityVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGlobalRulebookHandler<RulePerformMomentumChange>, IRulebookHandler<RulePerformMomentumChange>, ISubscriber, IGlobalRulebookSubscriber
{
	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<int> m_Current = new ReactiveProperty<int>();

	public readonly ReactiveProperty<float> CurrentPercent = new ReactiveProperty<float>();

	private readonly ReactiveProperty<bool> m_IsParty = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HeroicActActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> DesperateMeasureActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<float> HeroicActPercent = new ReactiveProperty<float>();

	public readonly ReactiveProperty<float> DesperateMeasurePercent = new ReactiveProperty<float>();

	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveCommand m_MomentumChanged = new ReactiveCommand();

	private readonly MomentumGroup m_Group;

	private bool m_IsDesperateMeasuresReachedButDidntShowed;

	private bool m_IsHeroicActReachedButDidntShowed;

	private BlueprintMomentumRoot MomentumRoot => Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;

	public MomentumEntityVM(MomentumGroup momentumGroup)
	{
		m_IsDesperateMeasuresReachedButDidntShowed = false;
		m_IsHeroicActReachedButDidntShowed = false;
		m_Group = momentumGroup;
		m_Title.Value = momentumGroup.Blueprint.Name;
		m_IsParty.Value = momentumGroup.IsParty;
		AddDisposable(EventBus.Subscribe(this));
		UpdateMomentum(null);
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		if (m_IsParty.Value && currentUnit != null)
		{
			UpdateDesperateMeasure(currentUnit.GetDesperateMeasureThreshold(), currentUnit);
		}
	}

	public void OnEventDidTrigger(RulePerformMomentumChange evt)
	{
		if (evt.ResultGroup == m_Group)
		{
			UpdateMomentum(evt);
		}
	}

	public void UpdateMomentum(RulePerformMomentumChange evt)
	{
		m_Current.Value = m_Group.Momentum;
		CurrentPercent.Value = (float)m_Group.Momentum / (float)MomentumRoot.MaximalMomentum;
		Tooltip.Value = new TooltipTemplateMomentum(m_Current.Value);
		int heroicActThreshold = MomentumRoot.HeroicActThreshold;
		HeroicActPercent.Value = (float)heroicActThreshold / (float)MomentumRoot.MaximalMomentum;
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		HeroicActActive.Value = (float)m_Group.Momentum >= (float)heroicActThreshold || MomentumActionActive(currentUnit, MomentumAbilityType.HeroicAct);
		if (m_IsParty.Value && evt != null)
		{
			UpdateDesperateMeasure(currentUnit.GetDesperateMeasureThreshold(), currentUnit);
			TryPlayMomentumSound(evt, currentUnit);
			m_MomentumChanged.Execute();
		}
	}

	private void UpdateDesperateMeasure(int desperateMeasureThreshold, MechanicEntity entity)
	{
		DesperateMeasurePercent.Value = (float)desperateMeasureThreshold / (float)MomentumRoot.MaximalMomentum;
		DesperateMeasureActive.Value = (float)m_Group.Momentum <= (float)desperateMeasureThreshold || MomentumActionActive(entity, MomentumAbilityType.DesperateMeasure);
	}

	private void TryPlayMomentumSound(RulePerformMomentumChange evt, MechanicEntity entity)
	{
		bool isInPlayerParty = entity.IsInPlayerParty;
		bool flag = MomentumActionExist(entity, MomentumAbilityType.HeroicAct);
		bool flag2 = MomentumActionExist(entity, MomentumAbilityType.DesperateMeasure);
		int desperateMeasureThreshold = evt.ConcreteInitiator.GetDesperateMeasureThreshold();
		if (evt.ResultPrevValue > desperateMeasureThreshold && evt.ResultCurrentValue <= desperateMeasureThreshold && isInPlayerParty && !m_IsDesperateMeasuresReachedButDidntShowed)
		{
			if (flag2)
			{
				UISounds.Instance.Sounds.Combat.MomentumDesperateMeasuresReached.Play();
			}
			else
			{
				m_IsDesperateMeasuresReachedButDidntShowed = true;
			}
		}
		if (evt.ResultPrevValue < MomentumRoot.HeroicActThreshold && evt.ResultCurrentValue >= MomentumRoot.HeroicActThreshold && isInPlayerParty && !m_IsHeroicActReachedButDidntShowed)
		{
			if (flag)
			{
				UISounds.Instance.Sounds.Combat.MomentumHeroicActReached.Play();
			}
			else
			{
				m_IsHeroicActReachedButDidntShowed = true;
			}
		}
		if (m_IsDesperateMeasuresReachedButDidntShowed && flag2 && isInPlayerParty)
		{
			UISounds.Instance.Sounds.Combat.MomentumDesperateMeasuresReached.Play();
			m_IsDesperateMeasuresReachedButDidntShowed = false;
		}
		if (m_IsHeroicActReachedButDidntShowed && flag && isInPlayerParty)
		{
			UISounds.Instance.Sounds.Combat.MomentumHeroicActReached.Play();
			m_IsHeroicActReachedButDidntShowed = false;
		}
	}

	private bool MomentumActionExist(MechanicEntity entity, MomentumAbilityType type)
	{
		return entity.Facts.Contains(delegate(EntityFact a)
		{
			if (a.Blueprint is BlueprintAbility blueprint && blueprint.GetComponent<AbilitySpecialMomentumAction>() != null)
			{
				AbilitySpecialMomentumAction component = blueprint.GetComponent<AbilitySpecialMomentumAction>();
				if (component == null || component.MomentumType != type)
				{
					AbilitySpecialMomentumAction component2 = blueprint.GetComponent<AbilitySpecialMomentumAction>();
					if (component2 == null)
					{
						return false;
					}
					return component2.MomentumType == MomentumAbilityType.Both;
				}
				return true;
			}
			return false;
		});
	}

	private bool MomentumActionActive(MechanicEntity entity, MomentumAbilityType type)
	{
		if (entity is BaseUnitEntity baseUnitEntity)
		{
			return (baseUnitEntity?.UISettings.GetMomentumSlots()).Where(delegate(MechanicActionBarSlot s)
			{
				if (s.GetContentData() is AbilityData abilityData && abilityData.Blueprint.GetComponent<AbilitySpecialMomentumAction>() != null)
				{
					AbilitySpecialMomentumAction component = abilityData.Blueprint.GetComponent<AbilitySpecialMomentumAction>();
					if (component == null || component.MomentumType != type)
					{
						AbilitySpecialMomentumAction component2 = abilityData.Blueprint.GetComponent<AbilitySpecialMomentumAction>();
						if (component2 == null)
						{
							return false;
						}
						return component2.MomentumType == MomentumAbilityType.Both;
					}
					return true;
				}
				return false;
			}).Any((MechanicActionBarSlot slot) => slot.IsPossibleActive);
		}
		return false;
	}

	protected override void DisposeImplementation()
	{
		m_IsDesperateMeasuresReachedButDidntShowed = false;
		m_IsHeroicActReachedButDidntShowed = false;
	}

	public void OnEventAboutToTrigger(RulePerformMomentumChange evt)
	{
	}
}
