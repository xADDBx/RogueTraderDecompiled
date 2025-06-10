using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.MVVM.VM.ShipCustomization;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;
using UnityEngine.UI;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipUpgradeSlotPCView : ViewBase<ShipUpgradeSlotVM>, IVoidShipRotationHandler, ISubscriber
{
	[SerializeField]
	protected OwlcatMultiButton m_MultiButton;

	[SerializeField]
	protected Image m_CanUpgrade;

	[SerializeField]
	private CanvasGroup m_LinesBlock;

	[SerializeField]
	private GameObject[] m_ShipLines;

	[SerializeField]
	protected Sprite m_Upgrade;

	[SerializeField]
	protected Sprite m_Downgrade;

	protected UIContextMenu ContextMenuText;

	protected PartStarshipHull Hull;

	protected ContextMenuCollectionEntity HeaderEntity;

	protected ContextMenuCollectionEntity UpgradeEntity;

	protected ContextMenuCollectionEntity DowngradeEntity;

	protected override void BindViewImplementation()
	{
		if (base.ViewModel.IsLocked.Value)
		{
			m_MultiButton.SetActiveLayer(2);
			return;
		}
		AddDisposable(EventBus.Subscribe(this));
		SetShipLines();
		m_MultiButton.SetActiveLayer(0);
		AddDisposable(this.SetContextMenu(base.ViewModel.ContextMenu, isLeftClick: true));
		Hull = Game.Instance.Player.PlayerShip.Hull;
		SetupContextMenu();
		if ((bool)m_LinesBlock)
		{
			m_LinesBlock.alpha = 1f;
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	protected virtual void SetupContextMenu()
	{
		ContextMenuText = UIStrings.Instance.ContextMenu;
	}

	protected virtual void TryUpgrade()
	{
	}

	protected virtual void TryDowngrade()
	{
	}

	public void HandleOnRotationStart()
	{
		if ((bool)m_LinesBlock)
		{
			m_LinesBlock.alpha = 0f;
		}
	}

	public void HandleOnRotationStop()
	{
		if ((bool)m_LinesBlock)
		{
			m_LinesBlock.alpha = 1f;
		}
	}

	private void SetShipLines()
	{
		if (m_ShipLines.Length != 0)
		{
			GameObject[] shipLines = m_ShipLines;
			for (int i = 0; i < shipLines.Length; i++)
			{
				shipLines[i].SetActive(value: false);
			}
			switch (base.ViewModel.ShipType)
			{
			case PlayerShipType.SwordClassFrigate:
				m_ShipLines[0].SetActive(value: true);
				break;
			case PlayerShipType.FalchionClassFrigate:
				m_ShipLines[1].SetActive(value: true);
				break;
			case PlayerShipType.FirestormClassFrigate:
				m_ShipLines[2].SetActive(value: true);
				break;
			}
		}
	}
}
