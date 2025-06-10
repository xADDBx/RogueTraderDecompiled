using System;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class PetInterestSettings
{
	public CutsceneByPet[] PetCutscenes;

	public float Radius;

	public int ReactionCooldown;

	public float PetPathLimit = float.MaxValue;
}
