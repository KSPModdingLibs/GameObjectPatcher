using HarmonyLib;
using KSPBuildTools;
using System;
using System.Collections.Generic;
using UniLinq;

namespace Mutiny
{
	static public class TypeExtensions
	{
		public static Type GetSubclassNamed(this Type thisType, string name)
		{
			// This doesn't work because GetSubclassesOfParentClass doesn't include the builtin unity types
			//List<Type> subclassTypes = AssemblyLoader.GetSubclassesOfParentClass(thisType);
			//Type result = subclassTypes.FirstOrDefault(t => t.Name == name);

			// probably horribly slow, but we can cache it later.

			Type result = AccessTools.TypeByName(name);

			if (result == null || !thisType.IsAssignableFrom(result))
			{
				Log.Error($"Could not find subclass of {thisType.Name} named {name}");
			}

			return result;
		}
	}
}