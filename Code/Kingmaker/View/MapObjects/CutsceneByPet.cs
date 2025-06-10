using System;
using Kingmaker.Blueprints;
using Kingmaker.Enums;

namespace Kingmaker.View.MapObjects;

[Serializable]
public struct CutsceneByPet
{
	public PetType PetyType;

	public CutsceneReference Cutscene;
}
