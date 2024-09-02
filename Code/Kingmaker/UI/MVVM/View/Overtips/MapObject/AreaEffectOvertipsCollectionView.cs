using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Overtips.MapObject;
using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;
using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject.Collections;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Overtips.MapObject;

public class AreaEffectOvertipsCollectionView : ViewBase<AreaEffectOvertipsCollectionVM>
{
	[SerializeField]
	private RectTransform m_TargetContainer;

	[SerializeField]
	private OvertipAreaEffectView m_OvertipAreaEffectView;

	private readonly Queue<MonoBehaviour> m_FreeAreaEffectOvertips = new Queue<MonoBehaviour>();

	private readonly Dictionary<OvertipAreaEffectVM, MonoBehaviour> m_ActiveOvertips = new Dictionary<OvertipAreaEffectVM, MonoBehaviour>();

	private bool m_ClearDeadOvertips;

	private void PrewarmOvertips<T>(Queue<MonoBehaviour> queue, T prefab, int count, Transform targetContainer) where T : MonoBehaviour
	{
		WidgetFactory.InstantiateWidget(prefab, count, targetContainer);
	}

	public void Initialize()
	{
		PrewarmOvertips(m_FreeAreaEffectOvertips, m_OvertipAreaEffectView, 5, m_TargetContainer);
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
				foreach (OvertipAreaEffectVM overtip in base.ViewModel.Overtips)
				{
					AreaEffectEntity areaEffectEntity = overtip.AreaEffectEntity;
					bool flag = areaEffectEntity != null && areaEffectEntity.IsVisibleForPlayer && !overtip.HideFromScreen && overtip.IsInCameraFrustum;
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
			List<OvertipAreaEffectVM> list = TempList.Get<OvertipAreaEffectVM>();
			using (ProfileScope.New("ClearDeadOvertips"))
			{
				foreach (var (overtipAreaEffectVM2, _) in m_ActiveOvertips)
				{
					if (overtipAreaEffectVM2.IsDisposed)
					{
						list.Add(overtipAreaEffectVM2);
					}
				}
				foreach (OvertipAreaEffectVM item in list)
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

	private void AddOvertip(OvertipAreaEffectVM vm)
	{
		if (!(m_OvertipAreaEffectView == null))
		{
			Transform targetContainer = m_TargetContainer;
			OvertipAreaEffectView widget = GetWidget(m_FreeAreaEffectOvertips, m_OvertipAreaEffectView, targetContainer);
			widget.Bind(vm);
			m_ActiveOvertips.Add(vm, widget);
		}
	}

	private void RemoveOvertip(OvertipAreaEffectVM vm)
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
		Queue<MonoBehaviour> freeAreaEffectOvertips = m_FreeAreaEffectOvertips;
		FreeSurfaceOvertip(view);
		freeAreaEffectOvertips.Enqueue(view);
	}

	private void FreeSurfaceOvertip(MonoBehaviour view)
	{
		if (view is OvertipAreaEffectView overtipAreaEffectView)
		{
			overtipAreaEffectView.Unbind();
		}
	}
}
