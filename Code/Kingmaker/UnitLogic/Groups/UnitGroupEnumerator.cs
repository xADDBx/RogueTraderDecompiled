using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.UnitLogic.Groups;

public struct UnitGroupEnumerator
{
	private readonly List<UnitReference> m_List;

	private int m_Index;

	public BaseUnitEntity Current
	{
		get
		{
			if (m_Index >= 0 && m_Index < m_List.Count)
			{
				return m_List[m_Index].ToBaseUnitEntity();
			}
			return null;
		}
	}

	public UnitGroupEnumerator(List<UnitReference> list)
	{
		m_Index = -1;
		m_List = list;
	}

	public bool MoveNext()
	{
		while (++m_Index < m_List.Count && Current == null)
		{
		}
		return Current != null;
	}
}
