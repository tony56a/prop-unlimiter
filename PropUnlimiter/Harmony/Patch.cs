﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Harmony
{
	public static class PatchInfoSerialization
	{
		class Binder : SerializationBinder
		{
			public override Type BindToType(string assemblyName, string typeName)
			{
				var types = new Type[] {
					typeof(PatchInfo),
					typeof(Patch[]),
					typeof(Patch)
				};
				foreach (var type in types)
					if (typeName == type.FullName)
						return type;
				var typeToDeserialize = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
				return typeToDeserialize;
			}
		}

		public static byte[] Serialize(this PatchInfo patchInfo)
		{
#pragma warning disable XS0001
			using (var streamMemory = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(streamMemory, patchInfo);
				return streamMemory.GetBuffer();
			}
#pragma warning restore XS0001
		}

		public static PatchInfo Deserialize(byte[] bytes)
		{
			var formatter = new BinaryFormatter();
			formatter.Binder = new Binder();
#pragma warning disable XS0001
			var streamMemory = new MemoryStream(bytes);
#pragma warning restore XS0001
			return (PatchInfo)formatter.Deserialize(streamMemory);
		}

		// general sorting by (in that order): before, after, priority and index
		public static int PriorityComparer(object obj, int index, int priority, string[] before, string[] after)
		{
			var trv = Traverse.Create(obj);
			var theirOwner = trv.Field("owner").GetValue<string>();
			var theirPriority = trv.Field("priority").GetValue<int>();
			var theirIndex = trv.Field("index").GetValue<int>();

			if (before != null && Array.IndexOf(before, theirOwner) > -1)
				return -1;
			if (after != null && Array.IndexOf(after, theirOwner) > -1)
				return 1;

			if (priority != theirPriority)
				return -(priority.CompareTo(theirPriority));

			return index.CompareTo(theirIndex);
		}
	}

	[Serializable]
	public class PatchInfo
	{
		public Patch[] prefixes;
		public Patch[] postfixes;
		public Patch[] processors;

		public PatchInfo()
		{
			prefixes = new Patch[0];
			postfixes = new Patch[0];
			processors = new Patch[0];
		}

		public void AddPrefix(MethodInfo patch, string owner, int priority, string[] before, string[] after)
		{
			var l = prefixes.ToList();
			l.Add(new Patch(patch, prefixes.Count() + 1, owner, priority, before, after));
			prefixes = l.ToArray();
		}

		public void AddPostfix(MethodInfo patch, string owner, int priority, string[] before, string[] after)
		{
			var l = postfixes.ToList();
			l.Add(new Patch(patch, postfixes.Count() + 1, owner, priority, before, after));
			postfixes = l.ToArray();
		}

		public void AddProcessor(MethodInfo patch, string owner, int priority, string[] before, string[] after)
		{
			var l = processors.ToList();
			l.Add(new Patch(patch, processors.Count() + 1, owner, priority, before, after));
			processors = l.ToArray();
		}
	}

	[Serializable]
	public class Patch : IComparable
	{
		readonly public int index;
		readonly public string owner;
		readonly public int priority;
		readonly public string[] before;
		readonly public string[] after;

		readonly public MethodInfo patch;

		public Patch(MethodInfo patch, int index, string owner, int priority, string[] before, string[] after)
		{
			if (patch is DynamicMethod) throw new Exception("Cannot directly reference dynamic method \"" + patch + "\" in Harmony. Use a factory method instead that will return the dynamic method.");

			this.index = index;
			this.owner = owner;
			this.priority = priority;
			this.before = before;
			this.after = after;
			this.patch = patch;
		}

		public HarmonyProcessor GetProcessor(MethodBase original)
		{
			if (patch.ReturnType != typeof(HarmonyProcessor)) throw new Exception("Processor factory " + original + " must have a return type 'HarmonyProcessor'");
			if (patch.IsStatic == false) throw new Exception("Processor factory " + original + " must be static");
			var parameters = patch.GetParameters();
			if (parameters.Count() != 1) throw new Exception("Processor factory " + original + " must have exactly one parameter");
			if (parameters[0].ParameterType != typeof(MethodBase)) throw new Exception("Processor factory " + original + " must have a parameter of type 'MethodBase'");

			return patch.Invoke(null, new object[] { original }) as HarmonyProcessor;
		}

		public MethodInfo GetMethod(MethodBase original)
		{
			if (patch.ReturnType != typeof(DynamicMethod)) return patch;
			if (patch.IsStatic == false) return patch;
			var parameters = patch.GetParameters();
			if (parameters.Count() != 1) return patch;
			if (parameters[0].ParameterType != typeof(MethodBase)) return patch;

			// we have a DynamicMethod factory, let's use it
			return patch.Invoke(null, new object[] { original }) as DynamicMethod;
		}

		public override bool Equals(object obj)
		{
			return ((obj != null) && (obj is Patch) && (patch == ((Patch)obj).patch));
		}

		public int CompareTo(object obj)
		{
			return PatchInfoSerialization.PriorityComparer(obj, index, priority, before, after);
		}

		public override int GetHashCode()
		{
			return patch.GetHashCode();
		}
	}
}