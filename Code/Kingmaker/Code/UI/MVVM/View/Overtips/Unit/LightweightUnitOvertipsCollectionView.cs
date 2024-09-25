using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit;

public class LightweightUnitOvertipsCollectionView : ViewBase<LightweightUnitOvertipsCollectionVM>
{
	[SerializeField]
	private RectTransform m_NpcContainer;

	[SerializeField]
	private LightweightUnitOvertipView m_OvertipLightweightUnitView;

	private readonly Queue<MonoBehaviour> m_FreeNPCOvertips = new Queue<MonoBehaviour>();

	private readonly Dictionary<LightweightUnitOvertipVM, MonoBehaviour> m_ActiveOvertips = new Dictionary<LightweightUnitOvertipVM, MonoBehaviour>();

	private bool m_ClearDeadOvertips;

	private void PrewarmOvertips<T>(Queue<MonoBehaviour> queue, T prefab, int count, Transform targetContainer) where T : MonoBehaviour
	{
		WidgetFactory.InstantiateWidget(prefab, count, targetContainer);
	}

	public void Initialize()
	{
		PrewarmOvertips(m_FreeNPCOvertips, m_OvertipLightweightUnitView, 10, m_NpcContainer);
	}

	private bool ExtraUnitShouldHaveOvertip(BaseUnitEntity unit)
	{
		UnitPartInteractions optional = unit.GetOptional<UnitPartInteractions>();
		if (optional == null || !optional.HasDialogInteractions)
		{
			return unit.Blueprint.GetComponent<AddLocalMapMarker>() != null;
		}
		return true;
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
				foreach (LightweightUnitOvertipVM overtip in base.ViewModel.Overtips)
				{
					MechanicEntity unit = overtip.Unit;
					bool flag = unit != null && (!unit.Features.IsUntargetable || overtip.IsBarkActive.Value) && (!(unit is BaseUnitEntity { IsExtra: not false } baseUnitEntity) || ExtraUnitShouldHaveOvertip(baseUnitEntity) || overtip.IsBarkActive.Value || baseUnitEntity.View.IsHighlighted || baseUnitEntity.View.MouseHighlighted || overtip.HasSurrounding.Value) && unit.IsVisibleForPlayer && (overtip.ForceOnScreen || (!overtip.HideFromScreen && overtip.IsInCameraFrustum));
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
			List<LightweightUnitOvertipVM> list = TempList.Get<LightweightUnitOvertipVM>();
			using (ProfileScope.New("ClearDeadOvertips"))
			{
				foreach (var (lightweightUnitOvertipVM2, _) in m_ActiveOvertips)
				{
					if (lightweightUnitOvertipVM2.IsDisposed)
					{
						list.Add(lightweightUnitOvertipVM2);
					}
				}
				foreach (LightweightUnitOvertipVM item in list)
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

	private void AddOvertip(LightweightUnitOvertipVM vm)
	{
		if (!(m_OvertipLightweightUnitView == null))
		{
			Transform npcContainer = m_NpcContainer;
			LightweightUnitOvertipView widget = GetWidget(m_FreeNPCOvertips, m_OvertipLightweightUnitView, npcContainer);
			widget.Bind(vm);
			m_ActiveOvertips.Add(vm, widget);
		}
	}

	private void RemoveOvertip(LightweightUnitOvertipVM vm)
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
		Queue<MonoBehaviour> freeNPCOvertips = m_FreeNPCOvertips;
		FreeSurfaceOvertip(view);
		freeNPCOvertips.Enqueue(view);
	}

	private void FreeSurfaceOvertip(MonoBehaviour view)
	{
		if (view is OvertipUnitView overtipUnitView)
		{
			overtipUnitView.Unbind();
		}
	}
}
