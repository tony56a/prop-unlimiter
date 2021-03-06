﻿using Harmony.ILCopying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Harmony
{
	public static class PatchFunctions
	{
		public static void AddPrefix(PatchInfo patchInfo, string owner, HarmonyMethod info)
		{
			if (info == null || info.method == null) return;

			var priority = info.prioritiy == -1 ? Priority.Normal : info.prioritiy;
			var before = info.before ?? new string[0];
			var after = info.after ?? new string[0];

			patchInfo.AddPrefix(info.method, owner, priority, before, after);
		}

		public static void AddPostfix(PatchInfo patchInfo, string owner, HarmonyMethod info)
		{
			if (info == null || info.method == null) return;

			var priority = info.prioritiy == -1 ? Priority.Normal : info.prioritiy;
			var before = info.before ?? new string[0];
			var after = info.after ?? new string[0];

			patchInfo.AddPostfix(info.method, owner, priority, before, after);
		}

		public static void AddInfix(PatchInfo patchInfo, string owner, HarmonyMethod info)
		{
			if (info == null || info.method == null) return;

			var priority = info.prioritiy == -1 ? Priority.Normal : info.prioritiy;
			var before = info.before ?? new string[0];
			var after = info.after ?? new string[0];

			patchInfo.AddProcessor(info.method, owner, priority, before, after);
		}

		public static List<MethodInfo> GetSortedPatchMethods(MethodBase original, Patch[] patches)
		{
			return patches
				.Where(p => p.patch != null)
				.OrderBy(p => p)
				.Select(p => p.GetMethod(original))
				.ToList();
		}

		public static List<ICodeProcessor> GetSortedProcessors(MethodBase original, Patch[] processors)
		{
			return processors
				.Where(p => p.patch != null)
				.OrderBy(p => p)
				.SelectMany(p => p.GetProcessor(original).processors)
				.ToList();
		}

		public static void UpdateWrapper(MethodBase original, PatchInfo patchInfo)
		{
			var sortedPrefixes = GetSortedPatchMethods(original, patchInfo.prefixes);
			var sortedPostfixes = GetSortedPatchMethods(original, patchInfo.postfixes);
			var sortedProcessors = GetSortedProcessors(original, patchInfo.processors);

			var replacement = MethodPatcher.CreatePatchedMethod(original, sortedPrefixes, sortedPostfixes, sortedProcessors);
			if (replacement == null) throw new MissingMethodException("Cannot create dynamic replacement for " + original);

			var originalCodeStart = Memory.GetMethodStart(original);
			var patchCodeStart = Memory.GetMethodStart(replacement);
			Memory.WriteJump(originalCodeStart, patchCodeStart);

			PatchTools.RememberObject(original, replacement); // no gc for new value + release old value to gc
		}
	}
}