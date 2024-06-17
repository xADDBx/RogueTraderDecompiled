using System;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class HiddenSettings : InteractionSettings
{
	public int DifficultyClass;

	public StatType StatType;
}
