using CalloutInterfaceAPI;
using Rage;
using Rage.Native;
using Adam69Callouts.Common;

namespace Adam69Callouts.Callouts
{


    [CalloutInterface("[Adam69 Callouts] Officer Down", CalloutProbability.Medium, "Reports of an officer down", "Code3", "LEO")]

    public class OfficerDown : Callout
    {

        private static readonly string[] pedsList = new string[] { "s_f_y_cop_01", "s_m_y_cop_01", "csb_cop", "s_f_y_sheriff_01", "s_m_y_sheriff_01", "s_m_y_hwaycop_01", "s_m_m_security_01", "s_f_y_ranger_01", "s_m_y_ranger_01" };
        private static Ped suspect;
        private static Ped officer;
        private static Blip copBlip;
        private static Vehicle emergencyVehicle;
        private static readonly string[] officerVehicle = new string[] { "police", "police2", "police3", "police4", "police5", "polgauntlet", "poldominator10", "poldorado", "polgreenwood", "polimpaler5", "polimpaler6", "polcaracara", "polcoquette4", "polfaction2", "polterminus", "dilettante2", "fbi", "pbus", "policeb", "pranger", "riot", "riot2", "sheriff", "sheriff2", "policeb2" };
        private static Blip officerVehicleBlip;
        private static Vector3 spawnpoint;
        private static Vector3 vehicleSpawn;
        private static Vector3 susSpawn;
        private static float vehicleHeading;
        private static float officerheading;
        private static float susHeading;
        private static Blip suspectBlip;
        private static int counter;
        private static string malefemale;
        private static readonly int armorCount = 1500; // Set the armor value for the officer and suspect

        // NEW: shooting behavior fields
        private static bool suspectStartedShooting = false;
        private static readonly Vector3 stripClubPosition = new(127.0f, -1297.0f, 29.2f); // interior entry-ish position

        public static bool IsDlcInstalled(string dlcName)
        {
            uint dlcHash = Game.GetHashKey(dlcName);
            return NativeFunction.CallByName<bool>("IS_DLC_PRESENT", dlcHash);
        }

        public override bool OnBeforeCalloutDisplayed()
        {
            if (!DLCManager.AreRequiredDLCsInstalled())
            {
                string missing = DLCManager.GetMissingDLC();
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~r~DLC Missing", "~w~Adam69 Callouts", $"Required DLC '{missing}' is not installed. This callout will not function properly.");
                Game.LogTrivial("Adam69 Callouts [LOG]: Required DLC '" + missing + "' is not installed. This callout will not function properly.");
                LoggingManager.Log("Adam69 Callouts [LOG]: Required DLC '" + missing + "' is not installed. This callout will not function properly.");
                return false;
            }

            spawnpoint = new(132.69f, -1308.34f, 29.03f);
            officerheading = 318.26f;
            susSpawn = new(116.04f, -1291.59f, 28.26f);
            susHeading = 246.21f;
            vehicleSpawn = new(140.00f, -1308.37f, 29.00f);
            vehicleHeading = 46.70f;
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_OFFICER_DOWN_02", spawnpoint);
            CalloutInterfaceAPI.Functions.SendMessage(this, "Officer Down Reported by an unkown civilian");
            CalloutMessage = "Officer Down Reported";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Officer Down callout has been accepted!");
            }
            else
            {
                Settings.EnableLogs = false;
            }

                Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Officer Down", "~b~Dispatch~w~: The suspect has been spotted! Respond ~r~Code3~w~.");


            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_3_Audio");

            officer = new Ped(pedsList[new Random().Next((int)pedsList.Length)], spawnpoint, 0f);
            officer.IsPersistent = true;
            officer.BlockPermanentEvents = true;
            officer.Kill();
            officer.IsValid();
            officer.Exists();

            NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(officer, "TD_PISTOL_FRONT", 1f, 1f);

            emergencyVehicle = new Vehicle(officerVehicle[new Random().Next((int)officerVehicle.Length)], vehicleSpawn, 0f);
            emergencyVehicle.IsPersistent = true;
            emergencyVehicle.IsValid();
            emergencyVehicle.Exists();

            suspect = new Ped(susSpawn);
            suspect.IsPersistent = true;
            suspect.IsValid();
            suspect.BlockPermanentEvents = true;
            suspect.Exists();

            copBlip = officer.AttachBlip();
            copBlip.Color = System.Drawing.Color.Blue;
            copBlip.IsRouteEnabled = true;
            copBlip.Exists();

            suspectBlip = suspect.AttachBlip();
            suspectBlip.Color = System.Drawing.Color.Red;
            suspectBlip.Exists();

            try
            {
                // Ensure the cop vehicle exists and is valid
                if (emergencyVehicle != null && emergencyVehicle.IsValid())
                {
                    // Turn on emergency lights
                    emergencyVehicle.IsSirenOn = true; // Activates the siren and emergency lights
                    emergencyVehicle.IsSirenSilent = true; // Keeps the siren silent while lights are active (optional)
                    emergencyVehicle.LockStatus = VehicleLockStatus.Locked; // Locks the vehicle
                }
                else
                {
                    if (Settings.EnableLogs)
                    {
                        Game.LogTrivial("Emergency Vehicle is null or invalid. Cannot enable emergency lights.");
                        LoggingManager.Log("Adam69 Callouts [LOG]: " + LogLevel.Error);
                        LoggingManager.Log("Adam69 Callouts [LOG]: " + LogLevel.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Error enabling emergency lights: " + ex.Message);
                LoggingManager.Log("Adam69 Callouts [LOG]: Error enabling emergency lights: " + ex.Message);
                LoggingManager.Log("Adam69 Callouts [LOG]: Error enabling emergency lights: " + ex.StackTrace);
                LoggingManager.Log("Adam69 Callouts [LOG]: Please report this issue on the Adam69 Callouts Discord server: https://discord.gg/N9KgZx4KUn");
            }

            officerVehicleBlip = emergencyVehicle.AttachBlip();
            officerVehicleBlip.Color = System.Drawing.Color.LightBlue;

            if (suspect.IsMale)
                malefemale = "Sir";
            else
                malefemale = "Ma'am";

            counter = 0;
            suspectStartedShooting = false; // ensure flag reset on accept

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (suspect != null && suspect.Exists()) suspect.Delete();
            if (suspectBlip != null && suspectBlip.Exists()) suspectBlip.Delete();
            if (copBlip != null && copBlip.Exists()) copBlip.Delete();
            if (officer != null && officer.Exists()) officer.Delete();
            if (officerVehicleBlip != null && officerVehicleBlip.Exists()) officerVehicleBlip.Delete();
            if (copBlip != null && copBlip.Exists()) copBlip.Delete();
            if (emergencyVehicle != null && emergencyVehicle.Exists()) emergencyVehicle.Delete();

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            if (MainPlayer.DistanceTo(officer) <= 10f)
            {
                if (Settings.HelpMessages)
                {
                    Game.DisplayHelp("Press ~y~" + Settings.Dialog.ToString() + "~w~ to Call in a officer down to ~b~dispatch~w~.");
                }
                else
                {
                    Settings.HelpMessages = false;
                    return;
                }

                // NEW: when player arrives, have suspect run into strip club and start shooting
                if (!suspectStartedShooting && suspect != null && suspect.IsValid())
                {
                    try
                    {
                        suspectStartedShooting = true;

                        // Clear current tasks and start navigation in a separate fiber so we don't block Process()
                        suspect.Tasks.Clear();

                        GameFiber.StartNew(() =>
                        {
                            try
                            {
                                // Ask the suspect to move toward the interior position
                                suspect.Tasks.FollowNavigationMeshToPosition(stripClubPosition, 2.0f, -1);

                                // Wait until suspect is close to the target or until suspect becomes invalid
                                int waitTicks = 0;
                                while (suspect != null && suspect.IsValid() && suspect.Position.DistanceTo2D(stripClubPosition) > 3f && waitTicks < 1000)
                                {
                                    GameFiber.Yield();
                                    waitTicks++;
                                }

                                if (suspect != null && suspect.IsValid())
                                {
                                    // Equip weapon and armour
                                    suspect.Inventory.GiveNewWeapon("WEAPON_COMBATPISTOL", 500, true);
                                    suspect.Armor = armorCount;

                                    // Make suspect aggressive toward the player
                                    suspect.Tasks.Clear();
                                    suspect.Tasks.FightAgainst(MainPlayer);

                                    // Request backup and play shots fired audio
                                    PolicingRedefined.API.BackupDispatchAPI.RequestPanicBackup();
                                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_ShotsFired_Audio_Remastered_01");

                                    if (Settings.EnableLogs)
                                    {
                                        Game.LogTrivial("[Adam69 Callouts LOG]: Suspect started shooting inside strip club.");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Game.LogTrivial("Adam69 Callouts [LOG]: Error in suspect navigation fiber: " + ex.Message);
                                if (Settings.EnableLogs)
                                {
                                    LoggingManager.Log("Adam69 Callouts [LOG]: Error in suspect navigation fiber: " + ex.Message);
                                    LoggingManager.Log(ex.StackTrace);
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Game.LogTrivial("Adam69 Callouts [LOG]: Error while making suspect shoot: " + ex.Message);
                        if (Settings.EnableLogs)
                        {
                            LoggingManager.Log("Adam69 Callouts [LOG]: Error while making suspect shoot: " + ex.Message);
                            LoggingManager.Log(ex.StackTrace);
                        }
                    }
                }

                if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                {
                    counter++;

                    try
                    {
                        if (counter == 1)
                        {
                            Game.DisplaySubtitle("~b~You~w~: Dispatch, we got an officer down, requesting medic but have them stage a few blocks away from the scene until the scene is secured.");
                        }
                        if (counter == 2)
                        {
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_OfficerDown_Audio_2");
                            PolicingRedefined.API.BackupDispatchAPI.RequestOfficerDownBackup();
                            PolicingRedefined.API.BackupDispatchAPI.RequestEMSCode3Backup();
                        }
                        if (counter == 3)
                        {
                            suspect.Tasks.FightAgainst(MainPlayer);
                            suspect.Inventory.GiveNewWeapon("WEAPON_COMBATPISTOL", 500, true);
                            suspect.Armor = armorCount;
                            MainPlayer.Armor = armorCount;
                        }
                        if (counter == 4)
                        {
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_ShotsFired_Audio_Remastered_01");
                            PolicingRedefined.API.BackupDispatchAPI.RequestPanicBackup();
                        }
                    }
                    catch (Exception ex)
                    {
                        Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~r~Error", "~w~Officer Down", "An error occurred, please report this on the JM Modifications Discord server.");

                        if (Settings.EnableLogs)
                        {
                            Game.LogTrivial("Adam69 Callouts [LOG]: " + ex.Message);
                            LoggingManager.Log("Adam69 Callouts [LOG]: " + ex.Message);
                            LoggingManager.Log("Adam69 Callouts [LOG]: " + ex.StackTrace);
                        }
                    }
                }

                if (MainPlayer.IsDead)
                {
                    if (Settings.MissionMessages)
                    {
                        BigMessageThread bigMessage = new BigMessageThread();
                        bigMessage.MessageInstance.ShowColoredShard("MISSION FAILED!", "You'll get 'em next time!", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                    }
                    else
                    {
                        Settings.MissionMessages = false;
                    }

                    End();
                }

                if(Game.IsKeyDown(System.Windows.Forms.Keys.End))
                {
                    End();
                }
            }

            base.Process();
        }

        public override void End()
        {
            if (officer != null && officer.Exists()) officer.WarpIntoVehicle(emergencyVehicle, -1);
            if (officer != null && officer.Exists()) officer.Dismiss();
            if (copBlip != null && copBlip.Exists()) copBlip.Delete();
            if (suspect != null && suspect.Exists()) suspect.Dismiss();
            if (suspectBlip != null && suspectBlip.Exists()) suspectBlip.Delete();
            if (emergencyVehicle != null && emergencyVehicle.Exists()) emergencyVehicle.Delete();
            if (officerVehicleBlip != null && officerVehicleBlip.Exists()) officerVehicleBlip.Delete();
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");
            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Officer Down", "~b~You~w~: We are Code4. Show me back10-8!");
            base.End();


            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();

                bigMessage.MessageInstance.ShowColoredShard("CODE4", "The scene is now secure.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }
            else
            {
                Settings.MissionMessages = false;
            }


            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Officer Down callout is code4!");
            }
            else
            {
                Settings.EnableLogs = false;
            }

        }

    }
}