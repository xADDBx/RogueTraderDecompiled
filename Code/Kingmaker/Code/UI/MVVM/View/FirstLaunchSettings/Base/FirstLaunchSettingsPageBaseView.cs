using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base;

public abstract class FirstLaunchSettingsPageBaseView<TViewModel> : ViewBase<TViewModel> where TViewModel : FirstLaunchSettingsPageVM
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected IConsoleEntity[] AdditionalEntities;

	public void Initialize()
	{
		InitializeImpl();
	}

	public void SetNavigationBehaviour(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		m_NavigationBehaviour = navigationBehaviour;
		SetNavigationBehaviourImpl(navigationBehaviour);
	}

	public void AddNavigationEntities(IConsoleEntity[] entities)
	{
		AdditionalEntities = entities;
	}

	protected virtual void InitializeImpl()
	{
	}

	protected virtual void SetNavigationBehaviourImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		BuildNavigation();
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		m_NavigationBehaviour.Clear();
	}

	public void ClearNavigationBehaviour()
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
		m_NavigationBehaviour.Clear();
	}

	private void BuildNavigation()
	{
		BuildNavigationImpl(m_NavigationBehaviour);
		BuildAdditionalNavigation(m_NavigationBehaviour);
	}

	protected virtual void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
	}

	protected virtual void BuildAdditionalNavigation(GridConsoleNavigationBehaviour navigationBehaviour)
	{
	}
}
