using System;

namespace Core.Cheats;

public static class CheatsManagerHolder
{
	public readonly struct CheatSystem
	{
		public readonly CheatDatabase Database;

		public readonly CheatExecutorCommand CommandExecutor;

		public readonly CheatExecutorVariable VariableExecutor;

		public readonly CheatExecutorExternal ExternalExecutor;

		public readonly CheatsParser Parser;

		public readonly HelpCommands Help;

		public CheatSystem(CheatDatabase database, CheatExecutorCommand commandExecutor, CheatExecutorVariable variableExecutor, CheatExecutorExternal externalExecutor, CheatsParser parser, HelpCommands help)
		{
			Database = database;
			CommandExecutor = commandExecutor;
			VariableExecutor = variableExecutor;
			ExternalExecutor = externalExecutor;
			Parser = parser;
			Help = help;
		}
	}

	public static CheatsParser Instance => System.Parser;

	public static CheatSystem System { get; set; }

	public static CheatSystem CreateForThisDomain(Action<string, string> externalDelegate)
	{
		CheatDatabase cheatDatabase = new CheatDatabase();
		CheatExecutorCommand cheatExecutorCommand = new CheatExecutorCommand(cheatDatabase);
		CheatExecutorVariable cheatExecutorVariable = new CheatExecutorVariable(cheatDatabase);
		CheatExecutorExternal cheatExecutorExternal = new CheatExecutorExternal(cheatDatabase, externalDelegate);
		HelpCommands help = new HelpCommands(cheatDatabase);
		CheatsParser parser = new CheatsParser(cheatDatabase, cheatExecutorExternal.ExecuteExternalWithDefaultLogging, cheatExecutorCommand.ExecuteCommandWithDefaultLogging, cheatExecutorVariable.ExecuteGetVariableWithDefaultLogging, cheatExecutorVariable.ExecuteSetVariableWithDefaultLogging);
		return new CheatSystem(cheatDatabase, cheatExecutorCommand, cheatExecutorVariable, cheatExecutorExternal, parser, help);
	}
}
