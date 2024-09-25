using System;
using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.NetRoles.Base;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NetRoles.Console;

public class NetRolesPlayerConsoleView : NetRolesPlayerBaseView
{
	[SerializeField]
	private List<NetRolesPlayerCharacterConsoleView> m_Characters;

	public List<NetRolesPlayerCharacterConsoleView> Characters => m_Characters;

	public override void Initialize()
	{
		base.Initialize();
		m_Characters.ForEach(delegate(NetRolesPlayerCharacterConsoleView c)
		{
			c.Initialize();
		});
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		for (int i = 0; i < m_Characters.Count; i++)
		{
			m_Characters[i].Bind((base.ViewModel.Players.Count > i) ? base.ViewModel.Players[i] : null);
		}
	}

	public void AddGamerTagInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, Action hideGamersTagModeAction)
	{
		m_GamerTagAndName.AddGamerTagInput(inputLayer, hintsWidget, hideGamersTagModeAction);
	}
}
