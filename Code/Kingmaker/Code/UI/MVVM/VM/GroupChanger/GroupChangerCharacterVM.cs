using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.GroupChanger;

public class GroupChangerCharacterVM : VMBase
{
	public readonly ReactiveCommand<GroupChangerCharacterVM> Click = new ReactiveCommand<GroupChangerCharacterVM>();

	public readonly ReactiveProperty<bool> IsInParty = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsLock = new ReactiveProperty<bool>(initialValue: false);

	public readonly BlueprintUnit BpUnit;

	public readonly UnitReference UnitRef;

	public readonly UnitHealthPartVM HealthPartVm;

	public readonly UnitBuffPartVM BuffPartVm;

	public readonly UnitPortraitPartVM PortraitPartVm;

	public readonly bool IsRemovable;

	public readonly bool IsLevelUp;

	public readonly bool IsCharacterOverload;

	public readonly bool IsPartyOverload;

	public readonly string CharacterName;

	public readonly int CharacterLevel;

	public readonly Encumbrance CharacterEncumbrance;

	private bool m_IsFocused;

	public bool IsFocused => m_IsFocused;

	public GroupChangerCharacterVM(UnitReference unit, bool isLock)
	{
		BaseUnitEntity baseUnitEntity = (BaseUnitEntity)unit.Entity;
		BpUnit = baseUnitEntity.Blueprint;
		UnitRef = unit;
		IsLock.Value = isLock;
		IsRemovable = baseUnitEntity.State.CanRemoveFromParty;
		IsLevelUp = LevelUpController.CanLevelUp(baseUnitEntity);
		CharacterEncumbrance = baseUnitEntity.EncumbranceData?.Value ?? Encumbrance.Light;
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		Encumbrance encumbrance = ((loadedAreaState == null || !loadedAreaState.Settings.CapitalPartyMode) ? Game.Instance.Player.Encumbrance : Encumbrance.Light);
		IsCharacterOverload = CharacterEncumbrance == Encumbrance.Overload;
		AreaPersistentState loadedAreaState2 = Game.Instance.LoadedAreaState;
		IsPartyOverload = (loadedAreaState2 == null || !loadedAreaState2.Settings.CapitalPartyMode) && encumbrance == Encumbrance.Overload;
		CharacterName = baseUnitEntity.CharacterName;
		CharacterLevel = baseUnitEntity.Progression.CharacterLevel;
		AddDisposable(HealthPartVm = new UnitHealthPartVM(baseUnitEntity));
		AddDisposable(BuffPartVm = new UnitBuffPartVM(baseUnitEntity));
		AddDisposable(PortraitPartVm = new UnitPortraitPartVM());
		BuffPartVm.SetUnitData(baseUnitEntity);
		PortraitPartVm.SetUnitData(baseUnitEntity);
	}

	public void OnClick()
	{
		Click.Execute(this);
	}

	public void SetIsInParty(bool isInParty)
	{
		IsInParty.Value = isInParty;
	}

	public void SetFocused(bool isFocused)
	{
		m_IsFocused = isFocused;
	}

	protected override void DisposeImplementation()
	{
	}
}
