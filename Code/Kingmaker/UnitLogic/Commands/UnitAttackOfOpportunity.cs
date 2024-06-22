using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Newtonsoft.Json;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitAttackOfOpportunity : UnitCommand<UnitAttackOfOpportunityParams>
{
	public static readonly HashSet<UnitAttackOfOpportunity> AllActive = new HashSet<UnitAttackOfOpportunity>();

	[JsonProperty]
	private BlueprintAbility m_AbilityBlueprint;

	[JsonProperty]
	private bool m_RemoveAbilityAfterAttackEnds;

	private UnitAnimationActionCastSpell.SpecialBehaviourType m_Special;

	private UnitAnimationActionCastSpell.CastAnimationStyle m_CastAnimStyle;

	private float m_CastTime;

	private PlayLoopAnimationByBuff m_loopingAnimationBuff;

	[JsonProperty]
	public WeaponSlot Hand { get; private set; }

	[JsonProperty]
	public AbilityExecutionProcess ExecutionProcess { get; private set; }

	public BlueprintFact Reason => base.Params.Reason;

	public new MechanicEntity Target => base.Target?.Entity;

	public override bool ShouldBeInterrupted
	{
		get
		{
			if (base.Executor != null)
			{
				return !base.Executor.CanAttack(Hand?.Weapon);
			}
			return false;
		}
	}

	public override bool IsMoveUnit => false;

	public int CurrentActionIndex => ExecutionProcess?.Context.ActionIndex ?? 0;

	public int ActionsCount { get; private set; }

	private AbilityData Ability { get; set; }

	private WeaponAnimationStyle WeaponStyle
	{
		get
		{
			UnitViewHandsEquipment handsEquipment = base.Executor.View.HandsEquipment;
			if (handsEquipment != null && !handsEquipment.InCombat)
			{
				return WeaponAnimationStyle.Fist;
			}
			return Ability.Weapon?.GetAnimationStyle() ?? base.Executor.Body.PrimaryHand?.GetWeaponStyle() ?? WeaponAnimationStyle.None;
		}
	}

	public override bool IsWaitingForAnimation
	{
		get
		{
			if (base.Executor?.AnimationManager != null && !base.Executor.AnimationManager.CanRunIdleAction())
			{
				return !base.Executor.AnimationManager.InCover;
			}
			return false;
		}
	}

	public UnitAttackOfOpportunity(UnitAttackOfOpportunityParams @params)
		: base(@params)
	{
	}

	protected override void TriggerAnimation()
	{
		IEnumerable<PlayLoopAnimationByBuff> components = base.Executor.OwnerEntity.Facts.GetComponents<PlayLoopAnimationByBuff>();
		if (!components.Empty())
		{
			m_loopingAnimationBuff = components.First();
			m_loopingAnimationBuff.TryResetAction();
		}
		base.Executor.View.HideOffWeapon(hide: true);
		IAbilityCustomAnimation component = Ability.Blueprint.GetComponent<IAbilityCustomAnimation>();
		if (component != null)
		{
			UnitAnimationAction unitAnimationAction = component.GetAbilityAction(base.Executor)?.Load();
			if (!unitAnimationAction)
			{
				ScheduleAct(SlowMoController.SlowMoFactor);
			}
			else if (base.Executor.View.AnimationManager?.CreateHandle(unitAnimationAction) is UnitAnimationActionHandle unitAnimationActionHandle)
			{
				unitAnimationActionHandle.CastingTime = m_CastTime;
				unitAnimationActionHandle.Spell = Ability.Blueprint;
				unitAnimationActionHandle.SpecialCastBehaviour = m_Special;
				unitAnimationActionHandle.AttackWeaponStyle = WeaponStyle;
				unitAnimationActionHandle.AttackTargetDistance = (Target.Position - base.Executor.Position).magnitude;
				unitAnimationActionHandle.IsBurst = Ability.IsBurstAttack;
				unitAnimationActionHandle.BurstCount = Ability.BurstAttacksCount;
				StartAnimation(unitAnimationActionHandle);
			}
			else
			{
				PFLog.Default.Error(base.Executor.View, $"{base.Executor} cannot start custom animation {unitAnimationAction} for {Ability.Blueprint}");
				ScheduleAct(SlowMoController.SlowMoFactor);
			}
		}
		else
		{
			StartAnimation(Ability.Blueprint.IsSpell ? UnitAnimationType.CastSpell : (IsMainHandAttack(Ability) ? UnitAnimationType.MainHandAttack : UnitAnimationType.OffHandAttack), UnitAnimationActionHandleInitializer);
		}
		base.TriggerAnimation();
		void UnitAnimationActionHandleInitializer(UnitAnimationActionHandle h)
		{
			h.CastStyle = m_CastAnimStyle;
			h.CastingTime = m_CastTime;
			h.Spell = Ability.Blueprint;
			h.SpecialCastBehaviour = m_Special;
			h.AttackWeaponStyle = WeaponStyle;
			h.AttackTargetDistance = (Target.Position - base.Executor.Position).magnitude;
			h.IsBurst = Ability.IsBurstAttack;
			h.BurstCount = Ability.BurstAttacksCount;
		}
	}

	protected override void StartAnimation(UnitAnimationActionHandle handle)
	{
		ItemEntityWeapon weapon = Ability.Weapon;
		MechadendritesType mechadendritesType = ((weapon == null || weapon.Blueprint.IsMelee) ? Ability.Blueprint.UsedMechadendrite : MechadendritesType.Ballistic);
		ItemEntityWeapon weapon2 = Ability.Weapon;
		if (((weapon2 != null && !weapon2.Blueprint.IsMelee) || Ability.Blueprint.UseOnMechadendrite) && base.Executor.GetOptional<UnitPartMechadendrites>() != null)
		{
			UnitPartMechadendrites optional = base.Executor.GetOptional<UnitPartMechadendrites>();
			if (optional != null && optional.Mechadendrites.ContainsKey(mechadendritesType))
			{
				base.Animation = handle;
				base.HasAnimation = true;
				UnitAnimationActionHandle unitAnimationActionHandle = (UnitAnimationActionHandle)handle.Clone();
				handle.AttackWeaponStyle = WeaponAnimationStyle.Mechadendrites;
				handle.CastStyle = UnitAnimationActionCastSpell.CastAnimationStyle.Mechadendrites;
				unitAnimationActionHandle.ChangeManager(base.Executor.GetOptional<UnitPartMechadendrites>()?.Mechadendrites[mechadendritesType]?.AnimationManager);
				if (unitAnimationActionHandle.Action != null)
				{
					base.Executor.GetOptional<UnitPartMechadendrites>()?.Mechadendrites[mechadendritesType]?.AnimationManager.Execute(unitAnimationActionHandle);
					Dictionary<MechadendritesType, MechadendriteSettings>.ValueCollection valueCollection = base.Executor.GetOptional<UnitPartMechadendrites>()?.Mechadendrites.Values;
					if (valueCollection != null && !valueCollection.Empty())
					{
						foreach (MechadendriteSettings item in valueCollection)
						{
							if (item.MechadendritesType != mechadendritesType)
							{
								UnitAnimationActionHandle unitAnimationActionHandle2 = (UnitAnimationActionHandle)unitAnimationActionHandle.Clone();
								unitAnimationActionHandle2.ChangeManager(item.AnimationManager);
								if (unitAnimationActionHandle2.Action != null)
								{
									item.AnimationManager.Execute(unitAnimationActionHandle2);
								}
							}
						}
					}
				}
			}
		}
		base.StartAnimation(handle);
	}

	protected override void OnInit(AbstractUnitEntity executor)
	{
		base.OnInit(executor);
		AllActive.Add(this);
		Hand = base.Executor.GetThreatHand();
		if (Hand == null)
		{
			throw new Exception($"{base.Executor} can't make attack of opportunity: has no threat hand");
		}
		m_AbilityBlueprint = Hand.Weapon.Blueprint.AttackOfOpportunityAbility;
		if (m_AbilityBlueprint == null)
		{
			throw new Exception($"{base.Executor} can't make attack of opportunity: weapon in threat hand doesn't have any ability for AOO");
		}
		AbilityData abilityData = new AbilityData(m_AbilityBlueprint, base.Executor)
		{
			OverrideWeapon = Hand.Weapon,
			IsAttackOfOpportunity = true
		};
		if (Hand.Weapon?.Blueprint?.AttackOfOpportunityAbilityFXSettings != null)
		{
			abilityData.FXSettingsOverride = Hand.Weapon.Blueprint.AttackOfOpportunityAbilityFXSettings;
		}
		ActionsCount = abilityData.BurstAttacksCount;
		Ability = abilityData;
		if (!base.IsOneFrameCommand)
		{
			m_CastAnimStyle = Ability.Blueprint.Animation;
			m_CastTime = 2.5f;
		}
		else
		{
			m_Special = UnitAnimationActionCastSpell.SpecialBehaviourType.NoPrecast;
			m_CastTime = 0f;
		}
		BlueprintItemEquipmentUsable sourceItemUsableBlueprint = Ability.SourceItemUsableBlueprint;
		if (sourceItemUsableBlueprint != null && sourceItemUsableBlueprint.Type == UsableItemType.Wand)
		{
			m_CastAnimStyle = ((Target == base.Executor) ? UnitAnimationActionCastSpell.CastAnimationStyle.Self : UnitAnimationActionCastSpell.CastAnimationStyle.Directional);
			m_Special = UnitAnimationActionCastSpell.SpecialBehaviourType.NoPrecast;
			m_CastTime = 0f;
		}
		if (TurnController.IsInTurnBasedCombat() && base.Executor.IsInCombat)
		{
			m_CastTime = Math.Min(5f, m_CastTime);
		}
	}

	public override void Clear()
	{
		AllActive.Remove(this);
		base.Clear();
	}

	protected override void OnTick()
	{
		AbilityExecutionProcess executionProcess = ExecutionProcess;
		if (executionProcess != null && executionProcess.IsEngageUnit && executionProcess.IsEnded)
		{
			UnitAnimationActionHandle animation = base.Animation;
			if (animation == null || animation.IsReleased)
			{
				RestoreLoopAnimation();
				ForceFinish(ResultType.Success);
			}
		}
	}

	private void RestoreLoopAnimation()
	{
		if (m_loopingAnimationBuff != null)
		{
			m_loopingAnimationBuff.TrySetAction();
			m_loopingAnimationBuff = null;
		}
	}

	private bool IsMainHandAttack(AbilityData ability)
	{
		PartUnitBody bodyOptional = ability.Caster.GetBodyOptional();
		if (bodyOptional == null)
		{
			return true;
		}
		if (ability.Caster?.GetOptional<UnitPartMechadendrites>() == null)
		{
			return ability.Weapon == bodyOptional.PrimaryHand?.MaybeWeapon;
		}
		return true;
	}

	protected override ResultType OnAction()
	{
		if (CurrentActionIndex > 0)
		{
			if (CurrentActionIndex >= ActionsCount)
			{
				PFLog.Default.Error("CurrentActionIndex >= ActionsCount");
				RestoreLoopAnimation();
				return ResultType.Fail;
			}
			if (ExecutionProcess == null)
			{
				PFLog.Default.Error("ExecutionProcess == null");
				RestoreLoopAnimation();
				return ResultType.Fail;
			}
			ExecutionProcess.Context.NextAction();
			RestoreLoopAnimation();
			if (!ExecutionProcess.IsEngageUnit && CurrentActionIndex >= ActionsCount)
			{
				return ResultType.Success;
			}
			return ResultType.None;
		}
		AbilityExecutionContext context = Ability.CreateExecutionContext(Target);
		ExecutionProcess = Game.Instance.AbilityExecutor.Execute(context);
		ExecutionProcess.Context.NextAction();
		RestoreLoopAnimation();
		if (!ExecutionProcess.IsEngageUnit && CurrentActionIndex >= ActionsCount)
		{
			return ResultType.Success;
		}
		return ResultType.None;
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (base.Executor != null)
		{
			AllActive.Add(this);
		}
	}
}
