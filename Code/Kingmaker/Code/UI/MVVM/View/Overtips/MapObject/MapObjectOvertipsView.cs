using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject;

public abstract class MapObjectOvertipsView<TOvertipTransitionView, TOvertipMapObjectSimpleView, TOvertipMapObjectInteractionView, TOvertipDestructibleObjectView, TOvertipMapObjectTwitchDropsView> : ViewBase<MapObjectOvertipsVM> where TOvertipTransitionView : OvertipTransitionView where TOvertipMapObjectSimpleView : OvertipMapObjectSimpleView where TOvertipMapObjectInteractionView : OvertipMapObjectInteractionView where TOvertipDestructibleObjectView : OvertipDestructibleObjectView where TOvertipMapObjectTwitchDropsView : OvertipMapObjectTwitchDropsView
{
	[SerializeField]
	private RectTransform m_TargetContainer;

	[SerializeField]
	[FormerlySerializedAs("m_OvertipTransitionPCView")]
	private TOvertipTransitionView m_OvertipTransitionView;

	[SerializeField]
	[FormerlySerializedAs("m_OvertipMapObjectSimplePCView")]
	private TOvertipMapObjectSimpleView m_OvertipMapObjectSimpleView;

	[SerializeField]
	[FormerlySerializedAs("m_OvertipMapObjectInteractionPCView")]
	private TOvertipMapObjectInteractionView m_OvertipMapObjectInteractionView;

	[SerializeField]
	private TOvertipDestructibleObjectView m_OvertipDestructibleObjectView;

	[SerializeField]
	private TOvertipMapObjectTwitchDropsView m_OvertipMapObjectTwitchDropsView;

	private Queue<MonoBehaviour> m_FreeTransitionOvertips = new Queue<MonoBehaviour>();

	private Queue<MonoBehaviour> m_FreeMapObjectSimpleOvertips = new Queue<MonoBehaviour>();

	private Queue<MonoBehaviour> m_FreeMapObjectInteractionOvertips = new Queue<MonoBehaviour>();

	private Queue<MonoBehaviour> m_FreeDestructibleObjectOvertips = new Queue<MonoBehaviour>();

	private Queue<MonoBehaviour> m_FreeMapObjectTwitchDropsOvertips = new Queue<MonoBehaviour>();

	private Dictionary<BaseOvertipMapObjectVM, MonoBehaviour> m_ActiveOvertips = new Dictionary<BaseOvertipMapObjectVM, MonoBehaviour>();

	private bool m_ClearDeadOvertips;

	private void PrewarmOvertips<T>(Queue<MonoBehaviour> queue, T prefab, int count) where T : MonoBehaviour
	{
		WidgetFactory.InstantiateWidget(prefab, count, m_TargetContainer);
	}

	public void Initialize()
	{
		PrewarmOvertips(m_FreeTransitionOvertips, m_OvertipTransitionView, 5);
		PrewarmOvertips(m_FreeMapObjectSimpleOvertips, m_OvertipMapObjectSimpleView, 15);
		PrewarmOvertips(m_FreeMapObjectInteractionOvertips, m_OvertipMapObjectInteractionView, 15);
	}

	protected override void BindViewImplementation()
	{
		if (base.ViewModel.TransitionOvertipsCollectionVM != null)
		{
			AddDisposable(base.ViewModel.TransitionOvertipsCollectionVM.Overtips.ObserveRemove().Subscribe(delegate
			{
				m_ClearDeadOvertips = true;
			}));
			AddDisposable(base.ViewModel.TransitionOvertipsCollectionVM.Overtips.ObserveReset().Subscribe(delegate
			{
				m_ClearDeadOvertips = true;
			}));
		}
		if (base.ViewModel.MapInteractionObjectOvertipsCollectionVM.Overtips != null)
		{
			AddDisposable(base.ViewModel.MapInteractionObjectOvertipsCollectionVM.Overtips.ObserveRemove().Subscribe(delegate
			{
				m_ClearDeadOvertips = true;
			}));
			AddDisposable(base.ViewModel.MapInteractionObjectOvertipsCollectionVM.Overtips.ObserveReset().Subscribe(delegate
			{
				m_ClearDeadOvertips = true;
			}));
		}
		if (base.ViewModel.DestructibleObjectOvertipsCollectionVM != null)
		{
			AddDisposable(base.ViewModel.DestructibleObjectOvertipsCollectionVM.Overtips.ObserveRemove().Subscribe(delegate
			{
				m_ClearDeadOvertips = true;
			}));
			AddDisposable(base.ViewModel.DestructibleObjectOvertipsCollectionVM.Overtips.ObserveReset().Subscribe(delegate
			{
				m_ClearDeadOvertips = true;
			}));
		}
	}

	protected override void DestroyViewImplementation()
	{
		foreach (MonoBehaviour value in m_ActiveOvertips.Values)
		{
			FreeOvertip(value);
		}
		m_ActiveOvertips.Clear();
	}

	public void Update()
	{
		using (Counters.Overtips?.Measure())
		{
			using (ProfileScope.New("VM visibility"))
			{
				if (base.ViewModel?.TransitionOvertipsCollectionVM != null)
				{
					foreach (OvertipTransitionVM overtip in base.ViewModel.TransitionOvertipsCollectionVM.Overtips)
					{
						bool flag = overtip.MapObjectEntity != null && overtip.IsVisibleForPlayer.Value && !overtip.HideFromScreen && overtip.IsInCameraFrustum;
						bool flag2 = m_ActiveOvertips.Get(overtip) != null;
						if (flag != flag2)
						{
							if (flag)
							{
								AddTransition(overtip);
							}
							else
							{
								RemoveTransition(overtip);
							}
						}
					}
				}
				if (base.ViewModel?.MapInteractionObjectOvertipsCollectionVM != null)
				{
					foreach (OvertipMapObjectVM overtip2 in base.ViewModel.MapInteractionObjectOvertipsCollectionVM.Overtips)
					{
						bool flag3 = overtip2.MapObjectEntity != null && overtip2.MapObjectEntity.IsVisibleForPlayer && !overtip2.HideFromScreen && overtip2.IsInCameraFrustum;
						bool flag4 = m_ActiveOvertips.Get(overtip2) != null;
						if (flag3 != flag4)
						{
							if (flag3)
							{
								AddMapObject(overtip2);
							}
							else
							{
								RemoveMapObject(overtip2);
							}
						}
					}
				}
				if (base.ViewModel?.DestructibleObjectOvertipsCollectionVM != null)
				{
					foreach (OvertipDestructibleObjectVM overtip3 in base.ViewModel.DestructibleObjectOvertipsCollectionVM.Overtips)
					{
						bool flag5 = overtip3.MapObjectEntity != null && overtip3.MapObjectEntity.IsVisibleForPlayer && !overtip3.HideFromScreen;
						bool flag6 = m_ActiveOvertips.Get(overtip3) != null;
						if (flag5 != flag6)
						{
							if (flag5)
							{
								AddDestructibleObject(overtip3);
							}
							else
							{
								RemoveDestructibleObject(overtip3);
							}
						}
					}
				}
			}
			if (!m_ClearDeadOvertips)
			{
				return;
			}
			m_ClearDeadOvertips = false;
			List<BaseOvertipMapObjectVM> list = TempList.Get<BaseOvertipMapObjectVM>();
			using (ProfileScope.New("ClearDeadOvertips"))
			{
				foreach (var (baseOvertipMapObjectVM2, _) in m_ActiveOvertips)
				{
					if (baseOvertipMapObjectVM2.IsDisposed)
					{
						list.Add(baseOvertipMapObjectVM2);
					}
				}
				foreach (BaseOvertipMapObjectVM item in list)
				{
					if (item is OvertipTransitionVM vm)
					{
						RemoveTransition(vm);
					}
					else if (item is OvertipMapObjectVM vm2)
					{
						RemoveMapObject(vm2);
					}
					else if (item is OvertipDestructibleObjectVM vm3)
					{
						RemoveDestructibleObject(vm3);
					}
				}
			}
		}
	}

	private T GetWidget<T>(Queue<MonoBehaviour> queue, T prefab) where T : MonoBehaviour
	{
		if (queue.Count == 0)
		{
			T widget = WidgetFactory.GetWidget(prefab);
			widget.transform.SetParent(m_TargetContainer, worldPositionStays: false);
			return widget;
		}
		T obj = (T)queue.Dequeue();
		obj.transform.SetAsLastSibling();
		return obj;
	}

	private void AddTransition(OvertipTransitionVM transitionVM)
	{
		TOvertipTransitionView widget = GetWidget(m_FreeTransitionOvertips, m_OvertipTransitionView);
		widget.Bind(transitionVM);
		m_ActiveOvertips.Add(transitionVM, widget);
	}

	private void RemoveTransition(OvertipTransitionVM vm)
	{
		MonoBehaviour monoBehaviour = m_ActiveOvertips.Get(vm);
		if (!(monoBehaviour == null))
		{
			m_ActiveOvertips.Remove(vm);
			FreeOvertip(monoBehaviour);
		}
	}

	private void AddMapObject(OvertipMapObjectVM mapObjectVM)
	{
		Queue<MonoBehaviour> queue = ((!mapObjectVM.HasInteractionsWithOvertip) ? m_FreeMapObjectSimpleOvertips : (mapObjectVM.IsTwitchDrops ? m_FreeMapObjectTwitchDropsOvertips : m_FreeMapObjectInteractionOvertips));
		BaseOvertipMapObjectView prefab = ((!mapObjectVM.HasInteractionsWithOvertip) ? m_OvertipMapObjectSimpleView : (mapObjectVM.IsTwitchDrops ? ((BaseOvertipMapObjectView)m_OvertipMapObjectTwitchDropsView) : ((BaseOvertipMapObjectView)m_OvertipMapObjectInteractionView)));
		BaseOvertipMapObjectView widget = GetWidget(queue, prefab);
		widget.Bind(mapObjectVM);
		m_ActiveOvertips.Add(mapObjectVM, widget);
	}

	private void RemoveMapObject(OvertipMapObjectVM vm)
	{
		MonoBehaviour monoBehaviour = m_ActiveOvertips.Get(vm);
		if (!(monoBehaviour == null))
		{
			m_ActiveOvertips.Remove(vm);
			FreeOvertip(monoBehaviour);
		}
	}

	private void AddDestructibleObject(OvertipDestructibleObjectVM destructibleVM)
	{
		TOvertipDestructibleObjectView widget = GetWidget(m_FreeDestructibleObjectOvertips, m_OvertipDestructibleObjectView);
		widget.Bind(destructibleVM);
		m_ActiveOvertips.Add(destructibleVM, widget);
	}

	private void RemoveDestructibleObject(OvertipDestructibleObjectVM vm)
	{
		MonoBehaviour monoBehaviour = m_ActiveOvertips.Get(vm);
		if (!(monoBehaviour == null))
		{
			m_ActiveOvertips.Remove(vm);
			FreeOvertip(monoBehaviour);
		}
	}

	private void FreeOvertip(MonoBehaviour view)
	{
		if (!(view is TOvertipTransitionView val))
		{
			if (!(view is TOvertipMapObjectSimpleView val2))
			{
				if (!(view is TOvertipMapObjectInteractionView val3))
				{
					if (!(view is TOvertipDestructibleObjectView val4))
					{
						if (view is TOvertipMapObjectTwitchDropsView val5)
						{
							val5.Unbind();
							m_FreeMapObjectTwitchDropsOvertips.Enqueue(view);
						}
					}
					else
					{
						val4.Unbind();
						m_FreeDestructibleObjectOvertips.Enqueue(view);
					}
				}
				else
				{
					val3.Unbind();
					m_FreeMapObjectInteractionOvertips.Enqueue(view);
				}
			}
			else
			{
				val2.Unbind();
				m_FreeMapObjectSimpleOvertips.Enqueue(view);
			}
		}
		else
		{
			val.Unbind();
			m_FreeTransitionOvertips.Enqueue(view);
		}
	}
}
