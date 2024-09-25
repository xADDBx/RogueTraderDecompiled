using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartPolymorphed : BaseUnitPart, IHashable
{
	[JsonProperty]
	[CanBeNull]
	public BlueprintPortrait OriginalPortrait;

	[JsonProperty]
	[CanBeNull]
	public PortraitData OriginalPortraitData;

	[JsonProperty]
	public bool RestorePortrait;

	public Polymorph Component { get; private set; }

	public GameObject ViewReplacement { get; set; }

	[CanBeNull]
	public BlueprintUnit ReplaceBlueprintForInspection => Component?.ReplaceUnitForInspection;

	public void Setup([NotNull] Polymorph component)
	{
		Component = component;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(OriginalPortrait);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<PortraitData>.GetHash128(OriginalPortraitData);
		result.Append(ref val3);
		result.Append(ref RestorePortrait);
		return result;
	}
}
