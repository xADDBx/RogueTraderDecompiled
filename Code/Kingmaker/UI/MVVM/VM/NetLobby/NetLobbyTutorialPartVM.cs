using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;

namespace Kingmaker.UI.MVVM.VM.NetLobby;

public class NetLobbyTutorialPartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly AutoDisposingList<NetLobbyTutorialBlockVM> TutorialBlocksVMs = new AutoDisposingList<NetLobbyTutorialBlockVM>();

	private readonly Action m_CloseAction;

	public readonly bool WithBlocksAnimation;

	public NetLobbyTutorialPartVM(Action closeAction, bool withBlocksAnimation = false)
	{
		m_CloseAction = closeAction;
		WithBlocksAnimation = withBlocksAnimation;
		AddBlocks();
	}

	protected override void DisposeImplementation()
	{
	}

	private void AddBlocks()
	{
		TutorialBlocksVMs.Clear();
		List<NetLobbyTutorialBlockInfo> tutorialBlocksInfo = UIConfig.Instance.BlueprintUINetLobbyTutorial.TutorialBlocksInfo;
		if (tutorialBlocksInfo.Any())
		{
			tutorialBlocksInfo.ForEach(delegate(NetLobbyTutorialBlockInfo block)
			{
				TutorialBlocksVMs.Add(new NetLobbyTutorialBlockVM(block.BlockSprite, block.BlockDescription));
			});
		}
	}

	public void OnClose()
	{
		m_CloseAction();
	}
}
