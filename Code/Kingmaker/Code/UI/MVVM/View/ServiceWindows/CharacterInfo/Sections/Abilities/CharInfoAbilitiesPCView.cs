using UniRx;
using UniRx.Triggers;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoAbilitiesPCView : CharInfoAbilitiesBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_ActiveAbilities.OnPointerClickAsObservable().Subscribe(delegate
		{
			SetActiveAbilitiesState(state: true);
		}));
		AddDisposable(m_PassiveAbilities.OnPointerClickAsObservable().Subscribe(delegate
		{
			SetActiveAbilitiesState(state: false);
		}));
	}
}
