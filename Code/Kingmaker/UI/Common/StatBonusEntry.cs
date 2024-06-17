using JetBrains.Annotations;
using Kingmaker.Enums;

namespace Kingmaker.UI.Common;

public struct StatBonusEntry
{
	public int Bonus;

	[CanBeNull]
	public string Source;

	public ModifierDescriptor Descriptor;

	public static int Compare(StatBonusEntry x, StatBonusEntry y)
	{
		return ModifierDescriptorComparer.Instance.Compare(x.Descriptor, y.Descriptor);
	}
}
