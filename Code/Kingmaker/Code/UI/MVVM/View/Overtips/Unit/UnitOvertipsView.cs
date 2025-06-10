using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit.OvertipSpaceShipUnit;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.QA.Profiling;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit;

public class UnitOvertipsView : ViewBase<UnitOvertipsCollectionVM>
{
	[SerializeField]
	private RectTransform m_PartyContainer;

	[SerializeField]
	private RectTransform m_EnemyContainer;

	[SerializeField]
	private RectTransform m_NpcContainer;

	[SerializeField]
	private bool m_IsSpace;

	[FormerlySerializedAs("m_OvertipUnitPCView")]
	[SerializeField]
	private OvertipUnitView m_OvertipUnitView;

	[SerializeField]
	private OvertipSpaceShipUnitView m_OvertipSpaceShipUnitPCView;

	[SerializeField]
	private OvertipTorpedoUnitView m_OvertipTorpedoUnitView;

	private Queue<MonoBehaviour> m_FreePartyOvertips = new Queue<MonoBehaviour>();

	private Queue<MonoBehaviour> m_FreeEnemyOvertips = new Queue<MonoBehaviour>();

	private Queue<MonoBehaviour> m_FreeNPCOvertips = new Queue<MonoBehaviour>();

	private Dictionary<OvertipEntityUnitVM, MonoBehaviour> m_ActiveOvertips = new Dictionary<OvertipEntityUnitVM, MonoBehaviour>();

	private bool m_ClearDeadOvertips;

	private bool IsSpaceCombat => Game.Instance.CurrentMode == GameModeType.SpaceCombat;

	private void PrewarmOvertips<T>(Queue<MonoBehaviour> queue, T prefab, int count, Transform targetContainer) where T : MonoBehaviour
	{
		WidgetFactory.InstantiateWidget(prefab, count, targetContainer);
	}

	public void Initialize()
	{
		if (m_IsSpace)
		{
			PrewarmOvertips(m_FreePartyOvertips, m_OvertipSpaceShipUnitPCView, 1, m_PartyContainer);
			PrewarmOvertips(m_FreeEnemyOvertips, m_OvertipSpaceShipUnitPCView, 10, m_EnemyContainer);
			PrewarmOvertips(m_FreeNPCOvertips, m_OvertipSpaceShipUnitPCView, 5, m_NpcContainer);
			PrewarmOvertips(m_FreePartyOvertips, m_OvertipTorpedoUnitView, 1, m_PartyContainer);
			PrewarmOvertips(m_FreeEnemyOvertips, m_OvertipTorpedoUnitView, 5, m_EnemyContainer);
			PrewarmOvertips(m_FreeNPCOvertips, m_OvertipTorpedoUnitView, 1, m_NpcContainer);
		}
		else
		{
			PrewarmOvertips(m_FreePartyOvertips, m_OvertipUnitView, 6, m_PartyContainer);
			PrewarmOvertips(m_FreeEnemyOvertips, m_OvertipUnitView, 10, m_EnemyContainer);
			PrewarmOvertips(m_FreeNPCOvertips, m_OvertipUnitView, 10, m_NpcContainer);
		}
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
				foreach (OvertipEntityUnitVM overtip in base.ViewModel.Overtips)
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
			List<OvertipEntityUnitVM> list = TempList.Get<OvertipEntityUnitVM>();
			using (ProfileScope.New("ClearDeadOvertips"))
			{
				foreach (var (overtipEntityUnitVM2, _) in m_ActiveOvertips)
				{
					if (overtipEntityUnitVM2.IsDisposed)
					{
						list.Add(overtipEntityUnitVM2);
					}
				}
				foreach (OvertipEntityUnitVM item in list)
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

	private void AddOvertip(OvertipEntityUnitVM vm)
	{
		if (m_IsSpace)
		{
			AddSpaceOvertip(vm);
		}
		else
		{
			AddSurfaceOvertip(vm);
		}
	}

	private void AddSpaceOvertip(OvertipEntityUnitVM vm)
	{
		if (IsSpaceCombat && !(m_OvertipSpaceShipUnitPCView == null))
		{
			Transform targetContainer;
			Queue<MonoBehaviour> queue;
			if (vm.UnitUIWrapper.IsPlayerEnemy)
			{
				targetContainer = m_EnemyContainer;
				queue = m_FreeEnemyOvertips;
			}
			else if (vm.UnitUIWrapper.IsPlayer)
			{
				targetContainer = m_PartyContainer;
				queue = m_FreePartyOvertips;
			}
			else
			{
				targetContainer = m_NpcContainer;
				queue = m_FreeNPCOvertips;
			}
			BaseOvertipView<OvertipEntityUnitVM> baseOvertipView = ((!vm.UnitUIWrapper.IsSuicideAttacker) ? ((BaseOvertipView<OvertipEntityUnitVM>)GetWidget(queue, m_OvertipSpaceShipUnitPCView, targetContainer)) : ((BaseOvertipView<OvertipEntityUnitVM>)GetWidget(queue, m_OvertipTorpedoUnitView, targetContainer)));
			baseOvertipView.Bind(vm);
			m_ActiveOvertips.Add(vm, baseOvertipView);
		}
	}

	private void AddSurfaceOvertip(OvertipEntityUnitVM vm)
	{
		if (!(m_OvertipUnitView == null))
		{
			Transform targetContainer = (vm.UnitUIWrapper.IsPlayerEnemy ? m_EnemyContainer : (vm.UnitUIWrapper.IsPlayer ? m_PartyContainer : m_NpcContainer));
			OvertipUnitView overtipUnitView = (vm.UnitUIWrapper.IsPlayerEnemy ? GetWidget(m_FreeEnemyOvertips, m_OvertipUnitView, targetContainer) : (vm.UnitUIWrapper.IsPlayer ? GetWidget(m_FreePartyOvertips, m_OvertipUnitView, targetContainer) : GetWidget(m_FreeNPCOvertips, m_OvertipUnitView, targetContainer)));
			overtipUnitView.Bind(vm);
			m_ActiveOvertips.Add(vm, overtipUnitView);
		}
	}

	private void RemoveOvertip(OvertipEntityUnitVM vm)
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
		Transform parent = view.transform.parent;
		Queue<MonoBehaviour> obj = ((parent == m_EnemyContainer) ? m_FreeEnemyOvertips : ((parent == m_PartyContainer) ? m_FreePartyOvertips : m_FreeNPCOvertips));
		if (m_IsSpace)
		{
			FreeSpaceOvertip(view);
		}
		else
		{
			FreeSurfaceOvertip(view);
		}
		obj.Enqueue(view);
	}

	private void FreeSurfaceOvertip(MonoBehaviour view)
	{
		if (view is OvertipUnitView overtipUnitView)
		{
			overtipUnitView.Unbind();
		}
	}

	private void FreeSpaceOvertip(MonoBehaviour view)
	{
		if (view is OvertipTorpedoUnitView overtipTorpedoUnitView)
		{
			overtipTorpedoUnitView.Unbind();
		}
		else if (view is OvertipSpaceShipUnitView overtipSpaceShipUnitView)
		{
			overtipSpaceShipUnitView.Unbind();
		}
	}
}
