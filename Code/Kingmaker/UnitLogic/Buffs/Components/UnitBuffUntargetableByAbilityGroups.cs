using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("4804457c2d8b4eac8e9fa5a6d9de486b")]
[ClassInfoBox("Unit is untargetable by single target abilities having one of blocked ability groups")]
public class UnitBuffUntargetableByAbilityGroups : UnitBuffComponentDelegate, IHashable
{
	[SerializeField]
	private List<BlueprintAbilityGroupReference> m_BlockedGroups;

	public List<BlueprintAbilityGroupReference> BlockedGroups => m_BlockedGroups;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
