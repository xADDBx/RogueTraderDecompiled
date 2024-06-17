using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[TypeId("0ec4fe8b79e9541479efc2a8517046be")]
public class BlueprintUnitType : BlueprintScriptableObject
{
	public StatType KnowledgeStat;

	public Sprite Image;

	public LocalizedString Name;

	public LocalizedString Description;

	[SerializeField]
	[FormerlySerializedAs("SignatureAbilities")]
	private BlueprintUnitFactReference[] m_SignatureAbilities = new BlueprintUnitFactReference[0];

	public ReferenceArrayProxy<BlueprintUnitFact> SignatureAbilities
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] signatureAbilities = m_SignatureAbilities;
			return signatureAbilities;
		}
	}
}
