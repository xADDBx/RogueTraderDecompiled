using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class BuffingAreaSettings : InteractionSettings
{
	[SerializeField]
	[FormerlySerializedAs("Buff")]
	private BlueprintBuffReference m_Buff;

	public Rounds BuffDuration;

	public BlueprintBuff Buff => m_Buff?.Get();
}
