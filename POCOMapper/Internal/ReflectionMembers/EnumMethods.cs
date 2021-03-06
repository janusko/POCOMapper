﻿using System;
using System.Reflection;

namespace KST.POCOMapper.Internal.ReflectionMembers
{
	internal static class EnumMethods
	{
		private static readonly MethodInfo aParse = typeof(Enum).GetMethod(nameof(Enum.Parse), BindingFlags.Static | BindingFlags.Public);

		public static MethodInfo Parse(Type enumType)
		{
			return aParse;
		}
	}
}
