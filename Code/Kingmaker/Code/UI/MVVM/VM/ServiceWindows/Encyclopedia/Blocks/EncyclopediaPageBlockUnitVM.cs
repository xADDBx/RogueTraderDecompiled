using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Inspect;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockUnitVM : EncyclopediaPageBlockVM
{
	public InspectUnitsManager.UnitInfo UnitData;

	private BlueprintEncyclopediaBlockBestiaryUnit m_Unit => m_Block as BlueprintEncyclopediaBlockBestiaryUnit;

	public Sprite FullImage => m_Unit.FullImage;

	public BlueprintUnit Unit => m_Unit.Unit;

	public EncyclopediaPageBlockUnitVM(BlueprintEncyclopediaBlockBestiaryUnit block)
		: base(block)
	{
		UnitData = Game.Instance.Player.InspectUnitsManager.GetInfo(m_Unit.Unit);
	}
}
