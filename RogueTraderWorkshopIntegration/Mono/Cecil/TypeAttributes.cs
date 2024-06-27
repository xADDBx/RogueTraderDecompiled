using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil;

[Flags]
[ComVisible(false)]
public enum TypeAttributes : uint
{
	VisibilityMask = 7u,
	NotPublic = 0u,
	Public = 1u,
	NestedPublic = 2u,
	NestedPrivate = 3u,
	NestedFamily = 4u,
	NestedAssembly = 5u,
	NestedFamANDAssem = 6u,
	NestedFamORAssem = 7u,
	LayoutMask = 0x18u,
	AutoLayout = 0u,
	SequentialLayout = 8u,
	ExplicitLayout = 0x10u,
	ClassSemanticMask = 0x20u,
	Class = 0u,
	Interface = 0x20u,
	Abstract = 0x80u,
	Sealed = 0x100u,
	SpecialName = 0x400u,
	Import = 0x1000u,
	Serializable = 0x2000u,
	WindowsRuntime = 0x4000u,
	StringFormatMask = 0x30000u,
	AnsiClass = 0u,
	UnicodeClass = 0x10000u,
	AutoClass = 0x20000u,
	BeforeFieldInit = 0x100000u,
	RTSpecialName = 0x800u,
	HasSecurity = 0x40000u,
	Forwarder = 0x200000u
}
