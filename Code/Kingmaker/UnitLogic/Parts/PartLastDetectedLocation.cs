using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartLastDetectedLocation : EntityPart, IHashable
{
	[JsonProperty]
	private BlueprintAreaReference m_Area;

	[JsonProperty]
	private BlueprintAreaPartReference m_AreaPart;

	[JsonProperty]
	private int m_Chapter;

	public BlueprintArea Area => m_Area;

	public BlueprintAreaPart AreaPart => m_AreaPart;

	public int Chapter => m_Chapter;

	public void DetectLocation([NotNull] BlueprintArea area, [CanBeNull] BlueprintAreaPart areaPart, int chapter)
	{
		m_Area = area.ToReference<BlueprintAreaReference>();
		m_AreaPart = areaPart.ToReference<BlueprintAreaPartReference>();
		m_Chapter = chapter;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(m_Area);
		result.Append(ref val2);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(m_AreaPart);
		result.Append(ref val3);
		result.Append(ref m_Chapter);
		return result;
	}
}
