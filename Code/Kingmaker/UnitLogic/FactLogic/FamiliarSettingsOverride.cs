using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Visual.Critters;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("fe0b014f28284226bf2b77374b0a7328")]
public class FamiliarSettingsOverride : EntityFactComponentDelegate<MechanicEntity>, IHashable
{
	[SerializeField]
	[ValidateNotNull]
	private FamiliarSettings m_FamiliarSettings = new FamiliarSettings();

	public FamiliarSettings FamiliarSettings => m_FamiliarSettings;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
