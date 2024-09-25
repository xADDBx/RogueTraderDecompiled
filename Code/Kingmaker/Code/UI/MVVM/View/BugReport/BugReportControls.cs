using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Rewired;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.BugReport;

public static class BugReportControls
{
	private static bool s_LeftStickBtnPressed;

	private static bool s_RightStickBtnPressed;

	public static CompositeDisposable AddBugReportControls()
	{
		s_LeftStickBtnPressed = false;
		s_RightStickBtnPressed = false;
		InputLayer inputLayer = new InputLayer
		{
			ContextName = "BugReportBaseLayer"
		};
		return new CompositeDisposable
		{
			inputLayer.AddButton(delegate
			{
				s_LeftStickBtnPressed = true;
				TryToOpenBugReport();
			}, 18),
			inputLayer.AddButton(delegate
			{
				s_LeftStickBtnPressed = false;
			}, 18, InputActionEventType.ButtonJustReleased),
			inputLayer.AddButton(delegate
			{
				s_LeftStickBtnPressed = false;
			}, 18, InputActionEventType.ButtonLongPressJustReleased),
			inputLayer.AddButton(delegate
			{
				s_RightStickBtnPressed = true;
				TryToOpenBugReport();
			}, 19),
			inputLayer.AddButton(delegate
			{
				s_RightStickBtnPressed = false;
			}, 19, InputActionEventType.ButtonJustReleased),
			inputLayer.AddButton(delegate
			{
				s_RightStickBtnPressed = false;
			}, 19, InputActionEventType.ButtonLongPressJustReleased),
			GamePad.Instance.SetBugReportLayer(inputLayer)
		};
	}

	private static void TryToOpenBugReport()
	{
		if (s_LeftStickBtnPressed && s_RightStickBtnPressed)
		{
			s_LeftStickBtnPressed = false;
			s_RightStickBtnPressed = false;
			EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
			{
				h.HandleBugReportCanvasHotKeyOpen();
			});
		}
	}
}
