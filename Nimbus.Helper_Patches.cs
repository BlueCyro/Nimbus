using HarmonyLib;
using FrooxEngine;
using System.Reflection;
using System.Reflection.Emit;

namespace Nimbus;

public partial class Nimbus
{
    [HarmonyPatch(typeof(SaveControl))]
    public static class SaveControl_Patches
    {
        static readonly MethodInfo target = typeof(Type).GetProperty("FullName").GetGetMethod();
        static readonly MethodInfo legacyCall = typeof(NET8_Helpers).GetMethod(nameof(NET8_Helpers.TryGetLegacy));

        [HarmonyTranspiler]
        [HarmonyPatch("StoreTypeVersions")]
        public static IEnumerable<CodeInstruction> StoreTypeVersions_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var inst in instructions)
            {
                if (inst.Calls(target))
                {
                    yield return new(OpCodes.Call, legacyCall);
                }
                else
                {
                    yield return inst;
                }
            }
        }
    }



    [HarmonyPatch(typeof(Worker))]
    public static class Worker_Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch("WorkerTypeName", MethodType.Getter)]
        public static bool WorkerTypeName_Prefix(Worker __instance, ref string __result)
        {
            Type type = __instance.WorkerType;
            string typeName = type.TryGetLegacy();
            Debug($"(WorkerTypeName) Re-routing {type.FullName} to {typeName}!");
            __result = typeName;
            return false;
        }
    }



    [HarmonyPatch(typeof(WorkerManager))]
    public static class WorkerManager_Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetTypename")]
        public static bool GetTypename_Prefix(ref string __result, Type type)
        {
            string typeName = type.TryGetLegacy();
            Debug($"(WorkerManager) Re-routing {type.FullName} to {typeName}! ");
            __result = typeName;
            return false;
        }
    }


    
    // Little trickier, so we're patching this manually, hence no attributes
    public static class ThreadWorker_Patches
    {
        static readonly MethodInfo abortInfo = typeof(Thread).GetMethod("Abort", []);

        
        public static IEnumerable<CodeInstruction> Abort_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var inst in instructions)
            {
                if (inst.Calls(abortInfo))
                {
                    yield return new(OpCodes.Call, typeof(Thread).GetMethod("Interrupt"));
                }
                else
                {
                    yield return inst;
                }
            }
        }


            
        public static Exception JobWorker_Finalizer(Exception __exception)
        {
            if (__exception is ThreadInterruptedException)
            {
                Debug($"Caught thread interrupt. Exception: {__exception}");
                return null!;
            }
            else
            {
                return __exception;
            }
        }
    }
}
