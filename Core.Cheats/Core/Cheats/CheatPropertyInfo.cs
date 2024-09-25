namespace Core.Cheats;

public class CheatPropertyInfo
{
	public string Name { get; set; }

	public string Description { get; set; }

	public string ExampleUsage { get; set; }

	public ExecutionPolicy ExecutionPolicy { get; set; }

	public string Type { get; set; }

	public CheatPropertyInfo(string name, string description, string exampleUsage, ExecutionPolicy executionPolicy, string type)
	{
		Name = name;
		Description = description;
		ExampleUsage = exampleUsage;
		ExecutionPolicy = executionPolicy;
		Type = type;
	}

	public CheatPropertyInfo()
	{
	}
}
