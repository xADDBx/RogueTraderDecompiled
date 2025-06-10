using System.Collections.Generic;
using System.Linq;
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

	[SerializeField]
	[Tooltip("If true, immunity will apply to all abilities, except those in Groups.")]
	private bool m_InvertCondition;

	public bool IsBlocked(IEnumerable<BlueprintAbilityGroup> groups)
	{
		if (!m_InvertCondition)
		{
			return m_BlockedGroups.Any((BlueprintAbilityGroupReference p) => groups.Contains(p));
		}
		return m_BlockedGroups.All((BlueprintAbilityGroupReference p) => !groups.Contains(p));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
