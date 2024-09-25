using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.PointMarkers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.PathRenderer;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Code.UI.MVVM.VM.InGameCombat;

public class LineOfSightVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IAbilityTargetSelectionUIHandler, ISubscriber, IVirtualPositionUIHandler, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, IUnitDirectHoverUIHandler, ICellAbilityHandler
{
	public readonly ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<Vector3> StartPos = new ReactiveProperty<Vector3>(Vector3.zero);

	public Vector3 StartObjectOffset = Vector3.zero;

	public readonly ReactiveProperty<Vector3> EndPos = new ReactiveProperty<Vector3>(Vector3.zero);

	public Vector3 EndObjectOffset = Vector3.zero;

	public readonly ReactiveProperty<Vector3?> BestStartPos = new ReactiveProperty<Vector3?>(null);

	public readonly ReactiveProperty<float> HitChance = new ReactiveProperty<float>(0f);

	public readonly MechanicEntity Owner;

	private readonly UnitState m_UnitState;

	private readonly MechanicEntity m_CurrentUnit;

	private AbilityData m_CurrentAbility;

	private LineOfSightVM(Vector3 start, Vector3 endPosition, MechanicEntity currentUnit, MechanicEntity owner)
	{
		StartPos.Value = start;
		StartObjectOffset = UnitPathManager.Instance?.GetCellOffsetForUnit(currentUnit) ?? Vector3.zero;
		EndPos.Value = endPosition;
		EndObjectOffset = UnitPathManager.Instance?.GetCellOffsetForUnit(owner) ?? Vector3.zero;
		Owner = owner;
		m_CurrentUnit = currentUnit;
		m_CurrentAbility = Game.Instance.SelectedAbilityHandler?.Ability;
		m_UnitState = UnitStatesHolderVM.Instance.GetOrCreateUnitState(owner);
		AddDisposable(EventBus.Subscribe(this));
		EventBus.RaiseEvent(delegate(ILineOfSightHandler h)
		{
			h.OnLineOfSightCreated(this);
		});
	}

	public LineOfSightVM(MechanicEntity start, MechanicEntity end)
		: this(Game.Instance.VirtualPositionController?.GetDesiredPosition(start) ?? start.Position, end.Position, start, end)
	{
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			UpdatePosition();
		}));
		UpdatePosition();
		UpdateHitChance();
	}

	protected override void DisposeImplementation()
	{
		EventBus.RaiseEvent(delegate(ILineOfSightHandler h)
		{
			h.OnLineOfSightDestroyed(this);
		});
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		m_CurrentAbility = ability;
		UpdateHitChance();
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_CurrentAbility = null;
		UpdateHitChance();
	}

	public void HandleVirtualPositionChanged(Vector3? position)
	{
		UpdatePosition();
		UpdateHitChance();
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		UpdateHitChance();
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover)
	{
		UpdateVisibility();
	}

	public void HandleCellAbility(List<AbilityTargetUIData> abilityTargets)
	{
		UpdateVisibility();
	}

	private void UpdatePosition()
	{
		if (Owner != null)
		{
			PartLifeState partLifeState = m_UnitState?.Unit.LifeState;
			if ((partLifeState == null || partLifeState.State != UnitLifeState.Dead) && !Owner.IsDisposed)
			{
				using (ProfileScope.New("LineOfSight UpdatePosition"))
				{
					if (!LoadingProcess.Instance.IsLoadingInProcess)
					{
						StartPos.Value = Game.Instance.VirtualPositionController?.GetDesiredPosition(m_CurrentUnit) ?? m_CurrentUnit.Position;
						StartObjectOffset = UnitPathManager.Instance.Or(null)?.GetCellOffsetForUnit(m_CurrentUnit) ?? Vector3.zero;
						EndPos.Value = Owner.Position;
						EndObjectOffset = UnitPathManager.Instance.Or(null)?.GetCellOffsetForUnit(Owner) ?? Vector3.zero;
						BestStartPos.Value = UnitPredictionManager.RealHologramPosition;
					}
					return;
				}
			}
		}
		Dispose();
	}

	private void UpdateVisibility()
	{
		ItemEntityWeapon itemEntityWeapon = TryGetCurrentWeapon();
		AbilityData obj = m_CurrentAbility ?? itemEntityWeapon?.Abilities.FirstOrDefault()?.Data;
		bool flag = itemEntityWeapon?.Blueprint.IsMelee ?? false;
		bool flag2 = obj?.IsCharge ?? false;
		bool flag3 = m_CurrentAbility != null || m_UnitState.IsMouseOverUnit.Value || m_UnitState.IsAoETarget.Value || (Game.Instance.VirtualPositionController?.HasVirtualPosition ?? false);
		IsVisible.Value = !flag && !flag2 && flag3 && HitChance.Value > 0f;
	}

	private void UpdateHitChance()
	{
		float value = 0f;
		ItemEntityWeapon itemEntityWeapon = TryGetCurrentWeapon();
		AbilityData abilityData = ((!(m_CurrentAbility == null)) ? m_CurrentAbility : itemEntityWeapon?.Abilities.FirstOrDefault()?.Data);
		if (abilityData != null)
		{
			CustomGridNodeBase gridNode = AoEPatternHelper.GetGridNode(StartPos.Value);
			if (abilityData.CanTargetFromNode(gridNode, null, Owner, out var _, out var _))
			{
				CustomGridNodeBase bestShootingPositionForDesiredPosition = abilityData.GetBestShootingPositionForDesiredPosition(EndPos.Value);
				AbilityTargetUIData abilityTargetUIData;
				if (abilityData.IsScatter && !abilityData.IsMelee)
				{
					CustomGridNodeBase gridNode2 = AoEPatternHelper.GetGridNode(EndPos.Value);
					OrientedPatternData orientedPattern = abilityData.GetPatternSettings().GetOrientedPattern(abilityData, bestShootingPositionForDesiredPosition, gridNode2);
					List<AbilityTargetUIData> value2;
					using (CollectionPool<List<AbilityTargetUIData>, AbilityTargetUIData>.Get(out value2))
					{
						abilityData.GatherAffectedTargetsData(orientedPattern, bestShootingPositionForDesiredPosition.Vector3Position, Owner, in value2, Owner);
						abilityTargetUIData = value2.FirstOrDefault((AbilityTargetUIData t) => t.Target == Owner);
					}
				}
				else
				{
					abilityTargetUIData = AbilityTargetUIDataCache.Instance.GetOrCreate(abilityData, Owner, bestShootingPositionForDesiredPosition.Vector3Position);
				}
				value = abilityTargetUIData.HitWithAvoidanceChance;
			}
		}
		else
		{
			value = LosCalculations.GetWarhammerLos(StartPos.Value, m_CurrentUnit.SizeRect, Owner).CoverType switch
			{
				LosCalculations.CoverType.None => 80f, 
				LosCalculations.CoverType.Half => 50f, 
				LosCalculations.CoverType.Full => 10f, 
				LosCalculations.CoverType.Invisible => 0f, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		HitChance.Value = value;
		UpdateVisibility();
	}

	private ItemEntityWeapon TryGetCurrentWeapon()
	{
		HandsEquipmentSet obj = (m_CurrentUnit as UnitEntity)?.Body.CurrentHandsEquipmentSet;
		ItemEntityWeapon itemEntityWeapon = obj?.PrimaryHand.MaybeWeapon;
		ItemEntityWeapon itemEntityWeapon2 = obj?.SecondaryHand.MaybeWeapon;
		if (CheckWeapon(itemEntityWeapon))
		{
			return itemEntityWeapon;
		}
		if (CheckWeapon(itemEntityWeapon2))
		{
			return itemEntityWeapon2;
		}
		return itemEntityWeapon;
		static bool CheckWeapon(ItemEntityWeapon weapon)
		{
			return weapon?.Blueprint.IsRanged ?? false;
		}
	}
}
