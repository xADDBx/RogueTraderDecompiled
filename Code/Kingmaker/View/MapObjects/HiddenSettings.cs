using System;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class HiddenSettings : InteractionSettings
{
	public int DifficultyClass;

	public StatType StatType;
}
