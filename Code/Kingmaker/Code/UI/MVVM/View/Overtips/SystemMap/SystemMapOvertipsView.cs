using Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap;

public abstract class SystemMapOvertipsView<TOvertipSystemObjectView, TOvertipPlanetView, TOvertipAnomalyView> : ViewBase<SystemMapOvertipsVM> where TOvertipSystemObjectView : OvertipSystemObjectView where TOvertipPlanetView : OvertipPlanetView where TOvertipAnomalyView : OvertipAnomalyView
{
	[SerializeField]
	private RectTransform m_SystemObjectsContainer;

	[SerializeField]
	private TOvertipSystemObjectView m_OvertipSystemObjectPCView;

	[SerializeField]
	private TOvertipPlanetView m_OvertipPlanetPCView;

	[SerializeField]
	private TOvertipAnomalyView m_OvertipAnomalyPCView;

	public readonly ReactiveCollection<IFloatConsoleNavigationEntity> SystemMapObjectsCollection = new ReactiveCollection<IFloatConsoleNavigationEntity>();

	public void Initialize()
	{
		WidgetFactory.InstantiateWidget(m_OvertipSystemObjectPCView, 3, m_SystemObjectsContainer);
		WidgetFactory.InstantiateWidget(m_OvertipPlanetPCView, 3, m_SystemObjectsContainer);
		WidgetFactory.InstantiateWidget(m_OvertipAnomalyPCView, 3, m_SystemObjectsContainer);
	}

	protected override void BindViewImplementation()
	{
		base.ViewModel.SystemObjectOvertipsCollectionVM.Overtips.ForEach(AddSystemObjectView);
		base.ViewModel.PlanetOvertipsCollectionVM.Overtips.ForEach(AddPlanetView);
		base.ViewModel.AnomalyOvertipsCollectionVM.Overtips.ForEach(AddAnomalyView);
		AddDisposable(base.ViewModel.SystemObjectOvertipsCollectionVM.Overtips.ObserveAdd().Subscribe(delegate(CollectionAddEvent<OvertipEntitySystemObjectVM> value)
		{
			AddSystemObjectView(value.Value);
		}));
		AddDisposable(base.ViewModel.PlanetOvertipsCollectionVM.Overtips.ObserveAdd().Subscribe(delegate(CollectionAddEvent<OvertipEntityPlanetVM> value)
		{
			AddPlanetView(value.Value);
		}));
		AddDisposable(base.ViewModel.AnomalyOvertipsCollectionVM.Overtips.ObserveAdd().Subscribe(delegate(CollectionAddEvent<OvertipEntityAnomalyVM> value)
		{
			AddAnomalyView(value.Value);
		}));
		AddDisposable(base.ViewModel.PlanetOvertipsCollectionVM.Overtips.ObserveReset().Subscribe(ClearMapObjects));
	}

	protected override void DestroyViewImplementation()
	{
		ClearMapObjects();
	}

	private void AddSystemObjectView(OvertipEntitySystemObjectVM entitySystemObjectVM)
	{
		TOvertipSystemObjectView widget = WidgetFactory.GetWidget(m_OvertipSystemObjectPCView);
		widget.transform.SetParent(m_SystemObjectsContainer, worldPositionStays: false);
		widget.Bind(entitySystemObjectVM);
		if (widget.Visible)
		{
			SystemMapObjectsCollection.Add(widget);
		}
	}

	private void AddPlanetView(OvertipEntityPlanetVM entityPlanetVM)
	{
		TOvertipPlanetView widget = WidgetFactory.GetWidget(m_OvertipPlanetPCView);
		widget.transform.SetParent(m_SystemObjectsContainer, worldPositionStays: false);
		widget.Bind(entityPlanetVM);
		SystemMapObjectsCollection.Add(widget);
	}

	private void AddAnomalyView(OvertipEntityAnomalyVM entityAnomalyVM)
	{
		TOvertipAnomalyView widget = WidgetFactory.GetWidget(m_OvertipAnomalyPCView);
		widget.transform.SetParent(m_SystemObjectsContainer, worldPositionStays: false);
		widget.Bind(entityAnomalyVM);
		SystemMapObjectsCollection.Add(widget);
	}

	private void ClearMapObjects()
	{
		SystemMapObjectsCollection.ForEach(delegate(IFloatConsoleNavigationEntity obj)
		{
			WidgetFactory.DisposeWidget((MonoBehaviour)obj);
		});
		SystemMapObjectsCollection.Clear();
	}
}
