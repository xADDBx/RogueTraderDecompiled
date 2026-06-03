using Kingmaker.Utility;

namespace Kingmaker.QA.Arbiter.GameCore;

public class EditorArbiterEnvironment : IArbiterEnvironment
{
	public string Version => "Editor";

	public string Branch => Repository.GetRepositoryInfo(null)?.BranchName ?? "editor";

	public string Revision => Repository.GetRepositoryInfo(null)?.CommitHash ?? "editor";

	public bool IsAvailable => ArbiterIntegration.IsMainMenuActive();

	public bool IsLoggingEnabled => LoggingConfiguration.IsLoggingEnabled;

	public string ProjectAlias => "WH";
}
