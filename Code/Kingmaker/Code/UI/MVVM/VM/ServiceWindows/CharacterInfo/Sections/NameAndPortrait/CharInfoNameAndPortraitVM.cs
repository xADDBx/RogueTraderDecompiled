using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait.HitPoints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;

public class CharInfoNameAndPortraitVM : CharInfoComponentWithLevelUpVM
{
	public readonly ReactiveProperty<string> UnitName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<int> GroupCount = new ReactiveProperty<int>();

	public Sprite UnitPortraitSmall => PreviewUnit.Value?.UISettings.Portrait?.SmallPortrait;

	public Sprite UnitPortraitHalf => PreviewUnit.Value?.UISettings.Portrait?.HalfLengthPortrait ?? UnitPortraitSmall;

	public Sprite UnitPortraitFull => PreviewUnit.Value?.UISettings.Portrait.FullLengthPortrait ?? UnitPortraitHalf;

	public CharInfoHitPointsVM HitPoints { get; }

	public CharInfoNameAndPortraitVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReadOnlyReactiveProperty<LevelUpManager> levelUpManager = null)
		: base(unit, levelUpManager)
	{
		AddDisposable(HitPoints = new CharInfoHitPointsVM(unit));
	}

	protected override void DisposeImplementation()
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdateData();
	}

	private void UpdateData()
	{
		UnitName.Value = PreviewUnit.Value.CharacterName;
		GroupCount.Value = Game.Instance.SelectionCharacter.ActualGroup.Count;
	}

	public void SelectNextCharacter()
	{
		SelectCharacter(1);
	}

	public void SelectPrevCharacter()
	{
		SelectCharacter(-1);
	}

	private void SelectCharacter(int k)
	{
		List<BaseUnitEntity> actualGroup = Game.Instance.SelectionCharacter.ActualGroup;
		int num = (actualGroup.IndexOf(Unit.Value) + k) % actualGroup.Count;
		if (num < 0)
		{
			num += actualGroup.Count;
		}
		Game.Instance.SelectionCharacter.SetSelected(actualGroup[num]);
		if (actualGroup.Count == 1)
		{
			UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play();
		}
	}

	public override void HandleUISelectionChanged()
	{
		base.HandleUISelectionChanged();
		UpdateData();
	}
}
