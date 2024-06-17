using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IBugReportUIHandler : ISubscriber
{
	void HandleBugReportOpen(bool showBugReportOnly);

	void HandleBugReportCanvasHotKeyOpen();

	void HandleBugReportShow();

	void HandleBugReportHide();

	void HandleUIElementFeature(string featureName);
}
