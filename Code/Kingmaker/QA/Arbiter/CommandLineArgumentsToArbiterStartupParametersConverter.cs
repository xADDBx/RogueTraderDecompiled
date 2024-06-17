using Kingmaker.Utility.CommandLineArgs;

namespace Kingmaker.QA.Arbiter;

public class CommandLineArgumentsToArbiterStartupParametersConverter
{
	public static ArbiterStartupParameters Convert(CommandLineArguments arguments)
	{
		ArbiterStartupParameters arbiterStartupParameters = new ArbiterStartupParameters
		{
			Arbiter = (arguments.Contains("-arbiter") ? arguments.Get("-arbiter") : null),
			ArbiterServer = (arguments.Contains("-arbiterServer") ? arguments.Get("-arbiterServer") : null),
			ArbiterInstruction = (arguments.Contains("-arbiterInstruction") ? arguments.Get("-arbiterInstruction") : null),
			ArbiterPlatformDataPath = (arguments.Contains("-arbiterPlatformDataPath") ? arguments.Get("-arbiterPlatformDataPath") : null),
			ArbiterDisableNotifications = (arguments.Contains("-arbiterDisableNotifications") ? arguments.Get("-arbiterDisableNotifications") : null),
			ArbiterScreenResolution = (arguments.Contains("-arbiterScreenResolution") ? arguments.Get("-arbiterScreenResolution") : null),
			ArbiterReportPath = (arguments.Contains("-arbiterReportPath") ? arguments.Get("-arbiterReportPath") : null),
			ArbiterReportCache = (arguments.Contains("-arbiterReportCache") ? arguments.Get("-arbiterReportCache") : null),
			ArbiterExternalLogId = (arguments.Contains("-arbiterExternalLogId") ? arguments.Get("-arbiterExternalLogId") : null),
			ArbiterSceneReport = (arguments.Contains("-arbiterSceneReport") ? arguments.Get("-arbiterSceneReport") : null),
			ArbiterAreaReport = (arguments.Contains("-arbiterAreaReport") ? arguments.Get("-arbiterAreaReport") : null),
			ArbiterExitOnFinish = arguments.Contains("-arbiterExitOnFinish"),
			ArbiterRestart = arguments.Contains("-arbiterRestart"),
			ArbiterInstructionsPart = (arguments.Contains("-arbiterInstructionsPart") ? arguments.Get("-arbiterInstructionsPart") : null),
			ArbiterKeepFilesAfterUpload = arguments.Contains("-arbiterKeepFilesAfterUpload"),
			ArbiterTakeMemorySnapshots = arguments.Contains("-arbiterTakeMemorySnapshots")
		};
		arbiterStartupParameters.ArbiterScreenResolution = (arguments.Contains("-size") ? arguments.Get("-size") : null);
		PFLog.Arbiter.Log(string.Join("\n", "Arbiter params: ", "arbiter: " + (arbiterStartupParameters.Arbiter ?? "null"), "arbiterServer: " + (arbiterStartupParameters.ArbiterServer ?? "null"), "arbiterInstruction: " + (arbiterStartupParameters.ArbiterInstruction ?? "null"), "arbiterPlatformDataPath: " + (arbiterStartupParameters.ArbiterPlatformDataPath ?? "null"), "arbiterDisableNotifications: " + (arbiterStartupParameters.ArbiterDisableNotifications ?? "null"), "arbiterScreenResolution: " + (arbiterStartupParameters.ArbiterScreenResolution ?? "null"), "arbiterReportPath: " + (arbiterStartupParameters.ArbiterReportPath ?? "null"), "arbiterReportCache: " + (arbiterStartupParameters.ArbiterReportCache ?? "null"), "arbiterExternalLogId: " + (arbiterStartupParameters.ArbiterExternalLogId ?? "null"), "arbiterSceneReport: " + (arbiterStartupParameters.ArbiterSceneReport ?? "null"), "arbiterAreaReport: " + (arbiterStartupParameters.ArbiterAreaReport ?? "null"), $"arbiterExitOnFinish: {arbiterStartupParameters.ArbiterExitOnFinish}", $"arbiterRestart: {arbiterStartupParameters.ArbiterRestart}", "arbiterInstructionsPart: " + arbiterStartupParameters.ArbiterInstructionsPart, $"arbiterKeepFilesAfterUpload: {arbiterStartupParameters.ArbiterKeepFilesAfterUpload}", $"arbiterTakeMemorySnapshots: {arbiterStartupParameters.ArbiterTakeMemorySnapshots}"));
		return arbiterStartupParameters;
	}
}
