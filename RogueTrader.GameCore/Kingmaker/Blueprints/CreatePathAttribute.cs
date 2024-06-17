using System;

namespace Kingmaker.Blueprints;

public class CreatePathAttribute : Attribute
{
	public string Path;

	public CreatePathAttribute(string path)
	{
		Path = path;
	}
}
