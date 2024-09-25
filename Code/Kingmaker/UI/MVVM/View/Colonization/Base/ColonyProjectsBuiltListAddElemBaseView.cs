using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyProjectsBuiltListAddElemBaseView : ViewBase<ColonyProjectsBuiltListAddElemVM>, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IExplorationComponentEntity
{
	[SerializeField]
	protected OwlcatButton m_Button;

	protected override void BindViewImplementation()
	{
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public bool CanInteract()
	{
		return true;
	}

	public bool CanShowTooltip()
	{
		return false;
	}

	public void Interact()
	{
		base.ViewModel.OpenColonyProjects();
	}

	public void ShowTooltip()
	{
	}
}
