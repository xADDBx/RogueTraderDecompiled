namespace Kingmaker.UI.SurfaceCombatHUD;

public interface IIdentifierContainer
{
	void Push(int cellIdentifier);

	void PushRange(int cellIdentifierBegin, int cellIdentifierEnd);
}
