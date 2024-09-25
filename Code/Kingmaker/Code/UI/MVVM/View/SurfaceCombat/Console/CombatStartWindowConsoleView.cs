using Kingmaker.Code.UI.MVVM.View.Surface.InputLayers;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.Console;

public class CombatStartWindowConsoleView : CombatStartWindowView
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_StartBattleHint;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_StartBattleHint.Bind(SurfaceCombatInputLayer.Instance.AddButton(delegate
		{
		}, 17)));
	}
}
