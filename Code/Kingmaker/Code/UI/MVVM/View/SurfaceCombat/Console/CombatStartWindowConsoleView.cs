using Kingmaker.Code.UI.MVVM.View.EtudeCounter.Console;
using Kingmaker.Code.UI.MVVM.View.Surface.InputLayers;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.Console;

public class CombatStartWindowConsoleView : CombatStartWindowView
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_StartBattleHint;

	[SerializeField]
	private EtudeCounterConsoleView m_EtudeCounterView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_EtudeCounterView == null)
		{
			m_EtudeCounterView = Object.FindObjectOfType<EtudeCounterConsoleView>();
		}
		if (m_EtudeCounterView != null)
		{
			m_EtudeCounterView.SetEtudeCounterVisible(value: false);
		}
		AddDisposable(m_StartBattleHint.Bind(SurfaceCombatInputLayer.Instance.AddButton(delegate
		{
			if (m_EtudeCounterView != null)
			{
				m_EtudeCounterView.SetEtudeCounterVisible(value: true);
			}
		}, 17)));
	}
}
