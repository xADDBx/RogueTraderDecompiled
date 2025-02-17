namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput.ConsoleTypeProviders;

public class OverriddenPlatformProvider : ConsoleTypeProvider
{
	private ConsoleType? m_ConsoleType;

	public override bool TryGetConsoleType(out ConsoleType type)
	{
		type = m_ConsoleType.GetValueOrDefault();
		return m_ConsoleType.HasValue;
	}

	public void SetConsoleType(ConsoleType type)
	{
		m_ConsoleType = type;
	}

	public void Clear()
	{
		m_ConsoleType = null;
	}
}
