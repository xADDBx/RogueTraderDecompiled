using System;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class StatDamageEntry
{
	public StatType Attribute;

	[SerializeField]
	[FormerlySerializedAs("DamageBuff")]
	private BlueprintBuffReference m_DamageBuff;

	[SerializeField]
	[FormerlySerializedAs("DrainBuff")]
	private BlueprintBuffReference m_DrainBuff;

	public BlueprintBuff DamageBuff => m_DamageBuff?.Get();

	public BlueprintBuff DrainBuff => m_DrainBuff?.Get();
}
