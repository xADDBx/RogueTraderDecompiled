using System.Text;
using Kingmaker.ElementsSystem.ContextData;

namespace Owlcat.Plugins.DotNetExtensions;

public class PooledStringBuilderContextData : ContextData<PooledStringBuilderContextData>
{
	public readonly StringBuilder Builder = new StringBuilder(64);

	protected override void Reset()
	{
		Builder.Clear();
	}
}
