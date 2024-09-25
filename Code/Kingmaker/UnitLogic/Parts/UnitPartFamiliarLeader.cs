using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartFamiliarLeader : BaseUnitPart, IHashable
{
	[JsonProperty]
	private readonly List<FamiliarData> m_EquippedFamiliars = new List<FamiliarData>();

	[JsonProperty(PropertyName = "m_LastEquippedFamiliar")]
	public BlueprintUnit LastEquippedFamiliar { get; private set; }

	[CanBeNull]
	public AbstractUnitEntity FirstFamiliar => m_EquippedFamiliars.FirstItem((FamiliarData i) => i.Unit != null)?.Unit;

	public bool HasEquippedFamiliar(BlueprintUnit blueprint)
	{
		return m_EquippedFamiliars.HasItem((FamiliarData i) => i.Unit?.Blueprint == blueprint);
	}

	public bool HasEquippedFamiliar(BlueprintUnit blueprint, EntityFactSource source)
	{
		return GetEquippedFamiliar(blueprint, source) != null;
	}

	[CanBeNull]
	public FamiliarData GetEquippedFamiliar(BlueprintUnit blueprint, EntityFactSource source)
	{
		return m_EquippedFamiliars.FirstItem((FamiliarData x) => x.Unit?.Blueprint == blueprint && x.Source == source);
	}

	public bool HasEquippedFamiliar(AbstractUnitEntity unit)
	{
		return GetEquippedFamiliar(unit) != null;
	}

	[CanBeNull]
	private FamiliarData GetEquippedFamiliar(AbstractUnitEntity unit)
	{
		return m_EquippedFamiliars.FirstItem((FamiliarData i) => i.Unit == unit);
	}

	public void AddEquippedFamiliar([NotNull] AbstractUnitEntity familiar, EntityFactSource source)
	{
		FamiliarData familiarData = m_EquippedFamiliars.FirstItem((FamiliarData i) => i.Unit == familiar);
		if (familiarData == null)
		{
			familiarData = new FamiliarData(familiar, source);
			m_EquippedFamiliars.Add(familiarData);
			LastEquippedFamiliar = familiar.Blueprint;
		}
	}

	public void RemoveEquippedFamiliar(AbstractUnitEntity familiar)
	{
		FamiliarData equippedFamiliar = GetEquippedFamiliar(familiar);
		if (equippedFamiliar != null)
		{
			m_EquippedFamiliars.Remove(equippedFamiliar);
		}
	}

	public void UpdateFamiliarsVisibility()
	{
		foreach (FamiliarData equippedFamiliar in m_EquippedFamiliars)
		{
			UnitPartFamiliar unitPartFamiliar = equippedFamiliar.Unit?.GetRequired<UnitPartFamiliar>();
			if (unitPartFamiliar != null)
			{
				unitPartFamiliar.UpdateViewVisibility();
				unitPartFamiliar.UpdateIsInGameState(base.Owner.IsInGame);
			}
		}
	}

	protected override void OnApplyPostLoadFixes()
	{
		m_EquippedFamiliars.RemoveAll((FamiliarData i) => i.Unit == null);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<FamiliarData> equippedFamiliars = m_EquippedFamiliars;
		if (equippedFamiliars != null)
		{
			for (int i = 0; i < equippedFamiliars.Count; i++)
			{
				Hash128 val2 = ClassHasher<FamiliarData>.GetHash128(equippedFamiliars[i]);
				result.Append(ref val2);
			}
		}
		Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(LastEquippedFamiliar);
		result.Append(ref val3);
		return result;
	}
}
