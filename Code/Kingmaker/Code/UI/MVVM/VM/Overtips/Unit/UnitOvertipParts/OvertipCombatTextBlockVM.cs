using System;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.UI.MVVM.VM.Overtips.CombatText;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;

public class OvertipCombatTextBlockVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IEntitySubscriber, IGlobalRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IGlobalRulebookSubscriber, IGlobalRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, IWarhammerAttackHandler, IHealingHandler, IDamageHandler, IAttackOfOpportunityHandler<EntitySubscriber>, IAttackOfOpportunityHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IAttackOfOpportunityHandler, EntitySubscriber>, IStarshipAttackHandler, IGlobalRulebookHandler<RuleDealStarshipMoraleDamage>, IRulebookHandler<RuleDealStarshipMoraleDamage>, IGlobalRulebookHandler<RuleHealStarshipMoraleDamage>, IRulebookHandler<RuleHealStarshipMoraleDamage>, ICustomCombatText, IUICultAmbushVisibilityChangeHandler
{
	public readonly ReactiveCommand<CombatMessageBase> CombatMessage = new ReactiveCommand<CombatMessageBase>();

	private readonly MechanicEntity m_MechanicEntity;

	public readonly ReactiveProperty<Vector3> Position;

	public bool IsCutscene => Game.Instance.CurrentMode == GameModeType.Cutscene;

	public IEntity GetSubscribingEntity()
	{
		return m_MechanicEntity;
	}

	public OvertipCombatTextBlockVM(MechanicEntity mechanicEntity, ReactiveProperty<Vector3> position)
	{
		Position = position;
		m_MechanicEntity = mechanicEntity;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleHealing(RuleHealDamage healDamage)
	{
		if (healDamage.Target != m_MechanicEntity)
		{
			return;
		}
		int value = healDamage.Value;
		if (value != 0)
		{
			Sprite sprite = null;
			if (Rulebook.CurrentContext.First is RulePerformAbility rulePerformAbility)
			{
				sprite = rulePerformAbility.Spell.Blueprint.Icon;
			}
			CombatMessage.Execute(new CombatMessageHealing
			{
				Amount = value,
				Sprite = sprite
			});
		}
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if ((bool)ContextData<GameLogDisabled>.Current || dealDamage.Target != m_MechanicEntity || dealDamage.FromRuleWarhammerAttackRoll)
		{
			return;
		}
		Sprite sprite = null;
		if (Rulebook.CurrentContext.First is RulePerformAbility rulePerformAbility)
		{
			sprite = rulePerformAbility.Spell.Blueprint.Icon;
		}
		if (sprite == null)
		{
			if (dealDamage.Reason.Ability != null)
			{
				sprite = dealDamage.Reason.Ability.Blueprint.Icon;
			}
			else if (dealDamage.Reason.Fact != null)
			{
				sprite = (dealDamage.Reason.Fact.Blueprint as BlueprintAbility)?.Icon;
				if (sprite == null)
				{
					sprite = (dealDamage.Reason.Fact.Blueprint as BlueprintActivatableAbility)?.Icon;
				}
				if (sprite == null)
				{
					sprite = (dealDamage.Reason.Fact.Blueprint as BlueprintBuff)?.Icon;
				}
			}
		}
		CombatMessageDamage parameter = new CombatMessageDamage
		{
			Amount = dealDamage.Result,
			Sprite = sprite,
			IsCritical = false,
			IsImmune = dealDamage.Damage.Immune,
			SourcePosition = (dealDamage.Projectile?.LaunchPosition ?? Vector3.zero),
			TargetPosition = (dealDamage.Projectile?.CorePosition ?? Vector3.zero)
		};
		CombatMessage.Execute(parameter);
	}

	public void HandleAttackOfOpportunity(BaseUnitEntity target)
	{
		CombatMessage.Execute(new CombatMessageAttackOfOpportunity());
	}

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSavingThrow evt)
	{
		if (Rulebook.CurrentContext.Current != null && Rulebook.CurrentContext.Current.Initiator == m_MechanicEntity)
		{
			RuleDealDamage ruleDealDamage = Rulebook.CurrentContext.LastEventOfType<RuleDealDamage>();
			if (evt.IsPassed || ruleDealDamage?.Initiator != evt.Initiator)
			{
				CombatMessage.Execute(new CombatMessageSavingThrow
				{
					Passed = evt.IsPassed,
					Reason = evt.Reason.Name,
					Sprite = evt.Reason.Icon,
					StatType = evt.StatType,
					Roll = -1,
					DC = -1
				});
			}
		}
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (!evt.Spell.Blueprint.DisableLog && Rulebook.CurrentContext.Current != null && Rulebook.CurrentContext.Current.Initiator == m_MechanicEntity)
		{
			CombatMessage.Execute(new CombatMessageAbility
			{
				Name = evt.Spell.Blueprint.Name,
				Sprite = evt.Spell.Blueprint.Icon
			});
		}
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		if (withWeaponAttackHit.Target == m_MechanicEntity)
		{
			if (withWeaponAttackHit.ResultIsHit)
			{
				WarhammerAttackHit(withWeaponAttackHit);
				return;
			}
			CombatMessage.Execute(new CombatMessageAttackMiss
			{
				Result = withWeaponAttackHit.Result
			});
		}
	}

	private void WarhammerAttackHit(RulePerformAttack withWeaponAttackHit)
	{
		RuleDealDamage resultDamageRule = withWeaponAttackHit.ResultDamageRule;
		if (resultDamageRule == null)
		{
			return;
		}
		Sprite sprite = null;
		if (Rulebook.CurrentContext.First is RulePerformAbility rulePerformAbility)
		{
			sprite = rulePerformAbility.Spell.Blueprint.Icon;
		}
		if (sprite == null && resultDamageRule != null)
		{
			if (resultDamageRule.Reason.Ability != null)
			{
				sprite = resultDamageRule.Reason.Ability.Blueprint.Icon;
			}
			else if (resultDamageRule.Reason.Fact != null)
			{
				BlueprintMechanicEntityFact blueprint = resultDamageRule.Reason.Fact.Blueprint;
				if (!(blueprint is BlueprintAbility blueprintAbility))
				{
					if (!(blueprint is BlueprintActivatableAbility blueprintActivatableAbility))
					{
						if (blueprint is BlueprintBuff blueprintBuff)
						{
							sprite = blueprintBuff.Icon;
						}
					}
					else
					{
						sprite = blueprintActivatableAbility.Icon;
					}
				}
				else
				{
					sprite = blueprintAbility.Icon;
				}
			}
		}
		CombatMessageDamage parameter = new CombatMessageDamage
		{
			Amount = withWeaponAttackHit.ResultDamageValue,
			Sprite = sprite,
			IsCritical = withWeaponAttackHit.RollPerformAttackRule.ResultIsRighteousFury,
			IsImmune = false,
			IsEnemy = ((m_MechanicEntity as UnitEntity)?.Faction.IsPlayerEnemy ?? false),
			SourcePosition = (resultDamageRule.Projectile?.LaunchPosition ?? Vector3.zero),
			TargetPosition = (resultDamageRule.Projectile?.CorePosition ?? Vector3.zero)
		};
		CombatMessage.Execute(parameter);
	}

	public void HandleCustomCombatText(BaseUnitEntity targetUnit, string text)
	{
		if (targetUnit == m_MechanicEntity)
		{
			CombatMessage.Execute(new CombatMessageCustom
			{
				Text = text
			});
		}
	}

	public void HandleCultAmbushVisibilityChange()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity == m_MechanicEntity)
		{
			CombatMessage.Execute(new CombatMessageCultAmbush());
		}
	}

	public void HandleAttack(RuleStarshipPerformAttack starshipAttack)
	{
		if (starshipAttack.Target == m_MechanicEntity)
		{
			if (starshipAttack.ResultIsHit)
			{
				StarshipAttackHit(starshipAttack);
				return;
			}
			CombatMessage.Execute(new CombatMessageAttackMiss
			{
				Result = starshipAttack.Result
			});
		}
	}

	private void StarshipAttackHit(RuleStarshipPerformAttack starshipAttack)
	{
		if (starshipAttack.Weapon.IsFocusedEnergyWeapon)
		{
			CombatMessage.Execute(new CombatMessageStarshipDamage
			{
				Amount = starshipAttack.ResultDamageBeforeAbsorption,
				Sprite = starshipAttack.Ability.Blueprint.Icon,
				IsCritical = starshipAttack.ResultIsCritical,
				IsShieldDamage = (starshipAttack.ResultAbsorbedDamage > 0),
				IsEnemy = ((m_MechanicEntity as UnitEntity)?.Faction.IsPlayerEnemy ?? false)
			});
			return;
		}
		if (starshipAttack.ResultAbsorbedDamage > 0)
		{
			CombatMessage.Execute(new CombatMessageStarshipDamage
			{
				Amount = starshipAttack.ResultShieldStrengthLoss,
				Sprite = starshipAttack.Ability.Blueprint.Icon,
				IsShieldDamage = true,
				IsEnemy = ((m_MechanicEntity as UnitEntity)?.Faction.IsPlayerEnemy ?? false)
			});
		}
		if (starshipAttack.ResultDamage > 0)
		{
			CombatMessage.Execute(new CombatMessageStarshipDamage
			{
				Amount = starshipAttack.ResultDamage,
				Sprite = starshipAttack.Ability.Blueprint.Icon,
				IsCritical = starshipAttack.ResultIsCritical,
				IsShieldDamage = false,
				IsEnemy = ((m_MechanicEntity as UnitEntity)?.Faction.IsPlayerEnemy ?? false)
			});
		}
	}

	public void OnEventAboutToTrigger(RuleDealStarshipMoraleDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealStarshipMoraleDamage evt)
	{
		if (evt.Target == m_MechanicEntity && evt.Result != 0)
		{
			CombatMessage.Execute(new CombatMessageMomentum
			{
				Count = evt.Result
			});
		}
	}

	public void OnEventAboutToTrigger(RuleHealStarshipMoraleDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleHealStarshipMoraleDamage evt)
	{
		if (evt.Target == m_MechanicEntity && evt.Result != 0)
		{
			CombatMessage.Execute(new CombatMessageMomentum
			{
				Count = evt.Result
			});
		}
	}
}
