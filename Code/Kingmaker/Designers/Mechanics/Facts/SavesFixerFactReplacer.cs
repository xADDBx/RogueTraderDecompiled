using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("1bf823d1b20208344afc8e458c867c82")]
public class SavesFixerFactReplacer : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("OldFacts")]
	private BlueprintUnitFactReference[] m_OldFacts;

	[SerializeField]
	[FormerlySerializedAs("NewFacts")]
	private BlueprintUnitFactReference[] m_NewFacts;

	public ReferenceArrayProxy<BlueprintUnitFact> OldFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] oldFacts = m_OldFacts;
			return oldFacts;
		}
	}

	public ReferenceArrayProxy<BlueprintUnitFact> NewFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] newFacts = m_NewFacts;
			return newFacts;
		}
	}

	protected override void OnActivate()
	{
		foreach (BlueprintUnitFact oldFact in OldFacts)
		{
			base.Owner.Facts.Remove(oldFact);
		}
		foreach (BlueprintUnitFact newFact in NewFacts)
		{
			base.Owner.AddFact(newFact)?.AddSource(base.Fact);
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
