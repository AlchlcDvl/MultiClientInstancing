using AmongUs.Data;
using Il2CppInterop.Runtime.Attributes;
using InnerNet;
using MCI.Patches;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Utilities.ImGui;
using UnityEngine;

namespace MCI.Components;

public class Debugger(IntPtr ptrAlt) : MonoBehaviour(ptrAlt)
{
    [HideFromIl2Cpp]
    public DragWindow Window { get; } = new(new(20, 20, 0, 0), "MCI Debugger", () =>
    {
        GUILayout.Label($"{DataManager.Player.customization.Name} - Press F1 To Hide");

        var mouse = Input.mousePosition;
        GUILayout.Label($"Mouse Position\nx: {mouse.x:00.00} y: {mouse.y:00.00} z: {mouse.z:00.00}");

        if (PlayerControl.LocalPlayer)
        {
            GUILayout.Label($"{PlayerControl.LocalPlayer.CurrentOutfit.PlayerName}");
            var position = PlayerControl.LocalPlayer.gameObject.transform.position;
            GUILayout.Label($"Player Position\nx: {position.x:00.00} y: {position.y:00.00} z: {position.z:00.00}");

            if (MCIPlugin.Enabled && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                PlayerControl.LocalPlayer.Collider.enabled = GUILayout.Toggle(PlayerControl.LocalPlayer.Collider.enabled, "Enable Player Collider");
            }
        }

        if (!MCIPlugin.Enabled || !PlayerControl.LocalPlayer)
        {
            GUILayout.Label("Debugger features only work on localhosted lobbies");
            return;
        }

        if (AmongUsClient.Instance == null || GameManager.Instance == null ||
            !(AmongUsClient.Instance.GameState is InnerNetClient.GameStates.Joined
                or InnerNetClient.GameStates.Started ||
            (GameManager.Instance.GameHasStarted &&
                AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Ended)))
        {
            return;
        }

        if (GUILayout.Button($"Spawn Bot ({InstanceControl.Clients.Count}/15)"))
        {
            Keyboard_Joystick.CreatePlayer();
        }
        GUILayout.Label("Hold LftShft when pressing to bypass player limit.");

        if (GUILayout.Button("Remove Last Bot"))
            InstanceControl.RemovePlayer((byte)InstanceControl.Clients.Count);

        if (GUILayout.Button("Remove All Bots"))
            InstanceControl.RemoveAllPlayers();

        if (GUILayout.Button("Next Player"))
            Keyboard_Joystick.Switch(true);

        if (GUILayout.Button("Previous Player"))
            Keyboard_Joystick.Switch(false);

        if (GUILayout.Button("End Game"))
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsBySabotage, false);

        if (GUILayout.Button("Turn Impostor"))
        {
            PlayerControl.LocalPlayer.Data.Role.TeamType = RoleTeamTypes.Impostor;
            if (!PlayerControl.LocalPlayer.Data.IsDead)
            {
                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, AmongUs.GameOptions.RoleTypes.Impostor);
                DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(true);
                PlayerControl.LocalPlayer.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);
            }
            else
            {
                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, AmongUs.GameOptions.RoleTypes.ImpostorGhost);
            }
        }

        if (GUILayout.Button("Turn Crewmate"))
        {
            PlayerControl.LocalPlayer.Data.Role.TeamType = RoleTeamTypes.Crewmate;
            if (!PlayerControl.LocalPlayer.Data.IsDead)
                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, AmongUs.GameOptions.RoleTypes.Crewmate);
            else
                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, AmongUs.GameOptions.RoleTypes.CrewmateGhost);
        }

        if (GUILayout.Button("Complete Tasks"))
            foreach (var task in PlayerControl.LocalPlayer.myTasks)
            {
                PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);
            }

        if (GUILayout.Button("Complete Everyone's Tasks"))
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                foreach (var task in player.myTasks)
                {
                    player.RpcCompleteTask(task.Id);
                }
            }

        if (GUILayout.Button("Redo Intro Sequence"))
        {
            HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black));
            HudManager.Instance.StartCoroutine(HudManager.Instance.CoShowIntro());
        }

        if (!MeetingHud.Instance && GUILayout.Button("Start Meeting"))
        {
            PlayerControl.LocalPlayer.RemainingEmergencies++;
            PlayerControl.LocalPlayer.CmdReportDeadBody(null);
        }

        if (GUILayout.Button("End Meeting") && MeetingHud.Instance)
            MeetingHud.Instance.RpcClose();

        if (GUILayout.Button("Kill Self"))
            PlayerControl.LocalPlayer.RpcCustomMurder(PlayerControl.LocalPlayer, showKillAnim: false);

        if (GUILayout.Button("Kill All"))
        {
            foreach (var player in Helpers.GetAlivePlayers())
                PlayerControl.LocalPlayer.RpcCustomMurder(player, showKillAnim: false);
        }

        if (GUILayout.Button("Hide Player Names"))
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                player.cosmetics.nameTextContainer.gameObject.SetActive(false);
            }
        }
        if (GUILayout.Button("Show Player Names"))
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                player.cosmetics.nameTextContainer.gameObject.SetActive(true);
            }
        }
        if (GUILayout.Button("Toggle Shadows") && HudManager.InstanceExists)
        {
            HudManager.Instance.ShadowQuad.gameObject.SetActive(!HudManager.Instance.ShadowQuad.gameObject.active);
        }
    });

    public bool WindowEnabled { get; set; } = true;

    public void OnGUI()
    {
        if (WindowEnabled)
            Window.OnGUI();
    }

    public void Toggle()
    {
        WindowEnabled = !WindowEnabled;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            Toggle();
    }
}
