using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;

public class StatCheckLootMainPageBaseView<TStatCheckLootUnitCardView> : StatCheckLootPageView<StatCheckLootMainPageVM> where TStatCheckLootUnitCardView : StatCheckLootUnitCardBaseView
{
	[SerializeField]
	private List<TStatCheckLootUnitCardView> m_UnitCardSlots;

	protected override void InitializeImpl()
	{
		foreach (TStatCheckLootUnitCardView unitCardSlot in m_UnitCardSlots)
		{
			unitCardSlot.Initialize();
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.UpdateUnitSlots.Subscribe(UpdateUnitSlots));
		AddDisposable(base.ViewModel.ClearUnitSlots.Subscribe(ClearUnitSlots));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_InputLayer = null;
	}

	protected override void BuildNavigationImpl()
	{
		m_NavigationBehaviour.SetEntitiesHorizontal(m_UnitCardSlots);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "StatCheckLootMainPageBaseViewInput"
		});
		CreateInputImpl(m_InputLayer);
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer)
	{
	}

	protected void OnCheckStat()
	{
		base.ViewModel.CheckStat();
	}

	protected void OnClose()
	{
		base.ViewModel.CloseDialog();
	}

	protected void OnSwitchUnit()
	{
		base.ViewModel.SwitchUnit();
	}

	private void UpdateUnitSlots()
	{
		ClearUnitSlots();
		for (int i = 0; i < base.ViewModel.UnitSlotVMByStatType.Count; i++)
		{
			if (i >= m_UnitCardSlots.Count)
			{
				PFLog.UI.Error("StatCheckLootMainPageBaseView.UpdateUnitSlots - UnitSlotVMs count is more than slots count!");
				return;
			}
			TStatCheckLootUnitCardView val = m_UnitCardSlots[i];
			val.Bind(base.ViewModel.UnitSlotVMByStatType.ElementAt(i).Value);
			val.gameObject.SetActive(value: true);
		}
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	private void ClearUnitSlots()
	{
		foreach (TStatCheckLootUnitCardView unitCardSlot in m_UnitCardSlots)
		{
			unitCardSlot.Unbind();
			unitCardSlot.gameObject.SetActive(value: false);
		}
	}
}
