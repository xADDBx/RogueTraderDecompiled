using System;
using System.Threading.Tasks;
using Core.Cheats;
using Core.Cheats.ServerPlugins;
using Core.Console;
using Core.Console.ServerPlugins;
using Core.RestServer;
using Core.RestServer.Common;
using Core.StateCrawler.ServerPlugins;
using Kingmaker.GameCommands.Cheats;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Logging;

public static class CheatGameCommandSystem
{
	public static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Console");

	private static RestServer s_Server;

	private static void Destroy()
	{
		s_Server?.Dispose();
		s_Server = null;
		CheatsManagerHolder.System = default(CheatsManagerHolder.CheatSystem);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static async void Init()
	{
		CheatsManagerHolder.CheatSystem system = (CheatsManagerHolder.System = CreateForThisDomainWithExternals(SmartConsole.ExecuteLineImpl));
		s_Server?.Dispose();
		s_Server = new RestServer(CreateServerPlugins(in system));
		if (!BuildModeUtility.IsDevelopment)
		{
			ConsoleLogSink.SetMessageBufferSize(100);
		}
		Application.quitting += Destroy;
		await Task.Delay(15000);
		try
		{
			await s_Server.Start();
			await system.Parser.Execute("exec autoexec.cfg True");
		}
		catch (Exception ex)
		{
			LogChannel.System.Exception(ex);
		}
	}

	private static IRestServerPlugin[] CreateServerPlugins(in CheatsManagerHolder.CheatSystem system)
	{
		return new IRestServerPlugin[9]
		{
			new ConsolePlugin(ConsoleLogSink.Poll),
			new CommandPlugin(RunCheatCommandGameCommand.Create),
			new ExternalPlugin(RunExternalCheatGameCommand.Create),
			new GetVariablePlugin(system.VariableExecutor.ExecuteGetVariableWithDefaultLogging),
			new SetVariablePlugin(SetCheatVariableGameCommand.Create),
			new AutoCompletePlugin(system.Parser),
			new KnownPlugin(system.Database),
			new BindingsPlugin(),
			new DumpStatePlugin()
		};
	}

	private static CheatsManagerHolder.CheatSystem CreateForThisDomainWithExternals(Action<string, string> externalDelegate)
	{
		CheatDatabase cheatDatabase = new CheatDatabase();
		CheatExecutorCommand commandExecutor = new CheatExecutorCommand(cheatDatabase);
		CheatExecutorVariable cheatExecutorVariable = new CheatExecutorVariable(cheatDatabase);
		CheatExecutorExternal externalExecutor = new CheatExecutorExternal(cheatDatabase, externalDelegate);
		HelpCommands help = new HelpCommands(cheatDatabase);
		CheatsParser parser = new CheatsParser(cheatDatabase, RunExternalCheatGameCommand.Create, RunCheatCommandGameCommand.Create, cheatExecutorVariable.ExecuteGetVariableWithDefaultLogging, SetCheatVariableGameCommand.Create);
		return new CheatsManagerHolder.CheatSystem(cheatDatabase, commandExecutor, cheatExecutorVariable, externalExecutor, parser, help);
	}
}
