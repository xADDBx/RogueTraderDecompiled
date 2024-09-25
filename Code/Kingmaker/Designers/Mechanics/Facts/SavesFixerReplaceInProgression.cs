using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("a4d76e9e9069f9f459eb483dba093d5a")]
public class SavesFixerReplaceInProgression : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("OldFeature")]
	private BlueprintFeatureReference m_OldFeature;

	[SerializeField]
	[FormerlySerializedAs("NewFeature")]
	private BlueprintFeatureReference m_NewFeature;

	public BlueprintFeature OldFeature => m_OldFeature?.Get();

	public BlueprintFeature NewFeature => m_NewFeature?.Get();

	protected override void OnActivate()
	{
		if (base.Owner.Facts.Contains(OldFeature))
		{
			base.Owner.Progression.ReplaceFeature(OldFeature, NewFeature);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
