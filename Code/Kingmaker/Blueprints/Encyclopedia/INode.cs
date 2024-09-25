using System.Collections.Generic;

namespace Kingmaker.Blueprints.Encyclopedia;

public interface INode
{
	bool FirstExpanded { get; }

	string GetTitle();

	bool IsChilds();

	List<IPage> GetChilds();
}
