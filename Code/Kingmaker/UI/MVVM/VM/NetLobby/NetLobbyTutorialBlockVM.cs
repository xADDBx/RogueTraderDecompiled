using System;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.NetLobby;

public class NetLobbyTutorialBlockVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly Sprite BlockSprite;

	public readonly string BlockDescription;

	public NetLobbyTutorialBlockVM(Sprite blockSprite, string blockDescription)
	{
		BlockSprite = blockSprite;
		BlockDescription = blockDescription;
	}

	protected override void DisposeImplementation()
	{
	}
}
