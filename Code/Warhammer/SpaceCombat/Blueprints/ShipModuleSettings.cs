using System;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[Serializable]
public class ShipModuleSettings
{
	[SerializeField]
	private ShipModuleType m_moduleType;

	[SerializeField]
	private int m_crewCount;

	public ShipModuleType ModuleType => m_moduleType;

	public int CrewCount => m_crewCount;

	public ShipModuleSettings(ShipModuleType moduleType, int crewCount)
	{
		m_moduleType = moduleType;
		m_crewCount = crewCount;
	}
}
