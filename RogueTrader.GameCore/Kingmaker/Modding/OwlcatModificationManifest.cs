using System;
using JetBrains.Annotations;

namespace Kingmaker.Modding;

[Serializable]
public class OwlcatModificationManifest
{
	[Serializable]
	[UsedImplicitly]
	public class Dependency
	{
		public string Name = "";

		public string Version = "";
	}

	public string UniqueName = "";

	public string Version = "";

	public string DisplayName = "";

	public string Description = "";

	public string Author = "";

	public string Repository = "";

	public string HomePage = "";

	public Dependency[] Dependencies = new Dependency[0];
}
