using HarmonyLib;
using ResoniteModLoader;
using System.Reflection;
using Elements.Core;

namespace Nimbus;

#pragma warning disable CS1591

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

        // The ThreadWorker's Abort() method
        MethodInfo threadWorkerAbort = 
            typeof(WorkProcessor)
            .GetNestedType("ThreadWorker", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetMethod("Abort");
        
        // The WorkProcessor's JobWorker() method
        MethodInfo workProcessorJobWorker =
            typeof(WorkProcessor)
            .GetMethod("JobWorker", BindingFlags.Instance | BindingFlags.NonPublic);

        // Patch the ThreadWorker's Abort() method to use interrupt instead
        MethodInfo abortPatch = ((Delegate)ThreadWorker_Patches.Abort_Transpiler).Method;

        // Finalize the WorkProcessor's JobWorker() method to swallow exceptions from usage of Thread.Interrupt()
        MethodInfo jobWorkerFinalizer = ((Delegate)ThreadWorker_Patches.JobWorker_Finalizer).Method;

        harmony.Patch(threadWorkerAbort, transpiler: new(abortPatch));
        harmony.Patch(workProcessorJobWorker, finalizer: new(jobWorkerFinalizer));
    }
}

#pragma warning restore CS1591