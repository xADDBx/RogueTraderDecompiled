namespace Kingmaker.Code.UI.MVVM.View.WarningNotification;

public interface IWarningElement
{
	void Initialize();

	void ShowSequenceCanvasGroupFadeAnimation(float showHideTime, float stayOnScreenTime, bool withSound = true);
}
