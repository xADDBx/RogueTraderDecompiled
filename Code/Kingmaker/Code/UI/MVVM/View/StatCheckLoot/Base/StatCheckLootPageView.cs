using Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.View.StatCheckLoot.Base;

public abstract class StatCheckLootPageView<TViewModel> : ViewBase<TViewModel> where TViewModel : StatCheckLootPageVM
{
	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected InputLayer m_InputLayer;

	protected bool m_IsVisible;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		InitializeImpl();
	}

	public void SetVisibility(bool visible)
	{
		if (visible != m_IsVisible)
		{
			m_IsVisible = visible;
			base.gameObject.SetActive(visible);
			if (visible)
			{
				GamePad.Instance.PushLayer(m_InputLayer);
			}
			else
			{
				GamePad.Instance.PopLayer(m_InputLayer);
			}
		}
	}

	protected override void BindViewImplementation()
	{
		BuildNavigation();
	}

	protected override void DestroyViewImplementation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
	}

	protected virtual void InitializeImpl()
	{
	}

	private void BuildNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		BuildNavigationImpl();
	}

	protected virtual void BuildNavigationImpl()
	{
	}
}
