using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("ead9e50bd3dd7af4587e3749489a30bf")]
public class ReplaceSourceBone : UnitFactComponentDelegate, IHashable
{
	public string SourceBone;

	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<UnitPartVisualChanges>().AddReplacementBone(SourceBone);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<UnitPartVisualChanges>()?.RemoveReplacementBone(SourceBone);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
