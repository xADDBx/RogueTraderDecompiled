using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Markers;

public class LocalMapUnitMarkerVM : LocalMapMarkerVM
{
	private readonly BaseUnitEntity m_Unit;

	public UnitGroupMemory.UnitInfo UnitInfo { get; }

	public LocalMapUnitMarkerVM(UnitGroupMemory.UnitInfo unitInfo)
	{
		UnitInfo = unitInfo;
		BaseUnitEntity baseUnitEntity = (m_Unit = unitInfo.Unit);
		bool flag = unitInfo.Unit.Parts.GetAll<UnitPartInteractions>().Any((UnitPartInteractions c) => false);
		MarkerType = ((flag && !baseUnitEntity.Faction.IsPlayerEnemy) ? LocalMapMarkType.VeryImportantThing : LocalMapMarkType.Unit);
		Position.Value = baseUnitEntity.Position;
		IsVisible.Value = true;
		Description.Value = baseUnitEntity.CharacterName;
		IsEnemy.Value = baseUnitEntity.Faction.IsPlayerEnemy;
	}

	protected override void OnUpdateHandler()
	{
		if (m_Unit != null)
		{
			Position.Value = m_Unit.Position;
		}
	}

	public override Entity GetEntity()
	{
		return m_Unit;
	}
}
