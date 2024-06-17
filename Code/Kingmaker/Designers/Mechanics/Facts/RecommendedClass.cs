using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Add favorite class for level up")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("f4c35e78c727d424191ff5ed5a57dc5e")]
public class RecommendedClass : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("FavoriteClass")]
	private BlueprintCharacterClassReference m_FavoriteClass;

	public BlueprintCharacterClass FavoriteClass => m_FavoriteClass?.Get();

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
