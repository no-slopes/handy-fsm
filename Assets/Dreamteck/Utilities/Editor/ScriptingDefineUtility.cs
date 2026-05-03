namespace Dreamteck.Editor
{
    using System;
    using System.Reflection;
    using UnityEngine;
    using UnityEditor;

    public static class ScriptingDefineUtility
    {
        public static void Add(string define, BuildTargetGroup target, bool log = false)
        {
            string definesString = GetScriptingDefineSymbols(target);
            if (definesString.Contains(define)) return;
            string[] allDefines = definesString.Split(';');
            ArrayUtility.Add(ref allDefines, define);
            definesString = string.Join(";", allDefines);
            SetScriptingDefineSymbols(target, definesString);
            Debug.Log("Added \"" + define + "\" from " + EditorUserBuildSettings.selectedBuildTargetGroup + " Scripting define in Player Settings");
        }

        public static void Remove(string define, BuildTargetGroup target, bool log = false)
        {
            string definesString = GetScriptingDefineSymbols(target);
            if (!definesString.Contains(define)) return;
            string[] allDefines = definesString.Split(';');
            ArrayUtility.Remove(ref allDefines, define);
            definesString = string.Join(";", allDefines);
            SetScriptingDefineSymbols(target, definesString);
            Debug.Log("Removed \"" + define + "\" from " + EditorUserBuildSettings.selectedBuildTargetGroup + " Scripting define in Player Settings");
        }

        private static string GetScriptingDefineSymbols(BuildTargetGroup target)
        {
            if (TryGetNamedBuildTarget(target, out Type namedBuildTargetType, out object namedBuildTarget))
            {
                MethodInfo getMethod = typeof(PlayerSettings).GetMethod(
                    "GetScriptingDefineSymbols",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { namedBuildTargetType },
                    null);

                if (getMethod != null)
                {
                    return (string)getMethod.Invoke(null, new[] { namedBuildTarget });
                }
            }

#pragma warning disable CS0618
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
#pragma warning restore CS0618
        }

        private static void SetScriptingDefineSymbols(
            BuildTargetGroup target,
            string definesString)
        {
            if (TryGetNamedBuildTarget(target, out Type namedBuildTargetType, out object namedBuildTarget))
            {
                MethodInfo setMethod = typeof(PlayerSettings).GetMethod(
                    "SetScriptingDefineSymbols",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { namedBuildTargetType, typeof(string) },
                    null);

                if (setMethod != null)
                {
                    setMethod.Invoke(null, new[] { namedBuildTarget, definesString });
                    return;
                }
            }

#pragma warning disable CS0618
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, definesString);
#pragma warning restore CS0618
        }

        private static bool TryGetNamedBuildTarget(
            BuildTargetGroup target,
            out Type namedBuildTargetType,
            out object namedBuildTarget)
        {
            namedBuildTargetType =
                Type.GetType("UnityEditor.Build.NamedBuildTarget, UnityEditor.CoreModule")
                ?? Type.GetType("UnityEditor.Build.NamedBuildTarget, UnityEditor")
                ?? Type.GetType("UnityEditor.NamedBuildTarget, UnityEditor.CoreModule")
                ?? Type.GetType("UnityEditor.NamedBuildTarget, UnityEditor");

            namedBuildTarget = null;

            if (namedBuildTargetType == null)
            {
                return false;
            }

            MethodInfo fromBuildTargetGroup = namedBuildTargetType.GetMethod(
                "FromBuildTargetGroup",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(BuildTargetGroup) },
                null);

            if (fromBuildTargetGroup == null)
            {
                return false;
            }

            namedBuildTarget = fromBuildTargetGroup.Invoke(null, new object[] { target });
            return namedBuildTarget != null;
        }
    }
}
