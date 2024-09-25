using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;

public class CharInfoWeaponSetPCView : ViewBase<CharInfoWeaponSetVM>, IWidgetView
{
	[Header("Common Block")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private HandSlotView m_PrimaryHandView;

	[SerializeField]
	private HandSlotView m_SecondaryHandView;

	[Header("Stats Block")]
	[SerializeField]
	private TextMeshProUGUI m_DamageLabel;

	[SerializeField]
	private TextMeshProUGUI m_BulletsLabel;

	[SerializeField]
	private TextMeshProUGUI m_DistanceLabel;

	[SerializeField]
	private TextMeshProUGUI m_PenetrationLabel;

	[SerializeField]
	private GameObject m_BulletsBlock;

	[Header("Ability Block")]
	[SerializeField]
	private CharInfoWeaponSetAbilityPCView m_WeaponSetAbilityViewPrefab;

	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[Header("Damage Template")]
	[SerializeField]
	private Color m_templateTextColor;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		string template = GetTemplateText();
		BindHandSlot(m_PrimaryHandView, base.ViewModel.Primary);
		BindHandSlot(m_SecondaryHandView, base.ViewModel.Secondary);
		AddDisposable(base.ViewModel.SelectedHand.Subscribe(delegate(EquipSlotVM slotVm)
		{
			if (slotVm != null)
			{
				m_Title.text = slotVm.DisplayName.Value;
				if (slotVm.ItemEntity is ItemEntityWeapon { Blueprint: var blueprint } itemEntityWeapon)
				{
					DamageData resultDamage = GetWeaponStats(itemEntityWeapon).ResultDamage;
					m_DamageLabel.text = string.Format(template, resultDamage.MinValueBase, resultDamage.MaxValueBase);
					m_BulletsLabel.text = $"{blueprint.WarhammerMaxAmmo}";
					m_DistanceLabel.text = string.Format(template, itemEntityWeapon.AttackOptimalRange, itemEntityWeapon.AttackRange);
					m_PenetrationLabel.text = $"{blueprint.WarhammerPenetration}";
					m_BulletsBlock.SetActive(blueprint.WarhammerMaxAmmo > 0);
				}
			}
		}));
		AddDisposable(base.ViewModel.Abilities.ObserveAdd().Subscribe(delegate
		{
			DrawAbilities();
		}));
		AddDisposable(base.ViewModel.Abilities.ObserveRemove().Subscribe(delegate
		{
			DrawAbilities();
		}));
		DrawAbilities();
	}

	protected override void DestroyViewImplementation()
	{
		m_PrimaryHandView.Unbind();
		m_SecondaryHandView.Unbind();
		m_WidgetList.Clear();
	}

	private void BindHandSlot(HandSlotView slot, EquipSlotVM slotVm)
	{
		slot.Bind(slotVm);
		AddDisposable(base.ViewModel.SelectedHand.Subscribe(delegate(EquipSlotVM selectedVm)
		{
			if (selectedVm != null)
			{
				bool flag = selectedVm == slotVm;
				slot.Slot.SetActiveLayer(flag ? "Selected" : "Normal");
			}
		}));
		bool canConfirmClick = base.ViewModel.Primary.HasItem && base.ViewModel.Secondary.HasItem;
		slot.SetClickAction(delegate
		{
			base.ViewModel.SelectHand(slotVm);
		});
		slot.SetCanConfirmClick(canConfirmClick);
	}

	private void DrawAbilities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Abilities.ToArray(), m_WeaponSetAbilityViewPrefab);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharInfoWeaponSetVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharInfoWeaponSetVM;
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		gridConsoleNavigationBehaviour.AddRow<HandSlotView>(m_PrimaryHandView, m_SecondaryHandView);
		gridConsoleNavigationBehaviour.AddRow(m_WidgetList.VisibleEntries.Select((IWidgetView e) => (e as CharInfoWeaponSetAbilityPCView)?.NavigationEntity).ToList());
		return gridConsoleNavigationBehaviour;
	}

	private RuleCalculateStatsWeapon GetWeaponStats(ItemEntityWeapon weapon)
	{
		RuleCalculateStatsWeapon weaponStats = weapon.GetWeaponStats();
		BaseUnitEntity currentSelectedUnit = UIUtility.GetCurrentSelectedUnit();
		if (currentSelectedUnit == null)
		{
			return weaponStats;
		}
		return weapon.GetWeaponStats(currentSelectedUnit);
	}

	private string GetTemplateText()
	{
		return "<b>{0}<voffset=0.1em><size=74%>|</size></voffset></b><size=50%><color=#" + ColorUtility.ToHtmlStringRGBA(m_templateTextColor) + "> max</size> <size=66%><b>{1}</b></size></color>";
	}
}
