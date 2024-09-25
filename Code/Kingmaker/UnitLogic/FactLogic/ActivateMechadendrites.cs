using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.View.Mechadendrites;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("0c14fd969c24449eb3a6b346f6e5a4cc")]
public class ActivateMechadendrites : UnitFactComponentDelegate, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		UnitPartMechadendrites orCreate = base.Owner.GetOrCreate<UnitPartMechadendrites>();
		if (orCreate != null && !(base.Owner.View == null))
		{
			MechadendriteSettings[] componentsInChildren = base.Owner.View.GetComponentsInChildren<MechadendriteSettings>();
			foreach (MechadendriteSettings settings in componentsInChildren)
			{
				orCreate.RegisterMechadendrite(settings);
			}
		}
	}

	protected override void OnDeactivate()
	{
		UnitPartMechadendrites optional = base.Owner.GetOptional<UnitPartMechadendrites>();
		if (optional == null || base.Owner.View == null)
		{
			return;
		}
		foreach (KeyValuePair<MechadendritesType, MechadendriteSettings> mechadendrite in optional.Mechadendrites)
		{
			optional.UnregisterMechadendrite(mechadendrite.Value);
		}
		base.Owner.View.Data?.Remove<UnitPartMechadendrites>();
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
