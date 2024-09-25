using System;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;

namespace Kingmaker.PubSubSystem;

public interface IBugReportDescriptionUIHandler : ISubscriber
{
	void HandleFullScreenUIItJustWorks(bool active, FullScreenUIType fullScreenUIType);

	void HandleException(Exception exception);

	void HandleErrorMessages(string[] errorMessages);
}
