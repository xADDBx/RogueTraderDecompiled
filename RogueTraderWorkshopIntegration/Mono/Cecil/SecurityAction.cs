using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public enum SecurityAction : ushort
{
	Request = 1,
	Demand,
	Assert,
	Deny,
	PermitOnly,
	LinkDemand,
	InheritDemand,
	RequestMinimum,
	RequestOptional,
	RequestRefuse,
	PreJitGrant,
	PreJitDeny,
	NonCasDemand,
	NonCasLinkDemand,
	NonCasInheritance
}
