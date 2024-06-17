using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Enums.Damage;

public static class PhysicalDamageFormExtension
{
	public static bool Contains(this PhysicalDamageForm form, PhysicalDamageForm component)
	{
		return (form & component) == component;
	}

	public static bool Intersects(this PhysicalDamageForm form, PhysicalDamageForm component)
	{
		return (form & component) != 0;
	}

	public static IEnumerable<PhysicalDamageForm> Components(this PhysicalDamageForm form)
	{
		return from c in EnumUtils.GetValues<PhysicalDamageForm>()
			where form.Contains(c)
			select c;
	}
}
