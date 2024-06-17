using System;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.SaveLoad;

public class SaveLoadPortraitVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly Sprite Portrait;

	public readonly string Rank;

	public SaveLoadPortraitVM(Sprite portrait, string rank)
	{
		Portrait = portrait;
		Rank = rank;
	}

	protected override void DisposeImplementation()
	{
	}
}
