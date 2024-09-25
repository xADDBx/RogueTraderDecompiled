using System.Collections.Generic;

namespace Kingmaker.EntitySystem.Entities;

public class UnitReferenceComparer : IComparer<UnitReference>
{
	public static UnitReferenceComparer Instance = new UnitReferenceComparer();

	int IComparer<UnitReference>.Compare(UnitReference a, UnitReference b)
	{
		return a.CompareTo(b);
	}
}
