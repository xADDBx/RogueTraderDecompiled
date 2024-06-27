namespace StateHasher.Core;

public interface IHasherLogger
{
	void Log(string message);

	void Error(string message);
}
