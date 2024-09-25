using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Markers;

public class LocalMapCharacterMarkerVM : LocalMapMarkerVM
{
	private readonly BaseUnitEntity m_Unit;

	public LocalMapCharacterMarkerVM(BaseUnitEntity unit)
	{
		m_Unit = unit;
		MarkerType = LocalMapMarkType.PlayerCharacter;
		IsVisible.Value = true;
		Description.Value = unit.CharacterName;
		Position.Value = unit.Position;
		Portrait.Value = unit.Portrait.SmallPortrait;
		IsSelected.Value = Game.Instance.SelectionCharacter.IsSelected(unit);
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
		return null;
	}
}
