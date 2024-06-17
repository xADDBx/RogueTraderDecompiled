using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Equipment;

public class ItemEntityPlasmaDrives : StarshipItemEntity<BlueprintItemPlasmaDrives>, IHashable
{
	private ModifiableValue.Modifier m_modSpeed;

	private ModifiableValue.Modifier m_modInertia;

	private ModifiableValue.Modifier m_modEvasion;

	public ItemEntityPlasmaDrives(BlueprintItemPlasmaDrives bpItem)
		: base(bpItem)
	{
	}

	protected ItemEntityPlasmaDrives(JsonConstructorMark _)
		: base(_)
	{
	}

	public override void OnDidEquipped([NotNull] MechanicEntity wielder)
	{
		base.OnDidEquipped(wielder);
		m_modSpeed = base.Owner.GetStatOptional(StatType.WarhammerInitialAPBlue).AddItemModifier(base.Blueprint.Speed, this);
		m_modInertia = base.Owner.GetStatOptional(StatType.Inertia).AddItemModifier(base.Blueprint.Inertia, this);
		if (base.Blueprint.Evasion != 0)
		{
			m_modEvasion = base.Owner.GetStatOptional(StatType.Evasion).AddItemModifier(base.Blueprint.Evasion, this);
		}
	}

	public override void OnWillUnequip()
	{
		base.Owner.GetStatOptional(StatType.WarhammerInitialAPBlue).RemoveModifier(m_modSpeed);
		base.Owner.GetStatOptional(StatType.Inertia).RemoveModifier(m_modInertia);
		if (m_modEvasion != null)
		{
			base.Owner.GetStatOptional(StatType.Evasion).RemoveModifier(m_modEvasion);
		}
		base.OnWillUnequip();
	}

	protected override void OnPostLoad()
	{
		if (base.Wielder != null)
		{
			OnDidEquipped(base.Wielder);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
