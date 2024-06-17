using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;

public class BookEventChooseCharacterVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public List<BookEventCharacterVM> BookEventCharacters;

	public List<BookEventSkillsBlockVM> BookEventSkillsBlocks;

	public readonly ReactiveProperty<int?> SelectedIndex = new ReactiveProperty<int?>();

	private readonly BlueprintAnswer m_Answer;

	private BaseUnitEntity m_Unit;

	public BookEventChooseCharacterVM(BlueprintAnswer answer)
	{
		m_Answer = answer;
		List<BaseUnitEntity> list = Game.Instance.Player.Party.Where((BaseUnitEntity character) => !character.LifeState.IsFinallyDead).ToList();
		List<StatType> list2 = m_Answer.CharacterSelection.ComparisonStats.ToList();
		BookEventCharacters = new List<BookEventCharacterVM>();
		foreach (BaseUnitEntity item in list)
		{
			BookEventCharacters.Add(new BookEventCharacterVM(item, OnChoose));
		}
		BookEventSkillsBlocks = new List<BookEventSkillsBlockVM>();
		foreach (StatType item2 in list2)
		{
			BookEventSkillsBlocks.Add(new BookEventSkillsBlockVM(list, item2));
		}
	}

	public void ConfirmUnit()
	{
		Game.Instance.DialogController.SelectAnswer(m_Answer, m_Unit);
	}

	private void OnChoose(BaseUnitEntity unit)
	{
		m_Unit = unit;
		SelectedIndex.Value = ((unit == null) ? null : new int?(BookEventCharacters.FindIndex((BookEventCharacterVM ch) => ch.Unit == unit)));
	}

	protected override void DisposeImplementation()
	{
	}
}
