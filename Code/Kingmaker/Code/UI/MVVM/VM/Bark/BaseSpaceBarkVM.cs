using System;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Bark;

public class BaseSpaceBarkVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<string> Text = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<Sprite> UnitPortrait = new ReactiveProperty<Sprite>();

	public readonly ReactiveCommand HideCommand = new ReactiveCommand();

	protected BaseSpaceBarkVM(BaseUnitEntity baseUnitEntity, string text)
	{
		Text.Value = text;
		UnitPortrait.Value = baseUnitEntity.Portrait.SmallPortrait;
	}

	public void HideBark()
	{
		HideCommand.Execute();
	}

	protected override void DisposeImplementation()
	{
	}
}
