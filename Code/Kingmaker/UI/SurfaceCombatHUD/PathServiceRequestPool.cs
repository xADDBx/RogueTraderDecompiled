using System.Collections.Generic;

namespace Kingmaker.UI.SurfaceCombatHUD;

public sealed class PathServiceRequestPool
{
	private readonly Stack<PathServiceRequest> m_Stack = new Stack<PathServiceRequest>();

	public PathServiceRequest Get()
	{
		if (m_Stack.TryPop(out var result))
		{
			return result;
		}
		return new PathServiceRequest();
	}

	public void Release(PathServiceRequest request)
	{
		request.Clear();
		m_Stack.Push(request);
	}
}
