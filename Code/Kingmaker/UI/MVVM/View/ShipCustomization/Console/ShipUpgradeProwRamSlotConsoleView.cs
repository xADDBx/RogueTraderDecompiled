using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.Console;

public class ShipUpgradeProwRamSlotConsoleView : ShipUpgradeProwRamSlotPCView, IConfirmClickHandler, IConsoleEntity, IFloatConsoleNavigationEntity, IConsoleNavigationEntity
{
	[SerializeField]
	private GameObject m_Hint;

	[SerializeField]
	protected TextMeshProUGUI m_HintText;

	private bool m_IsAvailable = true;

	private HintData m_HintData;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_MultiButton.SetTooltip(base.ViewModel.ShipUpgradeTooltip));
		m_HintText.text = UIStrings.Instance.ShipCustomization.UpgradeProwRam;
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_Hint.SetActive(value: false);
	}

	public void SetFocus(bool value)
	{
		m_MultiButton.SetFocus(value);
		m_Hint.SetActive(value);
	}

	public bool IsValid()
	{
		if (m_MultiButton.IsValid())
		{
			return m_IsAvailable;
		}
		return false;
	}

	public bool CanConfirmClick()
	{
		return base.ViewModel.ContextMenu.Value != null;
	}

	public void OnConfirmClick()
	{
		m_MultiButton.ShowContextMenu(base.ViewModel.ContextMenu.Value);
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public Vector2 GetPosition()
	{
		return m_MultiButton.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public void SetAvailable(bool value)
	{
		m_IsAvailable = value;
	}
}
