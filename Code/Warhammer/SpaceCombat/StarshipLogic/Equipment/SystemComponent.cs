using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Equipment;

[Serializable]
public abstract class SystemComponent : IHashable
{
	public enum UpgradeResult
	{
		Successful,
		MaxUpgrade,
		NotEnoughScrap,
		Error
	}

	public enum DowngradeResult
	{
		Successful,
		MinUpgrade,
		Error
	}

	public enum SystemComponentType
	{
		InternalStructure,
		ProwRam
	}

	public StarshipEntity Owner;

	[JsonProperty]
	public int UpgradeLevel { get; protected set; }

	public BlueprintShipSystemComponent Blueprint => BlueprintRoot.Instance.Progression.ShipSystemComponents;

	protected SystemComponent(StarshipEntity ship)
	{
		Owner = ship;
	}

	protected SystemComponent(JsonConstructorMark _)
	{
	}

	public abstract UpgradeResult Upgrade();

	public abstract DowngradeResult Downgrade();

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		int val = UpgradeLevel;
		result.Append(ref val);
		return result;
	}
}
