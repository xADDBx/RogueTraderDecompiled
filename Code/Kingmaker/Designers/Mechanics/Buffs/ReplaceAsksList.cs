using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Buffs;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("e9d73d34a15a98e4cadc70d0b8623127")]
public class ReplaceAsksList : UnitBuffComponentDelegate, IHashable
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Asks")]
	private BlueprintUnitAsksListReference m_Asks;

	public BlueprintUnitAsksList Asks => m_Asks?.Get();

	protected override void OnActivate()
	{
		base.Owner.Asks.SetOverride(Asks);
		ObjectExtensions.Or(base.Owner.View, null)?.UpdateAsks();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Asks.SetOverride(null);
		ObjectExtensions.Or(base.Owner.View, null)?.UpdateAsks();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
