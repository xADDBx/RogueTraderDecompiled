using System;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;

public class ItemSlotConsoleView : ItemSlotBaseView, IConsoleNavigationEntity, IConsoleEntity
{
	public IObservable<Unit> OnConfirmClickAsObservable => m_MainButton.OnConfirmClickAsObservable();

	public IObservable<Unit> OnLongConfirmClickAsObservable => m_MainButton.OnLongConfirmClickAsObservable();

	public IObservable<Unit> OnFunc01ClickAsObservable => m_MainButton.OnFunc01ClickAsObservable();

	public IObservable<Unit> OnLongFunc01ClickAsObservable => m_MainButton.OnLongFunc01ClickAsObservable();

	public IObservable<Unit> OnFunc02ClickAsObservable => m_MainButton.OnFunc02ClickAsObservable();

	public IObservable<Unit> OnLongFunc02ClickAsObservable => m_MainButton.OnLongFunc02ClickAsObservable();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SubscribeInteractions();
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SubscribeInteractions()
	{
	}

	public void SetSelected(bool value)
	{
		if (!(m_MainButton == null))
		{
			m_MainButton.SetFocus(value);
		}
	}

	public void SetWaitingForSlotState(bool state)
	{
		if (!(m_MainButton == null))
		{
			if (state)
			{
				m_MainButton.SetActiveLayer("WaitingForSlot");
			}
			else
			{
				m_MainButton.SetActiveLayer((base.ViewModel.ItemEntity == null) ? "Empty" : "Busy");
			}
		}
	}
}
