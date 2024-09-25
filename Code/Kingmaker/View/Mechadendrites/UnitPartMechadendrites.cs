using System.Collections.Generic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.Equipment;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Mechadendrites;

public class UnitPartMechadendrites : AbstractUnitPart, IHashable
{
	public readonly Dictionary<MechadendritesType, MechadendriteSettings> Mechadendrites = new Dictionary<MechadendritesType, MechadendriteSettings>();

	public void RegisterMechadendrite(MechadendriteSettings settings)
	{
		if (Mechadendrites.ContainsKey(settings.MechadendritesType))
		{
			Mechadendrites[settings.MechadendritesType] = settings;
		}
		else
		{
			Mechadendrites.Add(settings.MechadendritesType, settings);
		}
		UnitViewHandsEquipment unitViewHandsEquipment = (base.Owner.View as UnitEntityView)?.HandsEquipment;
		if (unitViewHandsEquipment?.Sets == null)
		{
			return;
		}
		foreach (var (_, weaponSet2) in unitViewHandsEquipment?.Sets)
		{
			weaponSet2.MainHand?.MatchVisuals();
			weaponSet2.OffHand?.MatchVisuals();
			weaponSet2.OffHand?.AttachModel(toHand: true);
		}
	}

	public void UnregisterMechadendrite(MechadendriteSettings settings)
	{
		if (Mechadendrites.ContainsKey(settings.MechadendritesType) && Mechadendrites[settings.MechadendritesType] == settings)
		{
			Mechadendrites.Remove(settings.MechadendritesType);
		}
		UnitViewHandsEquipment unitViewHandsEquipment = (base.Owner.View as UnitEntityView)?.HandsEquipment;
		if (unitViewHandsEquipment?.Sets == null)
		{
			return;
		}
		foreach (var (_, weaponSet2) in unitViewHandsEquipment?.Sets)
		{
			weaponSet2.MainHand?.MatchVisuals();
			weaponSet2.OffHand?.MatchVisuals();
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
