using HarmonyLib;
using ResoniteModLoader;
using System.Reflection;
using Elements.Core;

namespace Nimbus;

public partial class Nimbus : ResoniteMod
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

        // Patch the ThreadWorker's abort method to use interrupt instead
        MethodInfo threadWorkerAbort =
            typeof(WorkProcessor)
            .GetNestedType("ThreadWorker", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetMethod("Abort");
        

        MethodInfo workProcessorJobWorker =
            typeof(WorkProcessor)
            .GetMethod("JobWorker", BindingFlags.Instance | BindingFlags.NonPublic);


        MethodInfo abortPatch = ((Delegate)ThreadWorker_Patches.Abort_Transpiler).Method;
        MethodInfo jobWorkerFinalizer = ((Delegate)ThreadWorker_Patches.JobWorker_Finalizer).Method;

        harmony.Patch(threadWorkerAbort, transpiler: new(abortPatch));
        harmony.Patch(workProcessorJobWorker, finalizer: new(jobWorkerFinalizer));
    }
}
