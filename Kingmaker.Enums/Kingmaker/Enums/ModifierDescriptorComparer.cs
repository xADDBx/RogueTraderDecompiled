using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Enums;

public class ModifierDescriptorComparer : IComparer<ModifierDescriptor>
{
	[NotNull]
	public static ModifierDescriptorComparer Instance = new ModifierDescriptorComparer();

	public static ModifierDescriptor[] SortedValues;

	[NotNull]
	private readonly int[] m_Order;

	private ModifierDescriptorComparer()
	{
		m_Order = new int[EnumUtils.GetMaxValue<ModifierDescriptor>() + 1];
		for (int i = 0; i < m_Order.Length; i++)
		{
			m_Order[i] = -1;
		}
		List<ModifierDescriptor> list = new List<ModifierDescriptor>(new ModifierDescriptor[5]
		{
			ModifierDescriptor.None,
			ModifierDescriptor.UntypedStackable,
			ModifierDescriptor.UntypedUnstackable,
			ModifierDescriptor.Difficulty,
			ModifierDescriptor.Polymorph
		});
		int j;
		for (j = 0; j < list.Count; j++)
		{
			m_Order[(int)list[j]] = j;
		}
		foreach (ModifierDescriptor value in EnumUtils.GetValues<ModifierDescriptor>())
		{
			if (m_Order[(int)value] < 0)
			{
				j++;
				m_Order[(int)value] = j;
				list.Add(value);
			}
		}
		SortedValues = list.ToArray();
	}

	public int Compare(ModifierDescriptor x, ModifierDescriptor y)
	{
		return m_Order[(int)x].CompareTo(m_Order[(int)y]);
	}
}
