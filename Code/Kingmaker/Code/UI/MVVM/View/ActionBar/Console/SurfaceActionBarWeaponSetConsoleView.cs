using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.Console;

public class SurfaceActionBarWeaponSetConsoleView : ViewBase<SurfaceActionBarPartWeaponSetVM>
{
	[SerializeField]
	private SurfaceActionBarSlotWeaponConsoleView m_MainHandWeapon;

	[SerializeField]
	private SurfaceActionBarSlotWeaponConsoleView m_OffHandWeapon;

	[SerializeField]
	private TextMeshProUGUI[] m_WeaponSetIndexes;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.MainHandWeapon.CombineLatest(base.ViewModel.OffHandWeapon, (ItemSlotVM _, ItemSlotVM _) => true).Subscribe(delegate
		{
			if (base.ViewModel.IsTwoHanded)
			{
				m_MainHandWeapon.Bind(base.ViewModel.MainHandWeapon.Value);
				m_OffHandWeapon.Bind(base.ViewModel.MainHandWeapon.Value);
			}
			else
			{
				m_MainHandWeapon.Bind(base.ViewModel.MainHandWeapon.Value);
				m_OffHandWeapon.Bind(base.ViewModel.OffHandWeapon.Value);
			}
			m_OffHandWeapon.SetFakeMode(base.ViewModel.IsTwoHanded);
		}));
		TextMeshProUGUI[] weaponSetIndexes = m_WeaponSetIndexes;
		for (int i = 0; i < weaponSetIndexes.Length; i++)
		{
			weaponSetIndexes[i].text = UIUtility.ArabicToRoman(base.ViewModel.Index + 1);
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}
