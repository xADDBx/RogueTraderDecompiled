using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintStarship))]
[TypeId("7522ae23c06843eba5950a7056984633")]
public class BlueprintStarshipStarSystemSettingsComponent : BlueprintComponent
{
	[SerializeField]
	private GameObject m_StarSystemVisual;

	[SerializeField]
	[Tooltip("Will hide this visual on starsystem maps")]
	private GameObject m_BaseVisual;

	[SerializeField]
	private BlueprintStarshipSoundSettings.Reference m_SoundSettings;

	public GameObject StarSystemVisual => m_StarSystemVisual;

	public GameObject BaseVisual => m_BaseVisual;

	[CanBeNull]
	public BlueprintStarshipSoundSettings SoundSettings => m_SoundSettings?.Get();
}
