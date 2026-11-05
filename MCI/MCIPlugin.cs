using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MCI.Components;
using MiraAPI;
using Reactor;
using Reactor.Utilities;

namespace MCI;

[BepInAutoPlugin("auavengers.mci", "MCI")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[BepInDependency(SubmergedCompatibility.SUBMERGED_GUID, BepInDependency.DependencyFlags.SoftDependency)]
public partial class MCIPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);

    internal static ManualLogSource Logger { get; private set; }

    public static string RobotName { get; set; } = "Bot";

    public static bool Enabled { get; set; } = true;
    public static bool IKnowWhatImDoing { get; set; }
    public static bool Persistence { get; set; } = true;

    public static Debugger Debugger { get; set; }

    /// <summary>
    ///     Determines if the current build is a dev build.
    /// </summary>
    public static bool IsDevBuild => Version.Contains("dev", StringComparison.OrdinalIgnoreCase) || Version.Contains("ci", StringComparison.OrdinalIgnoreCase);

    private ConfigEntry<bool> logControls;

    public override void Load()
    {
        ReactorCredits.Register("MCI", Version, IsDevBuild, ReactorCredits.AlwaysShow);
        Logger = Log;

        Harmony.PatchAll();
        SubmergedCompatibility.Initialize();

        ClassInjector.RegisterTypeInIl2Cpp<Debugger>();
        Debugger = AddComponent<Debugger>();

        logControls = Config.Bind("Logging", "Log Controls In Console", false);

        if (!logControls.Value)
            return;

        Logger.LogWarning("\n" +
                        "|------------------------|\n" + // Yellow stands out.
                        "| MultiClientInstancing  |\n" +
                        "|                        |\n" +
                        "| Developed By:          |\n" +
                        "| MyDragonBreath         |\n" +
                        "| WhichTwix              |\n" +
                        "| AlchlcDvl              |\n" +
                        "| lekillerdesgames       |\n" +
                        "| AtonyGit               |\n" +
                        "|------------------------|\n" +
                        "| Controls:              |\n" +
                        "| F5: Add Players        |\n" +
                        "|  + LSHIFT To Bypass 15 |\n" +
                        "| F6: Toggle Bot Removal |\n" +
                        "|  + LSHIFT IKWIDM       |\n" +
                        "| F9: Cycle Upwards      |\n" +
                        "| F10: Cycle Downwards   |\n" +
                        "| F11: Remove All        |\n" +
                        "| F1: Toggle Debugger    |\n" +
                        "|------------------------|");
    }
}
