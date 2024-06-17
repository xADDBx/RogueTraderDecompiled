using Kingmaker.Blueprints.Credits;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public interface ICreditsBlock : ICreditsView
{
	void Initialize(ICreditsView view);

	void Append(string row, BlueprintCreditsGroup group);

	void Ping(int row);

	void Show();

	void Hide();
}
