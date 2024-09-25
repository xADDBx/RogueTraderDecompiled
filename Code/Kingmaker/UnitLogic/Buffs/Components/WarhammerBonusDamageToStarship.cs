using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[TypeId("7421aebdb7c790c4bad0366a79eb2db1")]
public class WarhammerBonusDamageToStarship : UnitBuffComponentDelegate, ITargetRulebookHandler<RuleStarshipCalculateDamageForTarget>, IRulebookHandler<RuleStarshipCalculateDamageForTarget>, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleStarshipPerformAttack>, IRulebookHandler<RuleStarshipPerformAttack>, IHashable
{
	private enum SidePolicy
	{
		ChoosenSide,
		RandomSide,
		RandomExceptOppisite
	}

	private enum ApplyModificationTo
	{
		AllIncoming,
		Hull
	}

	[SerializeField]
	private SidePolicy m_sidePolicy;

	[SerializeField]
	[ShowIf("ChooseSide")]
	private StarshipHitLocation m_StarshipSide;

	[SerializeField]
	private ApplyModificationTo applyModificationTo;

	[SerializeField]
	private int m_BonusDamage;

	[SerializeField]
	private float m_ExtraDamageMod;

	[SerializeField]
	private GameObject m_BonusDamageMarker;

	private bool ChooseSide => m_sidePolicy == SidePolicy.ChoosenSide;

	protected override void OnActivate()
	{
		StarshipHitLocation location;
		if (m_sidePolicy != SidePolicy.RandomExceptOppisite || !(base.Buff.Owner is StarshipEntity) || !(base.Buff.Context.MaybeCaster is StarshipEntity initiator))
		{
			location = ((m_sidePolicy != SidePolicy.RandomSide && m_StarshipSide != 0) ? m_StarshipSide : ((StarshipHitLocation)base.Owner.Random.Range(1, 5)));
		}
		else
		{
			StarshipHitLocation[] array = Rulebook.Trigger(new RuleStarshipCalculateHitLocation(initiator, base.Buff.Owner as StarshipEntity)).ResultHitLocation switch
			{
				StarshipHitLocation.Fore => new StarshipHitLocation[3]
				{
					StarshipHitLocation.Fore,
					StarshipHitLocation.Port,
					StarshipHitLocation.Starboard
				}, 
				StarshipHitLocation.Aft => new StarshipHitLocation[3]
				{
					StarshipHitLocation.Aft,
					StarshipHitLocation.Port,
					StarshipHitLocation.Starboard
				}, 
				StarshipHitLocation.Port => new StarshipHitLocation[3]
				{
					StarshipHitLocation.Port,
					StarshipHitLocation.Fore,
					StarshipHitLocation.Aft
				}, 
				StarshipHitLocation.Starboard => new StarshipHitLocation[3]
				{
					StarshipHitLocation.Starboard,
					StarshipHitLocation.Fore,
					StarshipHitLocation.Aft
				}, 
				_ => new StarshipHitLocation[4]
				{
					StarshipHitLocation.Fore,
					StarshipHitLocation.Aft,
					StarshipHitLocation.Port,
					StarshipHitLocation.Starboard
				}, 
			};
			location = array[base.Owner.Random.Range(0, array.Length)];
		}
		base.Owner.GetOrCreate<UnitPartSideVulnerability>().Add(base.Fact, location);
		ShowBonusDamageMarker();
	}

	protected override void OnDeactivate()
	{
		HideBonusDamageMarker();
	}

	public void OnEventAboutToTrigger(RuleStarshipCalculateDamageForTarget evt)
	{
		UnitPartSideVulnerability.Entry entry = base.Owner.GetOptional<UnitPartSideVulnerability>()?.Get(base.Fact);
		if (entry != null && applyModificationTo == ApplyModificationTo.Hull && evt.ResultHitLocation == entry.ShipLocation)
		{
			evt.BonusDamage += m_BonusDamage;
			evt.ExtraDamageMod += m_ExtraDamageMod;
		}
	}

	public void OnEventDidTrigger(RuleStarshipCalculateDamageForTarget evt)
	{
	}

	public void OnEventAboutToTrigger(RuleStarshipPerformAttack evt)
	{
		UnitPartSideVulnerability.Entry entry = base.Owner.GetOptional<UnitPartSideVulnerability>()?.Get(base.Fact);
		if (entry != null && applyModificationTo == ApplyModificationTo.AllIncoming && evt.ResultHitLocation == entry.ShipLocation)
		{
			evt.BonusDamage += m_BonusDamage;
			evt.ExtraDamageMod += m_ExtraDamageMod;
		}
	}

	public void OnEventDidTrigger(RuleStarshipPerformAttack evt)
	{
	}

	private void ShowBonusDamageMarker()
	{
		UnitPartSideVulnerability.Entry entry = base.Owner.GetOptional<UnitPartSideVulnerability>()?.Get(base.Fact);
		if (entry != null)
		{
			if (entry.Marker == null)
			{
				entry.Marker = Object.Instantiate(m_BonusDamageMarker);
			}
			entry.Marker.transform.position = base.Owner.View.ViewTransform.position;
			entry.Marker.transform.rotation = base.Owner.View.ViewTransform.rotation;
			entry.Marker.transform.SetParent(base.Owner.View.ViewTransform);
			switch (entry.ShipLocation)
			{
			case StarshipHitLocation.Fore:
				entry.Marker.transform.position += base.Owner.View.ViewTransform.forward;
				break;
			case StarshipHitLocation.Port:
				entry.Marker.transform.rotation = Quaternion.LookRotation(-base.Owner.View.ViewTransform.right, base.Owner.View.ViewTransform.up);
				entry.Marker.transform.position -= base.Owner.View.ViewTransform.right;
				break;
			case StarshipHitLocation.Starboard:
				entry.Marker.transform.rotation = Quaternion.LookRotation(base.Owner.View.ViewTransform.right, base.Owner.View.ViewTransform.up);
				entry.Marker.transform.position += base.Owner.View.ViewTransform.right;
				break;
			case StarshipHitLocation.Aft:
				entry.Marker.transform.rotation = Quaternion.LookRotation(-base.Owner.View.ViewTransform.forward, base.Owner.View.ViewTransform.up);
				entry.Marker.transform.position -= base.Owner.View.ViewTransform.forward;
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
		ShowBonusDamageMarker();
	}

	protected override void OnViewWillDetach()
	{
		HideBonusDamageMarker();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
