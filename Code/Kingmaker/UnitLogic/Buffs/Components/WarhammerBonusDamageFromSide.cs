using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[TypeId("b821b468558e4e41beaa8bf08f9178b4")]
public class WarhammerBonusDamageFromSide : UnitBuffComponentDelegate, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IHashable
{
	public class Data : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public WarhammerCombatSide ChosenSide;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref ChosenSide);
			return result;
		}
	}

	public bool IsRandomSide;

	[HideIf("IsRandomSide")]
	public bool ClosestToCasterSide;

	[HideIf("IsRandomSide")]
	public WarhammerCombatSide Side;

	[SerializeField]
	[HideIf("IsRandomSide")]
	private bool OtherSidesButSelected;

	public int BonusDamagePercent;

	public ContextValue PercentValue;

	public ContextValue FlatValue;

	public ContextValue UnmodifiableFlatValue;

	public ContextValueModifierWithType Modifier;

	public bool CreateOppositeOnProk;

	public bool DestroyOnProk;

	public bool CanStack;

	public GameObject BonusDamageMarker;

	public ConditionsChecker ApplyConditions;

	public ActionList ActionsOnAttackInitiator;

	public ActionList ActionsOnAttackTarget;

	public ActionList ActionsOnNotAttackInitiator;

	public ActionList ActionsOnNotAttackTarget;

	[SerializeField]
	private BlueprintUnitFactReference m_MarkedForDestruction;

	[SerializeField]
	private BlueprintAbilityGroupReference m_DoTAbilityGroup;

	public BlueprintUnitFact MarkedForDestruction => m_MarkedForDestruction?.Get();

	public BlueprintAbilityGroup DoTAbilityGroup => m_DoTAbilityGroup?.Get();

	protected override void OnActivate()
	{
		base.OnActivate();
		WarhammerCombatSide warhammerCombatSide;
		if (IsRandomSide)
		{
			warhammerCombatSide = (WarhammerCombatSide)base.Owner.Random.Range(0, 4);
		}
		else if (ClosestToCasterSide)
		{
			Vector3 forward = base.Owner.Forward;
			Vector3 normalized = (base.Owner.Center - base.Context.MaybeCaster.Center).normalized;
			warhammerCombatSide = CustomGraphHelper.GetWarhammerAttackSide(forward, normalized, base.Owner.Size);
		}
		else
		{
			warhammerCombatSide = Side;
		}
		base.Owner.GetOrCreate<UnitPartSideVulnerability>().Add(base.Fact, warhammerCombatSide, CanStack);
		RequestSavableData<Data>().ChosenSide = warhammerCombatSide;
		ShowBonusDamageMarker();
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		HideBonusDamageMarker();
		base.Owner.GetOptional<UnitPartSideVulnerability>()?.Remove(base.Fact);
	}

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (base.Owner.GetOptional<UnitPartSideVulnerability>()?.Get(base.Fact) == null || evt.MaybeTarget == null || evt.MaybeTarget.Facts.Contains(MarkedForDestruction))
		{
			return;
		}
		if (evt.Reason.Fact != null)
		{
			BlueprintBuff blueprintBuff = ((evt.Reason.Fact.Blueprint is BlueprintBuff) ? (evt.Reason.Fact.Blueprint as BlueprintBuff) : null);
			if (blueprintBuff != null && blueprintBuff.AbilityGroups.Contains(BlueprintWarhammerRoot.Instance.CombatRoot.DamageOverTimeAbilityGroup))
			{
				return;
			}
		}
		using (ContextData<MechanicsContext.Data>.Request().Setup(base.Context, evt.MaybeTarget))
		{
			using (ContextData<PropertyContextData>.Request().Setup(new PropertyContext(evt.ConcreteInitiator, base.Fact, evt.MaybeTarget, base.Context, evt, evt.Ability)))
			{
				if (ApplyConditions == null || ApplyConditions.Check())
				{
					Vector3 normalized = (evt.MaybeTarget.Center - evt.ConcreteInitiator.Center).normalized;
					WarhammerCombatSide warhammerAttackSide = CustomGraphHelper.GetWarhammerAttackSide(evt.MaybeTarget.Forward, normalized, evt.MaybeTarget.Size);
					if (IsSideCorrect(warhammerAttackSide))
					{
						evt.ValueModifiers.Add(ModifierType.PctAdd, BonusDamagePercent + PercentValue.Calculate(base.Context), base.Fact);
						evt.ValueModifiers.Add(ModifierType.ValAdd, FlatValue.Calculate(base.Context), base.Fact);
						evt.ValueModifiers.Add(ModifierType.ValAdd_Extra, UnmodifiableFlatValue.Calculate(base.Context), base.Fact);
						Modifier.TryApply(evt.ValueModifiers, base.Fact, ModifierDescriptor.None);
					}
				}
			}
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		UnitPartSideVulnerability.Entry entry = base.Owner.GetOptional<UnitPartSideVulnerability>()?.Get(base.Fact);
		if (entry == null)
		{
			return;
		}
		if (evt.Reason.Fact != null)
		{
			BlueprintBuff blueprintBuff = ((evt.Reason.Fact.Blueprint is BlueprintBuff) ? (evt.Reason.Fact.Blueprint as BlueprintBuff) : null);
			if (blueprintBuff != null && blueprintBuff.AbilityGroups.Contains(BlueprintWarhammerRoot.Instance.CombatRoot.DamageOverTimeAbilityGroup))
			{
				return;
			}
		}
		Vector3 normalized = (evt.ConcreteTarget.Center - evt.ConcreteInitiator.Center).normalized;
		if (CustomGraphHelper.GetWarhammerAttackSide(evt.ConcreteTarget.Forward, normalized, evt.ConcreteTarget.Size) == entry.UnitSide)
		{
			base.Fact.RunActionInContext(ActionsOnAttackInitiator, evt.ConcreteInitiator.ToITargetWrapper());
			base.Fact.RunActionInContext(ActionsOnAttackTarget, evt.ConcreteTarget.ToITargetWrapper());
			if (CreateOppositeOnProk)
			{
				HideBonusDamageMarker();
				base.Owner.GetOptional<UnitPartSideVulnerability>()?.CreateOpposite(base.Fact, entry, DestroyOnProk, CanStack);
				entry = base.Owner.GetOptional<UnitPartSideVulnerability>()?.Get(base.Fact);
				if (entry != null)
				{
					RequestSavableData<Data>().ChosenSide = entry.UnitSide;
					ShowBonusDamageMarker();
				}
			}
		}
		else
		{
			base.Fact.RunActionInContext(ActionsOnNotAttackInitiator, evt.ConcreteInitiator.ToITargetWrapper());
			base.Fact.RunActionInContext(ActionsOnNotAttackTarget, evt.ConcreteTarget.ToITargetWrapper());
		}
	}

	private void ShowBonusDamageMarker()
	{
		UnitPartSideVulnerability.Entry entry = base.Owner.GetOptional<UnitPartSideVulnerability>()?.Get(base.Fact);
		if (entry != null && !(base.Owner.View == null))
		{
			if (entry.Marker == null)
			{
				entry.Marker = Object.Instantiate(BonusDamageMarker);
			}
			entry.Marker.transform.position = base.Owner.View.ViewTransform.position;
			entry.Marker.transform.rotation = base.Owner.View.ViewTransform.rotation;
			entry.Marker.transform.SetParent(base.Owner.View.ViewTransform);
			switch (entry.UnitSide)
			{
			case WarhammerCombatSide.Front:
				entry.Marker.transform.position += base.Owner.View.ViewTransform.forward * base.Owner.View.Corpulence;
				break;
			case WarhammerCombatSide.Left:
				entry.Marker.transform.rotation = Quaternion.LookRotation(-base.Owner.View.ViewTransform.right, base.Owner.View.ViewTransform.up);
				entry.Marker.transform.position -= base.Owner.View.ViewTransform.right * base.Owner.View.Corpulence;
				break;
			case WarhammerCombatSide.Right:
				entry.Marker.transform.rotation = Quaternion.LookRotation(base.Owner.View.ViewTransform.right, base.Owner.View.ViewTransform.up);
				entry.Marker.transform.position += base.Owner.View.ViewTransform.right * base.Owner.View.Corpulence;
				break;
			case WarhammerCombatSide.Back:
				entry.Marker.transform.rotation = Quaternion.LookRotation(-base.Owner.View.ViewTransform.forward, base.Owner.View.ViewTransform.up);
				entry.Marker.transform.position -= base.Owner.View.ViewTransform.forward * base.Owner.View.Corpulence;
				break;
			}
			entry.Marker.SetActive(value: true);
		}
	}

	private void HideBonusDamageMarker()
	{
		UnitPartSideVulnerability.Entry entry = base.Owner.GetOptional<UnitPartSideVulnerability>()?.Get(base.Fact);
		if (entry != null && entry.Marker != null)
		{
			entry.Marker.SetActive(value: false);
		}
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		ShowBonusDamageMarker();
	}

	protected override void OnViewWillDetach()
	{
		HideBonusDamageMarker();
		base.OnViewWillDetach();
	}

	private bool IsSideCorrect(WarhammerCombatSide side)
	{
		WarhammerCombatSide chosenSide = RequestSavableData<Data>().ChosenSide;
		if (!OtherSidesButSelected)
		{
			return side == chosenSide;
		}
		return side != chosenSide;
	}

	public bool CheckSide(MechanicEntity initiator, MechanicEntity target)
	{
		Vector3 normalized = (target.Center - initiator.Center).normalized;
		WarhammerCombatSide warhammerAttackSide = CustomGraphHelper.GetWarhammerAttackSide(target.Forward, normalized, target.Size);
		return IsSideCorrect(warhammerAttackSide);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
