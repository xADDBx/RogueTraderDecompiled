using System;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.DialogSystem;

[Serializable]
public class CheckData
{
	public StatType Type;

	public int DC;

	public CheckData(StatType type, int dc)
	{
		Type = type;
		DC = dc;
	}
}
