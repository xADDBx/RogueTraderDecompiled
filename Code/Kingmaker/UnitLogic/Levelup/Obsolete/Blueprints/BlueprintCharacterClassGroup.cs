using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[TypeId("d1f7319372575e6418cec56d6dc73519")]
public class BlueprintCharacterClassGroup : BlueprintScriptableObject
{
	[NotNull]
	[SerializeField]
	private BlueprintCharacterClassReference[] m_CharacterClasses = new BlueprintCharacterClassReference[0];

	public ReferenceArrayProxy<BlueprintCharacterClass> CharacterClasses
	{
		get
		{
			BlueprintReference<BlueprintCharacterClass>[] characterClasses = m_CharacterClasses;
			return characterClasses;
		}
	}
}
