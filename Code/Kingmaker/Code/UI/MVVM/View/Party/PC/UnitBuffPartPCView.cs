using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.Other;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Party.PC;

public class UnitBuffPartPCView : ViewBase<UnitBuffPartVM>
{
	[SerializeField]
	private BuffPCView m_BuffView;

	[SerializeField]
	private RectTransform m_MainContainer;

	[SerializeField]
	private RectTransform m_AdditionalContainer;

	[SerializeField]
	private OwlcatSelectable m_AdditionalTrigger;

	private readonly List<BuffPCView> m_BuffList = new List<BuffPCView>();

	[SerializeField]
	private int m_BuffsLimit = 5;

	[HideInInspector]
	public BoolReactiveProperty IsHovered = new BoolReactiveProperty();

	public bool HasBuffs => m_BuffList.Any();

	protected override void BindViewImplementation()
	{
		m_AdditionalContainer.gameObject.SetActive(value: false);
		DrawBuffs();
		AddDisposable(base.ViewModel.Buffs.ObserveAdd().Subscribe(delegate
		{
			DrawBuffs();
		}));
		AddDisposable(base.ViewModel.Buffs.ObserveRemove().Subscribe(delegate
		{
			DrawBuffs();
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.Buffs.ObserveReset(), delegate
		{
			Clear();
		}));
		AddDisposable(m_AdditionalTrigger.OnPointerClickAsObservable().Subscribe(delegate
		{
			m_AdditionalContainer.gameObject.SetActive(!m_AdditionalContainer.gameObject.activeSelf);
		}));
	}

	private void DrawBuffs()
	{
		bool flag = base.ViewModel.Buffs.Count > m_BuffsLimit;
		if (m_AdditionalTrigger.gameObject.activeSelf != flag)
		{
			m_AdditionalTrigger.gameObject.SetActive(flag);
		}
		for (int i = 0; i < m_BuffList.Count; i++)
		{
			IViewModel viewModel = m_BuffList[i].GetViewModel();
			bool flag2 = false;
			foreach (BuffVM buff in base.ViewModel.Buffs)
			{
				if (buff == viewModel)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				WidgetFactory.DisposeWidget(m_BuffList[i]);
				m_BuffList.RemoveAt(i);
				i--;
			}
		}
		for (int j = 0; j < base.ViewModel.Buffs.Count; j++)
		{
			BuffVM buffVM = base.ViewModel.Buffs[j];
			bool flag3 = false;
			foreach (BuffPCView buff2 in m_BuffList)
			{
				if (buff2.GetViewModel() == buffVM)
				{
					flag3 = true;
					break;
				}
			}
			if (!flag3)
			{
				BuffPCView widget = WidgetFactory.GetWidget(m_BuffView);
				widget.SetHoverProperty(IsHovered);
				widget.Bind(buffVM);
				widget.transform.SetParent((flag && j >= m_BuffsLimit) ? m_AdditionalContainer : m_MainContainer, worldPositionStays: false);
				m_BuffList.Add(widget);
			}
		}
		m_AdditionalTrigger.transform.SetAsLastSibling();
	}

	public void HideAdditionalBuffs()
	{
		m_AdditionalContainer.gameObject.SetActive(value: false);
	}

	protected override void DestroyViewImplementation()
	{
		Clear();
	}

	private void Clear()
	{
		m_BuffList.ForEach(WidgetFactory.DisposeWidget);
		m_BuffList.Clear();
	}
}
