using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[TypeId("167f01bc1cd64d99b1d90896b08b6310")]
public class BlueprintProneRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintProneRoot>
	{
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintBuffReference m_ProneCommonBuff;

	public BlueprintBuff ProneCommonBuff => m_ProneCommonBuff;
}
