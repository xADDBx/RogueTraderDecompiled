using JetBrains.Annotations;

namespace Kingmaker.QA.Arbiter;

public class ArbiterStartupParameters
{
	[CanBeNull]
	public string Arbiter { get; set; }

	[CanBeNull]
	public string ArbiterServer { get; set; }

	[CanBeNull]
	public string ArbiterPlatformDataPath { get; set; }

	[CanBeNull]
	public string ArbiterDisableNotifications { get; set; }

	[CanBeNull]
	public string ArbiterScreenResolution { get; set; }

	[CanBeNull]
	public string ArbiterReportPath { get; set; }

	[CanBeNull]
	public string ArbiterReportCache { get; set; }

	[CanBeNull]
	public string ArbiterExternalLogId { get; set; }

	[CanBeNull]
	public string ArbiterSceneReport { get; set; }

	[CanBeNull]
	public string ArbiterAreaReport { get; set; }

	[CanBeNull]
	public string ArbiterInstruction { get; set; }

	public bool ArbiterTakeMemorySnapshots { get; set; }

	public bool ArbiterKeepFilesAfterUpload { get; set; }

	public bool ArbiterExitOnFinish { get; set; }

	public bool ArbiterRestart { get; set; }

	public string ArbiterInstructionsPart { get; set; }
}
