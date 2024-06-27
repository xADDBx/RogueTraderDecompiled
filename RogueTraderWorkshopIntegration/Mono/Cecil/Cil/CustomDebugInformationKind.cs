using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public enum CustomDebugInformationKind
{
	Binary,
	StateMachineScope,
	DynamicVariable,
	DefaultNamespace,
	AsyncMethodBody,
	EmbeddedSource,
	SourceLink
}
