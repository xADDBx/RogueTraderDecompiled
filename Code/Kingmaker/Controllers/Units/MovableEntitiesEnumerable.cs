using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers.Units;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct MovableEntitiesEnumerable
{
	public struct Enumerator
	{
		private readonly List<AbstractUnitEntity> m_Entities;

		private readonly BaseUnitEntity m_PlayerShip;

		private AbstractUnitEntity m_Current;

		private int m_Index;

		public AbstractUnitEntity Current => m_Current;

		public Enumerator(List<AbstractUnitEntity> entities, BaseUnitEntity playerShip)
		{
			m_Entities = entities;
			m_PlayerShip = playerShip;
			m_Current = null;
			m_Index = 0;
		}

		public bool MoveNext()
		{
			if (m_Index < m_Entities.Count)
			{
				m_Current = m_Entities[m_Index];
				m_Index++;
				return true;
			}
			if (m_Index == m_Entities.Count && m_PlayerShip != null)
			{
				m_Current = m_PlayerShip;
				m_Index++;
				return true;
			}
			return false;
		}
	}

	public Enumerator GetEnumerator()
	{
		BaseUnitEntity playerShip = ((Game.Instance.CurrentMode == GameModeType.GlobalMap) ? Game.Instance.Player.PlayerShip : null);
		return new Enumerator(Game.Instance.State.AllAwakeUnits.ToTempList(), playerShip);
	}
}
