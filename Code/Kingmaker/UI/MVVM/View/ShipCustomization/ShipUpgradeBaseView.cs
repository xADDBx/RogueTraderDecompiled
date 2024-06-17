using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.DollRoom;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;
using UnityEngine.UI;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipUpgradeBaseView<TShipInventoryStash, TShipComponentSlot, TShipUpgradeStructure, TShipUpgradeProwRam, TShipSelectorWindow> : ViewBase<ShipUpgradeVm>
{
	[SerializeField]
	protected TShipInventoryStash m_InventoryStashView;

	[SerializeField]
	protected DollRoomTargetController m_CharacterController;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected TShipSelectorWindow m_SelectorWindowView;

	[Header("Slots")]
	[SerializeField]
	protected TShipComponentSlot m_PlasmaDrives;

	[SerializeField]
	protected TShipComponentSlot m_VoidShieldGenerator;

	[SerializeField]
	protected TShipComponentSlot m_AugerArray;

	[SerializeField]
	protected TShipComponentSlot m_ArmorPlating;

	[SerializeField]
	protected TShipComponentSlot[] m_WeaponSlots;

	[SerializeField]
	protected TShipUpgradeStructure m_UpgradeStructureSlot;

	[SerializeField]
	protected TShipUpgradeProwRam m_UpgradeProwRamSlot;

	[SerializeField]
	protected Image m_ExperiencePanel;

	[SerializeField]
	protected RectTransform UpgradeSlotsBlock;

	[SerializeField]
	private RectTransform m_MiddlePosition;

	[SerializeField]
	private RectTransform m_TopPosition;

	protected TooltipTemplateSimple m_ExperienceTooltip;

	public TShipInventoryStash ShipStash => m_InventoryStashView;

	protected ShipDollRoom ShipRoom => UIDollRooms.Instance?.ShipDollRoom;

	public virtual void Initialize()
	{
		m_FadeAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		ShowWindow();
		if (ShipRoom != null)
		{
			ShipRoom.Initialize(m_CharacterController);
			ShipRoom.SetupShip(Game.Instance.Player.PlayerShip);
		}
		m_ExperienceTooltip = new TooltipTemplateSimple(UIStrings.Instance.Tooltips.CurrentLevelExperience, UIStrings.Instance.ShipCustomization.ShipExperienceDescription);
		AddDisposable(m_ExperiencePanel.SetTooltip(m_ExperienceTooltip));
		SetUpgradePos();
		UpdateSlots();
	}

	protected override void DestroyViewImplementation()
	{
		HideWindow();
	}

	protected void RotateToDefaultPosition()
	{
		ShipRoom.RotateToDefaultPosition();
	}

	private void ShowWindow()
	{
		base.gameObject.SetActive(value: true);
		m_FadeAnimator.AppearAnimation(OnAppearEnd);
	}

	private void HideWindow()
	{
		ShipRoom?.Hide();
		m_FadeAnimator.DisappearAnimation(OnDisappearEnd);
	}

	private void OnAppearEnd()
	{
		ShipRoom?.Show();
	}

	private void OnDisappearEnd()
	{
		ContextMenuHelper.HideContextMenu();
		base.gameObject.SetActive(value: false);
	}

	private void SetUpgradePos()
	{
		bool flag = false;
		switch (base.ViewModel.ShipType)
		{
		case PlayerShipType.FalchionClassFrigate:
			flag = true;
			break;
		case PlayerShipType.FirestormClassFrigate:
			flag = true;
			break;
		}
		if (flag)
		{
			Vector3 localPosition = m_TopPosition.localPosition;
			UpgradeSlotsBlock.localPosition = new Vector3(localPosition.x, localPosition.y, localPosition.z);
		}
		else
		{
			Vector3 localPosition2 = m_MiddlePosition.localPosition;
			UpgradeSlotsBlock.localPosition = new Vector3(localPosition2.x, localPosition2.y, localPosition2.z);
		}
	}

	protected virtual void UpdateSlots()
	{
	}
}
