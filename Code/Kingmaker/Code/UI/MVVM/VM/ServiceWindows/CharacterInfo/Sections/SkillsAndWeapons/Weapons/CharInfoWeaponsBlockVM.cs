using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;

public class CharInfoWeaponsBlockVM : CharInfoComponentVM
{
	public readonly List<CharInfoWeaponSetVM> WeaponSets = new List<CharInfoWeaponSetVM>();

	public CharInfoWeaponsBlockVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void DisposeImplementation()
	{
		ClearSets();
	}

	protected override void RefreshData()
	{
		ClearSets();
		CreateSets();
	}

	private void CreateSets()
	{
		for (int i = 0; i < 2; i++)
		{
			HandsEquipmentSet handsEquipmentSet = Unit.Value.Body.HandsEquipmentSets[i];
			if (!handsEquipmentSet.IsEmpty())
			{
				WeaponSets.Add(new CharInfoWeaponSetVM(handsEquipmentSet));
			}
		}
	}

	private void ClearSets()
	{
		WeaponSets.ForEach(delegate(CharInfoWeaponSetVM vm)
		{
			vm.Dispose();
		});
		WeaponSets.Clear();
	}
}
