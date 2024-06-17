using System;
using Kingmaker.Blueprints;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.CharGen.Portrait;

public class CharGenPortraitVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly PortraitData PortraitData;

	public Sprite PortraitSmall => PortraitData?.SmallPortrait;

	public Sprite PortraitHalf => PortraitData?.HalfLengthPortrait;

	public Sprite PortraitFull => PortraitData?.FullLengthPortrait;

	public CharGenPortraitVM(PortraitData portraitData)
	{
		PortraitData = portraitData;
		portraitData?.EnsureImages();
		UILog.VMCreated("CharGenPortraitVM");
	}

	protected override void DisposeImplementation()
	{
		UILog.VMDisposed("CharGenPortraitVM");
	}
}
