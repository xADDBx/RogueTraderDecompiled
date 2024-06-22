using System;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Particles;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

public class PartStarshipShields : StarshipPart, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStarshipShields>, IEntityPartOwner
	{
		PartStarshipShields Shields { get; }
	}

	[JsonProperty]
	private StarshipSectorShields[] m_SectorShields;

	[JsonProperty]
	public bool IsActive { get; private set; }

	public BlueprintItemVoidShieldGenerator VoidShieldGenerator => (BlueprintItemVoidShieldGenerator)(base.Owner.GetRequired<PartStarshipHull>().HullSlots.VoidShieldGenerator.MaybeItem?.Blueprint);

	public int ShieldsSum => m_SectorShields.Sum((StarshipSectorShields x) => x.Current);

	public int ShieldsMaxSum => m_SectorShields.Sum((StarshipSectorShields x) => x.Max);

	public StarshipSectorShields WeakestSector => m_SectorShields.MinBy((StarshipSectorShields x) => x.Current);

	public PartStarshipShields()
	{
		m_SectorShields = new StarshipSectorShields[4]
		{
			new StarshipSectorShields(this, StarshipSectorShieldsType.Fore),
			new StarshipSectorShields(this, StarshipSectorShieldsType.Port),
			new StarshipSectorShields(this, StarshipSectorShieldsType.Starboard),
			new StarshipSectorShields(this, StarshipSectorShieldsType.Aft)
		};
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		if (VoidShieldGenerator != null)
		{
			InternalActivateShields();
		}
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		if (VoidShieldGenerator != null && base.Owner.IsInGame)
		{
			TurnOnShieldsFX();
		}
	}

	protected override void OnPrePostLoad()
	{
		base.OnPrePostLoad();
		StarshipSectorShields[] sectorShields = m_SectorShields;
		for (int i = 0; i < sectorShields.Length; i++)
		{
			sectorShields[i].PrePostLoad(this);
		}
	}

	public StarshipSectorShields GetShields(StarshipSectorShieldsType sector)
	{
		return m_SectorShields[(int)sector];
	}

	public StarshipSectorShieldsType GetShieldsType(StarshipHitLocation hitLocation)
	{
		return hitLocation switch
		{
			StarshipHitLocation.Fore => StarshipSectorShieldsType.Fore, 
			StarshipHitLocation.Port => StarshipSectorShieldsType.Port, 
			StarshipHitLocation.Starboard => StarshipSectorShieldsType.Starboard, 
			StarshipHitLocation.Aft => StarshipSectorShieldsType.Aft, 
			_ => throw new ArgumentOutOfRangeException("hitLocation", hitLocation, null), 
		};
	}

	public StarshipSectorShields GetShields(StarshipHitLocation hitLocation)
	{
		StarshipSectorShieldsType shieldsType = GetShieldsType(hitLocation);
		return GetShields(shieldsType);
	}

	public int GetCurrentShields(StarshipSectorShieldsType sector)
	{
		return GetShields(sector).Current;
	}

	public int GetCurrentShields(StarshipHitLocation hitLocation)
	{
		if (hitLocation != 0)
		{
			return GetShields(hitLocation).Current;
		}
		return 0;
	}

	public void ActivateShields()
	{
		InternalActivateShields();
		TurnOnShieldsFX();
	}

	private void InternalActivateShields()
	{
		if (!IsActive)
		{
			StarshipSectorShields[] sectorShields = m_SectorShields;
			foreach (StarshipSectorShields obj in sectorShields)
			{
				obj.Damage = 0;
				obj.Reinforced = false;
				obj.WasHitLastTurn = false;
			}
			IsActive = true;
		}
	}

	public void DeactivateShields(bool onUnequip = false)
	{
		if (IsActive)
		{
			StarshipSectorShields[] sectorShields = m_SectorShields;
			foreach (StarshipSectorShields starshipSectorShields in sectorShields)
			{
				starshipSectorShields.Damage = ((!onUnequip) ? starshipSectorShields.Max : 0);
				starshipSectorShields.Reinforced = false;
			}
			IsActive = false;
			TurnOffShieldsFX();
		}
	}

	public void ReinforceShield(StarshipSectorShieldsType sector1, StarshipSectorShieldsType? sector2)
	{
		if (!IsActive)
		{
			return;
		}
		StarshipSectorShields[] sectorShields = m_SectorShields;
		foreach (StarshipSectorShields starshipSectorShields in sectorShields)
		{
			if (starshipSectorShields.Sector == sector1 || starshipSectorShields.Sector == sector2)
			{
				if (starshipSectorShields.Current > 0)
				{
					starshipSectorShields.Reinforced = true;
					starshipSectorShields.WasHitLastTurn = false;
				}
			}
			else
			{
				starshipSectorShields.Reinforced = false;
			}
		}
	}

	public void RestoreWeakestSector(int pctOfMaxStrength)
	{
		if (!IsActive)
		{
			return;
		}
		int num = 0;
		StarshipSectorShields starshipSectorShields = null;
		for (int i = 0; i < m_SectorShields.Length; i++)
		{
			StarshipSectorShields starshipSectorShields2 = m_SectorShields[i];
			int num2 = starshipSectorShields2.Max - starshipSectorShields2.Current;
			if (num2 > 0 && (starshipSectorShields == null || num2 > num))
			{
				starshipSectorShields = starshipSectorShields2;
				num = num2;
			}
		}
		if (starshipSectorShields != null)
		{
			starshipSectorShields.Damage = Math.Max(0, starshipSectorShields.Damage - starshipSectorShields.Max * pctOfMaxStrength / 100);
			TurnOnShieldSectorFX(starshipSectorShields);
		}
	}

	public int MinPercent()
	{
		int num = 100;
		for (int i = 0; i < m_SectorShields.Length; i++)
		{
			int val = Mathf.RoundToInt((float)m_SectorShields[i].Current * 100f / (float)m_SectorShields[i].Max);
			num = Math.Min(num, val);
		}
		return num;
	}

	public (int absorbedDamage, int shieldStrengthLoss) Absorb(StarshipHitLocation hitLocation, int damage, DamageType damageType, bool isPredictionOnly)
	{
		if (hitLocation == StarshipHitLocation.Undefined)
		{
			return (absorbedDamage: 0, shieldStrengthLoss: 0);
		}
		return AbsorbSector((int)(hitLocation - 1), damage, damageType, isPredictionOnly);
	}

	public void DamageAdjacent2Sectors(StarshipHitLocation hitLocation, int damage)
	{
		int num;
		int num2;
		switch (hitLocation)
		{
		default:
			return;
		case StarshipHitLocation.Fore:
		case StarshipHitLocation.Aft:
			num = 1;
			num2 = 2;
			break;
		case StarshipHitLocation.Port:
		case StarshipHitLocation.Starboard:
			num = 0;
			num2 = 3;
			break;
		}
		AddSectorDamage(m_SectorShields[num], damage);
		AddSectorDamage(m_SectorShields[num2], damage);
	}

	private void AddSectorDamage(StarshipSectorShields sectorShields, int shieldStrengthLoss)
	{
		shieldStrengthLoss = Math.Min(sectorShields.Max - sectorShields.Damage, shieldStrengthLoss);
		if (shieldStrengthLoss > 0)
		{
			sectorShields.WasHitLastTurn = true;
		}
		sectorShields.Damage += shieldStrengthLoss;
		if (sectorShields.Current <= 0)
		{
			sectorShields.Reinforced = false;
			TurnOffShieldSectorFX(sectorShields);
		}
		EventBus.RaiseEvent((IStarshipEntity)base.Owner, (Action<IShieldAbsorbsDamageHandler>)delegate(IShieldAbsorbsDamageHandler h)
		{
			h.HandleShieldAbsorbsDamage(shieldStrengthLoss, sectorShields.Current, sectorShields.Sector);
		}, isCheckRuntime: true);
	}

	private (int absorbedDamage, int shieldDamage) AbsorbSector(int sectorIndex, int damage, DamageType damageType, bool isPredictionOnly)
	{
		StarshipSectorShields starshipSectorShields = m_SectorShields[sectorIndex];
		int num;
		int num2;
		if (damageType == DamageType.Ram)
		{
			num = starshipSectorShields.Current / starshipSectorShields.RamAbsorbMod;
			num2 = starshipSectorShields.Current;
		}
		else
		{
			int num3 = (starshipSectorShields.Reinforced ? 200 : 100);
			BlueprintItemVoidShieldGenerator voidShieldGenerator = VoidShieldGenerator;
			if (voidShieldGenerator != null && voidShieldGenerator.HasExtraResistance && VoidShieldGenerator.damageExtraResistance == damageType)
			{
				num3 += VoidShieldGenerator.extraResistanceDamageReductionPercent;
			}
			num = Math.Min(starshipSectorShields.Current * num3 / 100, damage);
			num2 = num * 100 / num3;
		}
		if (!isPredictionOnly)
		{
			AddSectorDamage(starshipSectorShields, num2);
		}
		return (absorbedDamage: num, shieldDamage: num2);
	}

	private void TurnOnShieldsFX()
	{
		GameObject voidShieldTurnOn = BlueprintRoot.Instance.HitSystemRoot.VoidShieldTurnOn;
		SpawnAndAttachShieldFX(voidShieldTurnOn);
	}

	private void TurnOffShieldsFX()
	{
		GameObject voidShieldTurnOff = BlueprintRoot.Instance.HitSystemRoot.VoidShieldTurnOff;
		SpawnAndAttachShieldFX(voidShieldTurnOff);
	}

	public void TurnOnShieldSectorFX(StarshipSectorShields sectorShields)
	{
		SpawnAndAttachShieldFX(sectorShields.Sector switch
		{
			StarshipSectorShieldsType.Fore => BlueprintRoot.Instance.HitSystemRoot.VoidShieldTurnOnFront, 
			StarshipSectorShieldsType.Aft => BlueprintRoot.Instance.HitSystemRoot.VoidShieldTurnOnBack, 
			StarshipSectorShieldsType.Port => BlueprintRoot.Instance.HitSystemRoot.VoidShieldTurnOnLeft, 
			StarshipSectorShieldsType.Starboard => BlueprintRoot.Instance.HitSystemRoot.VoidShieldTurnOnRight, 
			_ => BlueprintRoot.Instance.HitSystemRoot.VoidShieldTurnOn, 
		});
	}

	public void TurnOffShieldSectorFX(StarshipSectorShields sectorShields)
	{
		SpawnAndAttachShieldFX(sectorShields.Sector switch
		{
			StarshipSectorShieldsType.Fore => BlueprintRoot.Instance.HitSystemRoot.VoidShieldTurnOffFront, 
			StarshipSectorShieldsType.Aft => BlueprintRoot.Instance.HitSystemRoot.VoidShieldTurnOffBack, 
			StarshipSectorShieldsType.Port => BlueprintRoot.Instance.HitSystemRoot.VoidShieldTurnOffLeft, 
			StarshipSectorShieldsType.Starboard => BlueprintRoot.Instance.HitSystemRoot.VoidShieldTurnOffRight, 
			_ => BlueprintRoot.Instance.HitSystemRoot.VoidShieldTurnOff, 
		});
	}

	private void SpawnAndAttachShieldFX(GameObject shieldFx)
	{
		StarshipView starshipView = base.Owner.View?.GetComponentInChildren<StarshipView>();
		if (!(null != shieldFx) || !(null != starshipView))
		{
			return;
		}
		GameObject gameObject = FxHelper.SpawnFxOnPoint(shieldFx, starshipView.transform.position);
		if (gameObject != null)
		{
			FxLocator fxLocator = starshipView.particleSnapMap.FxLocators.Find((FxLocator x) => x != null && x.name == "Locator_ShieldPositionFX");
			if (fxLocator != null)
			{
				gameObject.transform.localScale = fxLocator.transform.localToWorldMatrix.lossyScale;
				gameObject.transform.rotation = fxLocator.transform.localToWorldMatrix.rotation;
				gameObject.GetComponent<SnapToLocator>().Attach(starshipView.particleSnapMap, fxLocator.transform);
			}
			gameObject.SetActive(value: true);
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (!isTurnBased || EventInvokerExtensions.MechanicEntity != base.Owner)
		{
			return;
		}
		StarshipSectorShields[] sectorShields = m_SectorShields;
		foreach (StarshipSectorShields starshipSectorShields in sectorShields)
		{
			if (starshipSectorShields.WasHitLastTurn)
			{
				starshipSectorShields.Reinforced = false;
				starshipSectorShields.WasHitLastTurn = false;
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		StarshipSectorShields[] sectorShields = m_SectorShields;
		if (sectorShields != null)
		{
			for (int i = 0; i < sectorShields.Length; i++)
			{
				Hash128 val2 = ClassHasher<StarshipSectorShields>.GetHash128(sectorShields[i]);
				result.Append(ref val2);
			}
		}
		bool val3 = IsActive;
		result.Append(ref val3);
		return result;
	}
}
