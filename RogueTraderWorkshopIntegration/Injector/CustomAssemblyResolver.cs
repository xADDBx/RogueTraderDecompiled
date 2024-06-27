using System.IO;
using Mono.Cecil;

namespace Injector;

internal class CustomAssemblyResolver : BaseAssemblyResolver
{
	private readonly string[] _directories;

	public CustomAssemblyResolver(params string[] directories)
	{
		_directories = directories;
	}

	public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
	{
		try
		{
			return base.Resolve(name, parameters);
		}
		catch (AssemblyResolutionException)
		{
			string[] array = _directories;
			for (int i = 0; i < array.Length; i++)
			{
				string text = Path.Combine(array[i], name.Name + ".dll");
				if (File.Exists(text))
				{
					return AssemblyDefinition.ReadAssembly(text, parameters);
				}
			}
			throw;
		}
	}
}
