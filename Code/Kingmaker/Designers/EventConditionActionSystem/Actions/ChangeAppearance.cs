using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.View;
using Kingmaker.Visual.Sound;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("dc92edc6368e48c9b2222f0bf32297de")]
public class ChangeAppearance : GameAction
{
	public override string GetCaption()
	{
		return "Change Appearance";
	}

	protected override void RunAction()
	{
		UpdateSoundState(MusicStateHandler.MusicState.Chargen);
		CharGenConfig.Create(Game.Instance.Player.MainCharacter.Entity.ToBaseUnitEntity(), CharGenConfig.CharGenMode.Appearance).SetOnComplete(OnComplete).SetOnClose(OnClose)
			.SetOnCloseSoundAction(delegate
			{
				UpdateSoundState(MusicStateHandler.MusicState.Setting);
			})
			.OpenUI();
		static void OnClose()
		{
		}
		static void OnComplete(BaseUnitEntity unit)
		{
			UnitEntityView view = unit.CreateView();
			UnitEntityView view2 = unit.View;
			unit.DetachView();
			view2.DestroyViewObject();
			unit.AttachView(view);
			Game.Instance.Player.UpdateClaimedDlcRewardsByChosenAppearance(unit);
		}
		static void UpdateSoundState(MusicStateHandler.MusicState state)
		{
			SoundState.Instance.OnMusicStateChange(state);
		}
	}
}
