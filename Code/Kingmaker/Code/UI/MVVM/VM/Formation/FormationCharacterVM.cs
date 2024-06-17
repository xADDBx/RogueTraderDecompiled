using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Formations;
using Kingmaker.GameCommands;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Formation;

public class FormationCharacterVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly Sprite PortraitSprite;

	public readonly ReactiveCommand FormationUpdated;

	public readonly ReactiveProperty<bool> IsInteractable = new ReactiveProperty<bool>();

	private readonly int m_Index;

	private readonly BaseUnitEntity m_Unit;

	public readonly Vector2 OffsetPosition = new Vector2(0f, 138f);

	public FormationCharacterVM(int index, BaseUnitEntity unit, ReactiveCommand formationUpdated)
	{
		m_Index = index;
		m_Unit = unit;
		FormationUpdated = formationUpdated;
		PortraitSprite = unit.Portrait.SmallPortrait;
		SetupCharacter();
		AddDisposable(formationUpdated.Subscribe(delegate
		{
			SetupCharacter();
		}));
	}

	private void SetupCharacter()
	{
		PartyFormationManager formationManager = Game.Instance.Player.FormationManager;
		IsInteractable.Value = formationManager.IsCustomFormation;
	}

	public Vector2 GetOffset()
	{
		return Game.Instance.Player.FormationManager.CurrentFormation.GetOffset(m_Index, m_Unit);
	}

	public Vector3 GetLocalPosition()
	{
		return GetOffset() * 40f + OffsetPosition;
	}

	protected override void DisposeImplementation()
	{
	}

	public void MoveCharacter(Vector2 vector)
	{
		Game.Instance.GameCommandQueue.PartyFormationOffset(m_Index, m_Unit, vector);
	}
}
