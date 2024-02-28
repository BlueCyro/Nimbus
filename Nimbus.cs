using HarmonyLib;
using FrooxEngine;
using ResoniteModLoader;
using System.Reflection;
using System.Reflection.Emit;

namespace Nimbus;

public class Nimbus : ResoniteMod
{
    public override string Name => "Nimbus";
    public override string Author => "Cyro";
    public override string Version => "1.0.0";
    public override string Link => "https://www.github.com/RileyGuy/Nimbus";
    public static ModConfiguration? Config;



    public override void OnEngineInit()
    {
        Harmony harmony = new("net.Cyro.Nimbus");
        Config = GetConfiguration();
        Config?.Save(true);
        harmony.PatchAll();
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
                    yield return new(OpCodes.Ldc_I4_0);
                    yield return new(OpCodes.Ldnull);
                    yield return new(OpCodes.Call, legacyCall);
                }
                else
                {
                    yield return inst;
                }
            }
        }
    }
}
