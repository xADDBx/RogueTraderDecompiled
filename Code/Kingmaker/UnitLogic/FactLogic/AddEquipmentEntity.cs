using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("3714c1933917d084184a8731a9f39b4c")]
public class AddEquipmentEntity : UnitFactComponentDelegate, IHashable
{
	public EquipmentEntityLink EquipmentEntity;

	protected override void OnActivateOrPostLoad()
	{
		UnitEntityView view = base.Owner.View;
		if (!(view == null))
		{
			Character characterAvatar = view.CharacterAvatar;
			if (!(characterAvatar == null))
			{
				characterAvatar.AddEquipmentEntity(EquipmentEntity);
			}
		}
	}

	protected override void OnDeactivate()
	{
		UnitEntityView view = base.Owner.View;
		if (!(view == null))
		{
			Character characterAvatar = view.CharacterAvatar;
			if (!(characterAvatar == null))
			{
				characterAvatar.RemoveEquipmentEntity(EquipmentEntity);
			}
		}
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
