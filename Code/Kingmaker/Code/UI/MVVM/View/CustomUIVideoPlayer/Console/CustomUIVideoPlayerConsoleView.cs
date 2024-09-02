using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.CustomUIVideoPlayer.Base;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.CustomUIVideoPlayer.Console;

public class CustomUIVideoPlayerConsoleView : CustomUIVideoPlayerBaseView
{
	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, ConsoleHint playPauseVideoHint, BoolReactiveProperty isEnabled)
	{
		AddDisposable(playPauseVideoHint.Bind(inputLayer.AddButton(delegate
		{
			if (!VideoIsStarted)
			{
				StartVideo();
				ShowHideInterface(state: false);
			}
			else
			{
				PlayPauseVideo();
				ShowHideInterface(!VideoIsPlaying.Value);
			}
		}, 17, isEnabled.And(base.ViewModel.HasVideo).ToReactiveProperty())));
		playPauseVideoHint.SetLabel(UIStrings.Instance.DlcManager.PlayVideo);
		AddDisposable(VideoIsPlaying.Subscribe(delegate(bool value)
		{
			playPauseVideoHint.SetLabel(value ? UIStrings.Instance.DlcManager.StopVideo : UIStrings.Instance.DlcManager.PlayVideo);
		}));
	}
}
