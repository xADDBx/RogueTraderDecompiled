using System;
using Kingmaker.ElementsSystem;

namespace Kingmaker.BarkBanters;

[Serializable]
public class BanterConditions
{
	public bool ResponseRequired;

	public bool Unique;

	public int MinChapter;

	public int MaxChapter;

	public ConditionsChecker ExtraConditions;
}
