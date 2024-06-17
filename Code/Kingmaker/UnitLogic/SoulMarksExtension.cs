using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic;

public static class SoulMarksExtension
{
	public static IEnumerable<SoulMark> GetSoulMarks(this MechanicEntity entity)
	{
		return entity.Facts.GetAll<SoulMark>();
	}
}
