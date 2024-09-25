using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[TypeId("d5360515209f38448bb1eed62787cfca")]
public class BlueprintColonyEventsRoot : BlueprintScriptableObject
{
	[Serializable]
	public class ColonyEventToTimer : IHashable
	{
		[SerializeField]
		[JsonProperty]
		private BlueprintColonyEventReference m_ColonyEvent;

		[SerializeField]
		[JsonProperty]
		public int Segments;

		public BlueprintColonyEvent ColonyEvent => m_ColonyEvent?.Get();

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(m_ColonyEvent);
			result.Append(ref val);
			result.Append(ref Segments);
			return result;
		}
	}

	[Serializable]
	public class Reference : BlueprintReference<BlueprintColonyEventsRoot>
	{
	}

	public ColonyEventToTimer[] Events;

	public int EventCountInColony = 2;
}
