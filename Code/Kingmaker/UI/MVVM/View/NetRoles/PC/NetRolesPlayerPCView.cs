using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.NetRoles.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetRoles.PC;

public class NetRolesPlayerPCView : NetRolesPlayerBaseView
{
	[SerializeField]
	private List<NetRolesPlayerCharacterPCView> m_Characters;

	public override void Initialize()
	{
		base.Initialize();
		m_Characters.ForEach(delegate(NetRolesPlayerCharacterPCView c)
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
}
