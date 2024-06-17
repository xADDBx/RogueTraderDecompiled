using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.MVVM.VM.Exploration;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Console;

public class ExplorationScanHintWrapperConsoleView : ExplorationComponentWrapperBaseView<ExplorationScanButtonWrapperVM>, IExplorationUIHandler, ISubscriber, IExplorationScanUIHandler
{
	[SerializeField]
	private ConsoleHint m_ScanHint;

	private readonly BoolReactiveProperty m_ScanNotClicked = new BoolReactiveProperty(initialValue: true);

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EventBus.Subscribe(this));
	}

	public override IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return null;
	}

	public void CreateInput(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> isEnabled)
	{
		AddDisposable(m_ScanHint.Bind(inputLayer.AddButton(delegate
		{
			OnScanClick();
		}, 8, isEnabled.And(m_ScanNotClicked).ToReactiveProperty())));
		m_ScanHint.SetLabel(UIStrings.Instance.ExplorationTexts.ExploBeginScan);
	}

	private void OnScanClick()
	{
		base.ViewModel.Interact();
		m_ScanNotClicked.Value = false;
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
		m_ScanNotClicked.Value = true;
	}

	public void CloseExplorationScreen()
	{
	}

	public void HandleScanCancelled()
	{
		m_ScanNotClicked.Value = true;
	}
}
