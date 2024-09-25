using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Owlcat.Runtime.UI.SelectionGroup;

namespace Kingmaker.Code.UI.MVVM.VM.Retrain;

public class RespecCharacterVM : SelectionGroupEntityVM
{
	public readonly UnitPortraitPartVM UnitPortraitPartVM;

	public string CharacterName;

	private EntityRef<BaseUnitEntity> m_UnitRef;

	public BaseUnitEntity Unit => m_UnitRef.Entity;

	public RespecCharacterVM(BaseUnitEntity unit)
		: base(allowSwitchOff: false)
	{
		m_UnitRef = unit;
		AddDisposable(UnitPortraitPartVM = new UnitPortraitPartVM());
		UnitPortraitPartVM.SetUnitData(unit);
		CharacterName = unit.CharacterName;
	}

	protected override void DoSelectMe()
	{
	}

	protected override void DisposeImplementation()
	{
	}
}
