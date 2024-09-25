using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Other;

public class FloatButtonConsoleView : MonoBehaviour, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IConsoleEntityProxy
{
	[SerializeField]
	private OwlcatMultiButton m_FocusMultiButton;

	public OwlcatMultiButton Button => m_FocusMultiButton;

	public IConsoleEntity ConsoleEntityProxy => m_FocusMultiButton;

	public void SetFocus(bool value)
	{
		m_FocusMultiButton.SetFocus(value);
		m_FocusMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public bool IsValid()
	{
		if (m_FocusMultiButton.IsValid())
		{
			return m_FocusMultiButton.Interactable;
		}
		return false;
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}
}
