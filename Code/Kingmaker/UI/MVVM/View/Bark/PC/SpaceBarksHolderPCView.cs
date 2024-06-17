using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Bark.PC;

public class SpaceBarksHolderPCView : ViewBase<SpaceBarksHolderVM>
{
	[SerializeField]
	private SpaceBarkPCView m_SpaceBarkPCView;

	[SerializeField]
	private Transform m_BarksContainer;

	private readonly List<SpaceBarkPCView> m_BarksList = new List<SpaceBarkPCView>();

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.BarksVMs.ObserveAdd().Subscribe(delegate(CollectionAddEvent<SpaceBarkVM> value)
		{
			AddBarkView(value.Value);
		}));
		AddDisposable(base.ViewModel.ClearBarks.Subscribe(delegate
		{
			ClearBarks();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		ClearBarks();
	}

	private void AddBarkView(SpaceBarkVM vm)
	{
		SpaceBarkPCView widget = WidgetFactory.GetWidget(m_SpaceBarkPCView);
		widget.Initialize();
		widget.Bind(vm);
		widget.transform.SetParent(m_BarksContainer, worldPositionStays: false);
		m_BarksList.Add(widget);
	}

	private void ClearBarks()
	{
		m_BarksList.ForEach(WidgetFactory.DisposeWidget);
		m_BarksList.Clear();
	}
}
