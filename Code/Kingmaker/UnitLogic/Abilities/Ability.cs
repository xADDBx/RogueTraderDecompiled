using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities;

[JsonObject]
public sealed class Ability : MechanicEntityFact, IHashable
{
	[JsonProperty]
	private bool m_Hidden;

	[JsonProperty]
	public readonly AbilityData Data;

	[NotNull]
	private MechanicsContext m_Context;

	[JsonProperty]
	[CanBeNull]
	public BlueprintAbilityResource UsagesPerDayResource { get; set; }

	public new BlueprintAbility Blueprint => (BlueprintAbility)base.Blueprint;

	public MechanicsContext Context => m_Context;

	public override MechanicsContext MaybeContext => Context;

	public override bool Hidden
	{
		get
		{
			if (!base.Hidden && !m_Hidden && !Blueprint.Hidden && !(base.SourceItem is ItemEntityUsable))
			{
				return !Data.IsVisible();
			}
			return true;
		}
	}

	public Ability(BlueprintAbility blueprint, MechanicEntity ownerUnit)
		: base(blueprint)
	{
		Data = new AbilityData(this, ownerUnit);
		m_Context = new MechanicsContext(ownerUnit, ownerUnit, blueprint);
	}

	[UsedImplicitly]
	private Ability(JsonConstructorMark _)
	{
	}

	protected override void OnPrePostLoad()
	{
		Data.PrePostLoad(this);
		m_Context = new MechanicsContext((MechanicEntity)base.Owner, (MechanicEntity)base.Owner, Blueprint);
	}

	public bool Hide()
	{
		return m_Hidden = true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(UsagesPerDayResource);
		result.Append(ref val2);
		result.Append(ref m_Hidden);
		Hash128 val3 = ClassHasher<AbilityData>.GetHash128(Data);
		result.Append(ref val3);
		return result;
	}
}
