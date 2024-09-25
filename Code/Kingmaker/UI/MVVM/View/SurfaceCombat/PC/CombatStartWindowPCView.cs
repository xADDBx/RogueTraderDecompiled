using Kingmaker.Code.UI.MVVM.View.SurfaceCombat;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;

namespace Kingmaker.UI.MVVM.View.SurfaceCombat.PC;

public class CombatStartWindowPCView : CombatStartWindowView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_StartBattleButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.StartBattle));
	}
}
