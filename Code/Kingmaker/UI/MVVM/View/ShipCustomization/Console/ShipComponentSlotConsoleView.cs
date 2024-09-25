using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.Console;

public class ShipComponentSlotConsoleView : ShipComponentSlotBaseView<ItemSlotConsoleView>, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotPCView.Bind(base.ViewModel);
	}

	public void SetFocus(bool value)
	{
		if (base.ViewModel.ShowPossibleTarget.Value)
		{
			if (value)
			{
				OnHoverStart();
			}
			else
			{
				OnHoverEnd();
			}
		}
		m_ItemSlotPCView.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_ItemSlotPCView.IsValid();
	}

	public Vector2 GetPosition()
	{
		return m_ItemSlotPCView.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public bool CanConfirmClick()
	{
		return true;
	}

	public void OnConfirmClick()
	{
		OnClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
