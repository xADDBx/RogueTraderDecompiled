using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[ComponentName("Add stat bonus from ability value")]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[TypeId("db89aba32de193149950d16cc9d588f8")]
public class StarshipShieldEnhancement : UnitBuffComponentDelegate, IHashable
{
	public bool applyToAll;

	[HideIf("applyToAll")]
	public StarshipSectorShieldsType shieldType;

	public int bonusFlat;

	public int bonusPct;

	public bool isReinforcement;

	public Buff OwnerBuff => base.Buff;

	public bool ValidFor(StarshipSectorShieldsType type)
	{
		if (!applyToAll)
		{
			return type == shieldType;
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
