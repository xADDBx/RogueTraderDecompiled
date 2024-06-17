using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Features.Advancements;

[Serializable]
[TypeId("c534c916ed7a46609b21f58f312e4770")]
public abstract class BlueprintStatAdvancement : BlueprintFeature
{
	public new class Reference : BlueprintReference<BlueprintStatAdvancement>
	{
	}

	private enum Source
	{
		Career,
		Origin,
		Other
	}

	[SerializeField]
	private Source m_Source;

	public abstract int ValuePerRank { get; }

	public abstract StatType Stat { get; }

	public ModifierDescriptor ModifierDescriptor => m_Source switch
	{
		Source.Career => ModifierDescriptor.CareerAdvancement, 
		Source.Origin => ModifierDescriptor.OriginAdvancement, 
		_ => ModifierDescriptor.OtherAdvancement, 
	};
}
