using System;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;

public class BookEventCharacterVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public Sprite Portrait;

	public BaseUnitEntity Unit;

	private readonly Action<BaseUnitEntity> m_OnChooseUnit;

	public BookEventCharacterVM(BaseUnitEntity unit, Action<BaseUnitEntity> onChooseUnit)
	{
		Unit = unit;
		Portrait = Unit?.Portrait.SmallPortrait;
		m_OnChooseUnit = onChooseUnit;
	}

	public void OnChooseUnit(bool value)
	{
		m_OnChooseUnit(value ? Unit : null);
	}

	protected override void DisposeImplementation()
	{
	}
}
