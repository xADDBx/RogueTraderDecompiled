using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap;

public abstract class SectorMapOvertipsView<TOvertipSystemView, TOvertipRumourView> : ViewBase<SectorMapOvertipsVM> where TOvertipSystemView : OvertipSystemView where TOvertipRumourView : OvertipRumourView
{
	[SerializeField]
	private RectTransform m_SectorObjectsContainer;

	[FormerlySerializedAs("m_OvertipSystemPCView")]
	[SerializeField]
	private TOvertipSystemView m_OvertipSystemView;

	public readonly ReactiveCollection<TOvertipSystemView> SystemViewCollection = new ReactiveCollection<TOvertipSystemView>();

	[FormerlySerializedAs("m_OvertipRumourPCView")]
	[SerializeField]
	private TOvertipRumourView m_OvertipRumourView;

	[SerializeField]
	private CanvasGroup m_ClosePopupCanvas;

	[SerializeField]
	private CanvasGroup m_BlockPopupObject;

	protected override void BindViewImplementation()
	{
		base.ViewModel.SystemOvertipsCollectionVM.Overtips.ForEach(AddSystemView);
		base.ViewModel.RumoursOvertipsCollectionVM.Overtips.ForEach(AddRumourView);
		m_ClosePopupCanvas.blocksRaycasts = false;
		AddDisposable(base.ViewModel.SystemOvertipsCollectionVM.Overtips.ObserveAdd().Subscribe(delegate(CollectionAddEvent<OvertipEntitySystemVM> value)
		{
			AddSystemView(value.Value);
		}));
		AddDisposable(base.ViewModel.SystemOvertipsCollectionVM.Overtips.ObserveReset().Subscribe(ClearSystemViews));
		AddDisposable(base.ViewModel.RumoursOvertipsCollectionVM.Overtips.ObserveAdd().Subscribe(delegate(CollectionAddEvent<OvertipEntityRumourVM> value)
		{
			AddRumourView(value.Value);
		}));
		AddDisposable(base.ViewModel.BlockAllPopups.Subscribe(SetBlockPopupState));
		AddDisposable(base.ViewModel.CloseAllPopupsCanvas.Subscribe(delegate
		{
			m_ClosePopupCanvas.blocksRaycasts = base.ViewModel.CloseAllPopupsCanvas.Value;
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.CloseAllPopups, delegate
		{
			SetClosePopupState();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_ClosePopupCanvas.blocksRaycasts = false;
		ClearSystemViews();
	}

	private void SetBlockPopupState(bool state)
	{
		m_BlockPopupObject.blocksRaycasts = state;
	}

	protected void SetClosePopupState()
	{
		SystemViewCollection.ForEach(delegate(TOvertipSystemView v)
		{
			v.ClosePopup();
		});
	}

	private void AddSystemView(OvertipEntitySystemVM entitySystemVM)
	{
		TOvertipSystemView widget = WidgetFactory.GetWidget(m_OvertipSystemView);
		widget.transform.SetParent(m_SectorObjectsContainer, worldPositionStays: false);
		widget.Bind(entitySystemVM);
		SystemViewCollection.Add(widget);
	}

	private void ClearSystemViews()
	{
		SystemViewCollection.ForEach(WidgetFactory.DisposeWidget);
		SystemViewCollection.Clear();
	}

	private void AddRumourView(OvertipEntityRumourVM entityRumourVM)
	{
		TOvertipRumourView widget = WidgetFactory.GetWidget(m_OvertipRumourView);
		widget.transform.SetParent(m_SectorObjectsContainer, worldPositionStays: false);
		widget.Bind(entityRumourVM);
	}
}
