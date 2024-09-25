using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[TypeId("a3c4aa8ae7d66d44da653e6bab67ec1c")]
public class BlueprintCompanionStory : BlueprintScriptableObject
{
	[NotNull]
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Companion")]
	private BlueprintUnitReference m_Companion;

	public LocalizedString Title;

	public LocalizedString Description;

	public Sprite Image;

	[Header("For Employee")]
	public Gender Gender;

	public BlueprintUnit Companion => m_Companion?.Get();

	public void Unlock()
	{
		Game.Instance.Player.CompanionStories.Unlock(this);
	}
}
