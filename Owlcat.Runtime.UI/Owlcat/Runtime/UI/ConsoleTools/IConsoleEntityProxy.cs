namespace Owlcat.Runtime.UI.ConsoleTools;

public interface IConsoleEntityProxy : IConsoleEntity
{
	IConsoleEntity ConsoleEntityProxy { get; }
}
