using System.Collections.Generic;

namespace Kingmaker.TextTools.Base;

public abstract class TextTemplate
{
	public virtual int Balance => 0;

	public virtual int MinParameters => 0;

	public virtual int MaxParameters => 0;

	public abstract string Generate(bool capitalized, List<string> parameters);
}
