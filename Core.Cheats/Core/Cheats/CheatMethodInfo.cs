namespace Core.Cheats;

public class CheatMethodInfo
{
	public string Name { get; set; }

	public string Description { get; set; }

	public string ExampleUsage { get; set; }

	public ExecutionPolicy ExecutionPolicy { get; set; }

	public CheatParameter[] Parameters { get; set; } = new CheatParameter[0];


	public string ReturnType { get; set; }

	public CheatMethodInfo(string name, string description, string exampleUsage, ExecutionPolicy executionPolicy, CheatParameter[] parameters, string returnType)
	{
		Name = name;
		Description = description;
		ExampleUsage = exampleUsage;
		ExecutionPolicy = executionPolicy;
		Parameters = parameters;
		ReturnType = returnType;
	}

	public CheatMethodInfo()
	{
	}
}
