using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Mechanics.Damage;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickWeaponSetView : TooltipBaseBrickView<TooltipBrickWeaponSetVM>
{
	[SerializeField]
	protected HandSlotView m_HandSlotView;

	[SerializeField]
	private GameObject m_BulletsBlock;

	[SerializeField]
	private GameObject m_RateOfFireBlock;

	[SerializeField]
	private CharInfoWeaponSetAbilityPCView m_WeaponSetAbilityViewPrefab;

	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_DamageLabel;

	[SerializeField]
	private TextMeshProUGUI m_BulletsLabel;

	[SerializeField]
	private TextMeshProUGUI m_PenetrationLabel;

	[SerializeField]
	private TextMeshProUGUI m_DistanceLabel;

	[SerializeField]
	private TextMeshProUGUI m_RateOfFireLabel;

	[Header("Damage Template")]
	[SerializeField]
	private Color m_templateTextColor;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title);
		}
		m_HandSlotView.Bind(base.ViewModel.EquipSlot);
		BlueprintItemWeapon blueprint = base.ViewModel.Weapon.Blueprint;
		m_Title.text = base.ViewModel.Weapon.Name;
		string format = UIConfig.Instance.WeaponSetTextFormat.Replace("{color}", ColorUtility.ToHtmlStringRGBA(m_templateTextColor));
		DamageData resultDamage = base.ViewModel.Weapon.GetWeaponStats().ResultDamage;
		m_DamageLabel.text = string.Format(format, resultDamage.MinValueBase.ToString(), resultDamage.MaxValueBase.ToString());
		m_PenetrationLabel.text = blueprint.WarhammerPenetration.ToString();
		m_DistanceLabel.text = string.Format(format, base.ViewModel.Weapon.AttackOptimalRange.ToString(), base.ViewModel.Weapon.AttackRange.ToString());
		m_BulletsLabel.text = blueprint.WarhammerMaxAmmo.ToString();
		m_BulletsBlock.SetActive(blueprint.WarhammerMaxAmmo > 0);
		m_RateOfFireLabel.text = blueprint.RateOfFire.ToString();
		m_RateOfFireBlock.SetActive(blueprint.RateOfFire > 0);
		DrawAbilities();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_WidgetList.Clear();
		m_TextHelper.Dispose();
	}

	private void DrawAbilities()
	{
		m_WidgetList.Clear();
		m_WidgetList.DrawEntries(base.ViewModel.Abilities.ToArray(), m_WeaponSetAbilityViewPrefab);
	}
}
