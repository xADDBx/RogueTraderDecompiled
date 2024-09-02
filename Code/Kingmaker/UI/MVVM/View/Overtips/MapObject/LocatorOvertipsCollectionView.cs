using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Overtips.MapObject;
using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;
using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject.Collections;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Overtips.MapObject;

public class LocatorOvertipsCollectionView : ViewBase<LocatorOvertipsCollectionVM>
{
	[SerializeField]
	private RectTransform m_TargetContainer;

	[SerializeField]
	private OvertipLocatorView m_OvertipLocatorView;

	private readonly Queue<MonoBehaviour> m_FreeLocatorOvertips = new Queue<MonoBehaviour>();

	private readonly Dictionary<OvertipLocatorVM, MonoBehaviour> m_ActiveOvertips = new Dictionary<OvertipLocatorVM, MonoBehaviour>();

	private bool m_ClearDeadOvertips;

	private void PrewarmOvertips<T>(Queue<MonoBehaviour> queue, T prefab, int count, Transform targetContainer) where T : MonoBehaviour
	{
		WidgetFactory.InstantiateWidget(prefab, count, targetContainer);
	}

	public void Initialize()
	{
		PrewarmOvertips(m_FreeLocatorOvertips, m_OvertipLocatorView, 10, m_TargetContainer);
	}

	public void Update()
	{
		if (base.ViewModel?.Overtips == null)
		{
			return;
		}
		using (Counters.Overtips?.Measure())
		{
			using (ProfileScope.New("VM visibility"))
			{
				foreach (OvertipLocatorVM overtip in base.ViewModel.Overtips)
				{
					LocatorEntity locatorEntity = overtip.LocatorEntity;
					bool flag = locatorEntity != null && overtip.IsBarkActive.Value && locatorEntity.IsVisibleForPlayer && (overtip.ForceOnScreen || (!overtip.HideFromScreen && overtip.IsInCameraFrustum));
					bool flag2 = m_ActiveOvertips.Get(overtip) != null;
					if (flag != flag2)
					{
						if (flag)
						{
							AddOvertip(overtip);
						}
						else
						{
							RemoveOvertip(overtip);
						}
					}
				}
			}
			if (!m_ClearDeadOvertips)
			{
				return;
			}
			m_ClearDeadOvertips = false;
			List<OvertipLocatorVM> list = TempList.Get<OvertipLocatorVM>();
			using (ProfileScope.New("ClearDeadOvertips"))
			{
				foreach (var (overtipLocatorVM2, _) in m_ActiveOvertips)
				{
					if (overtipLocatorVM2.IsDisposed)
					{
						list.Add(overtipLocatorVM2);
					}
				}
				foreach (OvertipLocatorVM item in list)
				{
					RemoveOvertip(item);
				}
			}
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Overtips.ObserveRemove().Subscribe(delegate
		{
			m_ClearDeadOvertips = true;
		}));
		AddDisposable(base.ViewModel.Overtips.ObserveReset().Subscribe(delegate
		{
			m_ClearDeadOvertips = true;
		}));
	}

	protected override void DestroyViewImplementation()
	{
		foreach (MonoBehaviour value in m_ActiveOvertips.Values)
		{
			FreeOvertip(value);
		}
		m_ActiveOvertips.Clear();
	}

	private T GetWidget<T>(Queue<MonoBehaviour> queue, T prefab, Transform targetContainer) where T : MonoBehaviour
	{
		if (queue.Count == 0)
		{
			T widget = WidgetFactory.GetWidget(prefab);
			widget.transform.SetParent(targetContainer, worldPositionStays: false);
			return widget;
		}
		T obj = (T)queue.Dequeue();
		obj.transform.SetAsLastSibling();
		return obj;
	}

	private void AddOvertip(OvertipLocatorVM vm)
	{
		if (!(m_OvertipLocatorView == null))
		{
			Transform targetContainer = m_TargetContainer;
			OvertipLocatorView widget = GetWidget(m_FreeLocatorOvertips, m_OvertipLocatorView, targetContainer);
			widget.Bind(vm);
			m_ActiveOvertips.Add(vm, widget);
		}
	}

	private void RemoveOvertip(OvertipLocatorVM vm)
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
		Queue<MonoBehaviour> freeLocatorOvertips = m_FreeLocatorOvertips;
		FreeSurfaceOvertip(view);
		freeLocatorOvertips.Enqueue(view);
	}

	private void FreeSurfaceOvertip(MonoBehaviour view)
	{
		if (view is OvertipLocatorView overtipLocatorView)
		{
			overtipLocatorView.Unbind();
		}
	}
}
