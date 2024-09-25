using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("1cc0a2001d424bb8b1e30329f0f8693d")]
public class AddKingmakerEquipmentEntity : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private KingmakerEquipmentEntityReference m_EquipmentEntity;

	public KingmakerEquipmentEntity EquipmentEntity => m_EquipmentEntity;

	protected override void OnActivateOrPostLoad()
	{
		if (TryGetCharacter(out var resultCharacter))
		{
			resultCharacter.AddEquipmentEntities(GetEquipmentLinks());
		}
	}

	protected override void OnDeactivate()
	{
		if (TryGetCharacter(out var resultCharacter))
		{
			resultCharacter.RemoveEquipmentEntities(GetEquipmentLinks());
		}
	}

	private bool TryGetCharacter(out Character resultCharacter)
	{
		resultCharacter = null;
		UnitEntityView view = base.Owner.View;
		if (view == null)
		{
			return false;
		}
		resultCharacter = view.CharacterAvatar;
		return resultCharacter != null;
	}

	private IEnumerable<EquipmentEntityLink> GetEquipmentLinks()
	{
		Race race = base.Owner.Progression.Race?.RaceId ?? Race.Human;
		return EquipmentEntity.GetLinks(base.Owner.Gender, race);
	}

	protected override void OnViewDidAttach()
	{
		OnActivateOrPostLoad();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
