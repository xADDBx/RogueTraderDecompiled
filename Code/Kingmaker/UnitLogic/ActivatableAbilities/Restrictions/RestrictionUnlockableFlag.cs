using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

[ComponentName("AA restriction unit condition")]
[TypeId("98ea8514f7a42b54f83de5990ce09d68")]
public class RestrictionUnlockableFlag : ActivatableAbilityRestriction, IUnlockableFlagReference, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("NeededFlag")]
	private BlueprintUnlockableFlagReference m_NeededFlag;

	public bool Invert;

	public BlueprintUnlockableFlag NeededFlag => m_NeededFlag?.Get();

	protected override bool IsAvailable()
	{
		return NeededFlag.IsUnlocked != Invert;
	}

	UnlockableFlagReferenceType IUnlockableFlagReference.GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (flag == NeededFlag)
		{
			return UnlockableFlagReferenceType.Check;
		}
		return UnlockableFlagReferenceType.None;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
