using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Pointer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.Surface.InputLayers;

public class SurfaceCombatInputLayer : SurfaceMainInputLayer
{
	private readonly List<BaseUnitEntity> m_EnemyList = new List<BaseUnitEntity>();

	private int m_EnemyIndex = -1;

	private IDisposable m_CurrentUnitSubscription;

	public new static SurfaceCombatInputLayer Instance { get; private set; }

	public SurfaceCombatInputLayer()
	{
		if (GetType() == typeof(SurfaceCombatInputLayer))
		{
			Instance = this;
		}
		base.CursorEnabled = true;
		m_CurrentUnitSubscription = Game.Instance.SelectionCharacter.SelectedUnit.Subscribe(delegate
		{
			m_EnemyIndex = -1;
		});
	}

	protected override void UpdateLeftStickMovement()
	{
		CameraRig.Instance.ScrollBy2D(m_LeftStickVector);
		if (!CameraRig.Instance.OnLevelBound)
		{
			ConsoleCursor consoleCursor = ConsoleCursor.Instance.Or(null);
			if ((object)consoleCursor == null || consoleCursor.OnScreenCenter)
			{
				ConsoleCursor.Instance.Or(null)?.SetToCenter();
				goto IL_0069;
			}
		}
		ConsoleCursor.Instance.Or(null)?.MoveCursor(m_LeftStickVector);
		goto IL_0069;
		IL_0069:
		ConsoleCursor.Instance.Or(null)?.SnapToCurrentNode();
	}

	public override void OnPrevInteractable()
	{
		if (!Game.Instance.TurnController.IsPreparationTurn)
		{
			MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
			if (currentUnit != null && currentUnit.IsDirectlyControllable)
			{
				FocusOnEnemy(dir: false);
			}
		}
	}

	public override void OnNextInteractable()
	{
		if (!Game.Instance.TurnController.IsPreparationTurn)
		{
			MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
			if (currentUnit != null && currentUnit.IsDirectlyControllable)
			{
				FocusOnEnemy(dir: true);
			}
		}
	}

	private void FocusOnEnemy(bool dir)
	{
		UpdateEnemyList();
		if (m_EnemyList.Empty())
		{
			return;
		}
		if (dir)
		{
			if (++m_EnemyIndex >= m_EnemyList.Count)
			{
				m_EnemyIndex = 0;
			}
		}
		else if (--m_EnemyIndex < 0)
		{
			m_EnemyIndex = m_EnemyList.Count - 1;
		}
		CameraRig.Instance.ScrollTo(m_EnemyList[m_EnemyIndex].Position);
	}

	private void UpdateEnemyList()
	{
		Game gameInstance = Game.Instance;
		m_EnemyList.RemoveAll((BaseUnitEntity unit) => !IsValidEnemy(unit));
		m_EnemyList.AddRange(gameInstance.State.AllBaseUnits.Where((BaseUnitEntity unit) => IsValidEnemy(unit) && !m_EnemyList.Contains(unit)));
		m_EnemyList.Sort(delegate(BaseUnitEntity e1, BaseUnitEntity e2)
		{
			BaseUnitEntity baseUnitEntity = gameInstance.SelectionCharacter.SelectedUnit.Value ?? gameInstance.SelectionCharacter.ActualGroup.FirstOrDefault((BaseUnitEntity u) => u == gameInstance.TurnController.CurrentUnit);
			if (baseUnitEntity == null)
			{
				return 1;
			}
			float num = e1.DistanceTo(baseUnitEntity);
			float num2 = e2.DistanceTo(baseUnitEntity);
			return (num > num2) ? 1 : (-1);
		});
	}

	private bool IsValidEnemy(BaseUnitEntity unit)
	{
		if (unit.IsInCombat && unit.Faction.IsPlayerEnemy && unit.IsVisibleForPlayer)
		{
			return !unit.LifeState.IsDead;
		}
		return false;
	}

	public override void Dispose()
	{
		base.Dispose();
		m_CurrentUnitSubscription?.Dispose();
		m_CurrentUnitSubscription = null;
		base.CursorEnabled = false;
	}
}
