using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class VendorsData : IHashable
{
	[JsonProperty]
	private readonly List<DetectedVendorData> m_DetectedVendors = new List<DetectedVendorData>();

	public IEnumerable<DetectedVendorData> DetectedVendors => m_DetectedVendors;

	public void AddToDetected(MechanicEntity entity, [NotNull] BlueprintArea area, [CanBeNull] BlueprintAreaPart areaPart, int chapter)
	{
		m_DetectedVendors.Add(new DetectedVendorData(entity, area, areaPart, chapter));
	}

	public void ClearDetected()
	{
		m_DetectedVendors.Clear();
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<DetectedVendorData> detectedVendors = m_DetectedVendors;
		if (detectedVendors != null)
		{
			for (int i = 0; i < detectedVendors.Count; i++)
			{
				Hash128 val = ClassHasher<DetectedVendorData>.GetHash128(detectedVendors[i]);
				result.Append(ref val);
			}
		}
		return result;
	}
}
