using System.Text;
using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Utility.DotNetExtensions;

public class PooledStringBuilder : ContextData<PooledStringBuilder>
{
	public readonly StringBuilder Builder = new StringBuilder(64);

	protected override void Reset()
	{
		Builder.Clear();
	}
}
