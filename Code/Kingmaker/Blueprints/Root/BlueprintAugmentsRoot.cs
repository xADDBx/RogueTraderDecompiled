using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[TypeId("2e6d0752c3d64ce5a901f1c1f62a5347")]
public class BlueprintAugmentsRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintAugmentsRoot>
	{
	}

	[SerializeField]
	private BlueprintAugmentSlotReference[] m_CommonSlots;

	private static HashSet<BlueprintAugmentSlot> s_CommonSlotsCache;

	public IReadOnlyCollection<BlueprintAugmentSlot> CommonSlots => s_CommonSlotsCache ?? (s_CommonSlotsCache = m_CommonSlots.Select((BlueprintAugmentSlotReference r) => r.Get()).ToHashSet());
}
