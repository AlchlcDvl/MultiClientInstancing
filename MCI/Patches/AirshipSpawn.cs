using HarmonyLib;

namespace MCI.Patches;

[HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
public static class AirshipSpawn
{
    public static void Postfix(SpawnInMinigame __instance)
    {
        if (!MCIPlugin.Enabled)
            return;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data.PlayerName.Contains(MCIPlugin.RobotName))
                continue;

            var rand = UnityEngine.Random.Range(0, __instance.LocationButtons.Count(x => x.gameObject.activeSelf && x.enabled));
            player.gameObject.SetActive(true);
            __instance.LocationButtons[rand].ReceiveClickUp();
        }
    }
}
