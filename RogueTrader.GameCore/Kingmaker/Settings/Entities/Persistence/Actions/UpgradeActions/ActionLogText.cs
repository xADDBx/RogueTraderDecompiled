using Kingmaker.Settings.Interfaces;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Settings.Entities.Persistence.Actions.UpgradeActions;

public class ActionLogText : ISettingsUpgradeAction
{
	private readonly string m_Text;

	public ActionLogText(string text)
	{
		m_Text = text;
	}

	public void Upgrade(ISettingsProvider provider)
	{
		LogChannel.Default.Log(m_Text);
	}
}
