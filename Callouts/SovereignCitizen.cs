using CalloutInterfaceAPI;
using Adam69Callouts.Common;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using StopThePed;
using UltimateBackup;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts] Sovereign Citizen", CalloutProbability.Medium, "Sovereign citizen causing a disturbance — traffic stop refusal, paperwork showdowns, or barricade scenarios", "Code 2", "LSPD")]
    public class SovereignCitizen : Callout
    {
        private enum ScenarioType { TrafficStop, CheckpointRefusal, ProtestBlocking, ArmedRefusal }
        private static ScenarioType scenario;
        private static Ped suspect;
        private static Ped passenger;
        private static Vehicle suspectVehicle;
        private static Blip suspectBlip;
        private static Vector3 spawnPoint;
        private static int dialogStage;
        private static bool suspectArmed;
        private static bool suspectAggressive;
        private static readonly string[] vehiclePool = new string[] { "sentinel", "regina", "felon", "cog55", "stafford" };
        private static readonly string[] pedPool = new string[] { "s_m_m_prisoner_01", "g_m_m_chigoon_02", "a_m_m_farmer_01", "a_f_y_tourist_01" };
        private static readonly string[] weaponPool = new string[] { "weapon_pistol", "weapon_sawnoffshotgun", "weapon_microsmg" };

        public override bool OnBeforeCalloutDisplayed()
        {
            // Choose a semi-urban street or intersection near player
            spawnPoint = World.GetNextPositionOnStreet(MainPlayer.Position.Around2D(250f, 700f));
            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 120f);

            // Random scenario selection
            var r = new Random();
            scenario = (ScenarioType)r.Next(0, Enum.GetNames(typeof(ScenarioType)).Length);

            CalloutInterfaceAPI.Functions.SendMessage(this, "Sovereign Citizen report: refusal to comply with lawful orders or obstruction.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("SUSPICIOUS_PERSON_02", spawnPoint);
            CalloutMessage = "Sovereign Citizen Disturbance";
            CalloutPosition = spawnPoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: Sovereign Citizen callout accepted!");
                LoggingManager.Log("Adam69 Callouts: Sovereign Citizen accepted.");
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "Sovereign Citizen", "~b~Dispatch~w~: Reports of a Sovereign Citizen. Expect refusal/uncooperative behaviour. Respond ~r~Code 2~w~.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            var r = new Random();

            // Randomize suspect attributes
            suspectArmed = r.Next(0, 10) > 6; // ~30% armed
            suspectAggressive = r.Next(0, 10) > 7; // ~20% aggressive

            // Spawn a vehicle + suspect for traffic-related scenarios; otherwise spawn suspect on foot
            if (scenario == ScenarioType.TrafficStop || scenario == ScenarioType.CheckpointRefusal || scenario == ScenarioType.ArmedRefusal)
            {
                string vehModel = vehiclePool[r.Next(vehiclePool.Length)];
                suspectVehicle = new Vehicle(vehModel, spawnPoint);
                if (suspectVehicle.Exists())
                {
                    suspectVehicle.IsPersistent = true;
                    suspectVehicle.IsEngineOn = false;
                }

                suspect = suspectVehicle.CreateRandomDriver();

                if (r.Next(0, 10) > 6)
                {
                    int? seatIndex = suspectVehicle.GetFreePassengerSeatIndex();
                    if (seatIndex.HasValue)
                    {
                        passenger = new Ped(pedPool[r.Next(pedPool.Length)], suspectVehicle.GetOffsetPositionFront(2f), 0f);
                        if (passenger.Exists())
                        {
                            passenger.WarpIntoVehicle(suspectVehicle, seatIndex.Value);
                        }
                    }
                    else
                    {
                        passenger = null;
                    }
                }
                else
                {
                    passenger = null;
                }   
            }
            else // ProtestBlocking
            {
                // spawn suspect on foot and a couple of associates to block the road
                suspect = new Ped(pedPool[r.Next(pedPool.Length)], spawnPoint.Around2D(1f, 6f), 0f);
                passenger = new Ped(pedPool[r.Next(pedPool.Length)], spawnPoint.Around2D(1f, 6f), 0f);
                if (passenger.Exists())
                {
                    passenger.IsPersistent = true;
                    passenger.BlockPermanentEvents = true;
                    passenger.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_protest_sign@male_a"), "base", -1f, AnimationFlags.Loop);
                }
            }

            // Apply common setup
            if (suspect != null && suspect.Exists())
            {
                suspect.IsPersistent = true;
                suspect.BlockPermanentEvents = true;

                // Give weapon occasionally
                if (suspectArmed)
                {
                    SafeInventory.SafeGiveWeapon(suspect, weaponPool[r.Next(weaponPool.Length)], 120, true);
                }

                // Add blip and route
                suspectBlip = suspect.AttachBlip();
                suspectBlip.Color = System.Drawing.Color.Orange;
                suspectBlip.IsRouteEnabled = true;

                // Position-specific setup
                switch (scenario)
                {
                    case ScenarioType.TrafficStop:
                        // suspect sitting in vehicle simulating refusing to show documents
                        if (suspectVehicle != null && suspectVehicle.Exists())
                        {
                            suspect.Tasks.PlayAnimation(new AnimationDictionary("random@arrests"), "idle_a", -1f, AnimationFlags.Loop);
                        }
                        break;
                    case ScenarioType.CheckpointRefusal:
                        // suspect blocking a temporary checkpoint - spawn a protestor or sign-holding ped
                        if (passenger != null && passenger.Exists())
                        {
                            passenger.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_drug_dealer_hard@male@base"), "base", -1f, AnimationFlags.Loop);
                        }
                        break;
                    case ScenarioType.ProtestBlocking:
                        // both suspect and passenger are animated above
                        break;
                    case ScenarioType.ArmedRefusal:
                        if (suspectArmed)
                        {
                            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~r~ALERT", "Possible Armed Refusal", "~b~Use caution — suspect may be armed.");
                        }
                        break;
                }
            }

            dialogStage = 0;
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (suspect != null && suspect.Exists()) suspect.Delete();
            if (passenger != null && passenger.Exists()) passenger.Delete();
            if (suspectVehicle != null && suspectVehicle.Exists()) suspectVehicle.Delete();
            if (suspectBlip != null && suspectBlip.Exists()) suspectBlip.Delete();

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            base.Process();

            try
            {
                if (suspect == null || !suspect.Exists())
                {
                    // Nothing to handle
                    return;
                }

                // Provide help hint when close
                if (MainPlayer.DistanceTo(suspect) <= 12f && dialogStage == 0)
                {
                    if (Settings.HelpMessages)
                    {
                        Game.DisplayHelp("Press ~y~" + Settings.Dialog.ToString() + "~w~ to approach and begin lawful contact. Press ~y~" + Settings.RequestVehicleInfo.ToString() + "~w~ to request vehicle info.");
                    }

                    if (Game.IsKeyDown(Settings.Dialog))
                    {
                        GameFiber.Sleep(300);
                        NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                        Game.DisplaySubtitle("~b~You~w~: Good afternoon. I'm stopping you to confirm identification and registration. Please step out and provide documents.");
                        GameFiber.Sleep(1800);

                        dialogStage = 1;
                    }
                }

                // Interaction while dialog stage 1
                if (dialogStage == 1 && MainPlayer.DistanceTo(suspect) <= 10f)
                {
                    if (Game.IsKeyDown(Settings.Dialog))
                    {
                        GameFiber.Sleep(250);
                        dialogStage = 2;

                        // Show suspect refusal text with randomized outcomes
                        if (new Random().Next(0, 10) > 5) // refusal half the time
                        {
                            Game.DisplaySubtitle("~r~Suspect~w~: I am a sovereign citizen. You have no jurisdiction. I refuse to provide documents.");
                            GameFiber.Sleep(1800);

                            // escalate
                            if (suspectAggressive)
                            {
                                Game.DisplaySubtitle("~r~Suspect~w~: I will not comply!");
                                GameFiber.Sleep(1000);
                                if (suspectArmed)
                                {
                                    // Hostile armed — fight or flee
                                    suspect.Tasks.FightAgainst(MainPlayer);
                                    suspect.Armor = 300;
                                    UltimateBackup.API.Functions.callPanicButtonBackup(true);
                                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("OFFICERS_UNDER_FIRE");
                                }
                                else
                                {
                                    // Non-armed push/pull and possible flight
                                    suspect.Tasks.ReactAndFlee(MainPlayer);
                                    if (suspectBlip != null && suspectBlip.Exists()) suspectBlip.Color = System.Drawing.Color.Red;
                                }
                            }
                            else
                            {
                                // passive refusal - request backup or K9
                                Game.DisplaySubtitle("~b~You~w~: Sir/Ma'am, failure to comply may result in arrest. Please step out.");
                                GameFiber.Sleep(1500);

                                if (Settings.HelpMessages)
                                {
                                    Game.DisplayHelp("Press ~y~" + Settings.RequestVehicleInfo.ToString() + "~w~ to request backup or ~y~" + Settings.RequestTowTruck.ToString() + "~w~ to request tow/k9 (if needed).");
                                }
                            }
                        }
                        else
                        {
                            // Compliant: suspect provides documents
                            Game.DisplaySubtitle("~r~Suspect~w~: Fine. Here are my documents.");
                            GameFiber.Sleep(1300);
                            Game.DisplaySubtitle("~b~You~w~: Thank you. Everything seems in order. You are free to go.");
                        }
                    }
                }

                // Allow the player to request vehicle info / backup
                if (Game.IsKeyDown(Settings.RequestVehicleInfo))
                {
                    GameFiber.Sleep(300);
                    StopThePed.API.Functions.getVehicleRegistrationStatus(suspectVehicle);
                    StopThePed.API.Functions.getVehicleInsuranceStatus(suspectVehicle);
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Backup_Audio");
                    Game.DisplaySubtitle("~b~You~w~: Dispatch, requesting backup.");
                }

                if (Game.IsKeyDown(Settings.RequestTowTruck))
                {
                    GameFiber.Sleep(300);
                    StopThePed.API.Functions.callTowService();
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Tow_Truck_Audio");
                    Game.DisplaySubtitle("~b~You~w~: Dispatch, request tow/K9 as needed.");
                }

                if (suspectVehicle != null && suspectVehicle.Exists() && suspect.IsInVehicle(suspectVehicle, false))
                {
                    if (suspectVehicle.Speed > 5f && MainPlayer.DistanceTo(suspectVehicle) > 25f)
                    {
                        var pursuit = LSPD_First_Response.Mod.API.Functions.CreatePursuit();
                        LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(pursuit, suspect);
                        LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                        Game.DisplayNotification("~r~Suspect fleeing! Pursue and attempt to stop the vehicle!");
                        if (suspectBlip != null && suspectBlip.Exists()) suspectBlip.Color = System.Drawing.Color.Red;
                    }
                }

                if (suspect != null && suspect.Exists())
                {
                    if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(suspect) || LSPD_First_Response.Mod.API.Functions.IsPedStoppedByPlayer(suspect))
                    {
                        Game.DisplayHelp("Suspect detained. Press ~y~" + Settings.EndCall.ToString() + "~w~ to end the callout.");
                    }

                    if (suspect.IsDead)
                    {
                        Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~r~Sovereign Citizen", "~r~Suspect is deceased. Notify dispatch.");
                    }
                }

                if (MainPlayer.IsDead)
                {
                    if (Settings.MissionMessages)
                    {
                        BigMessageThread bigMessage = new BigMessageThread();
                        bigMessage.MessageInstance.ShowColoredShard("MISSION FAILED!", "You have fallen in the line of duty.", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                    }

                    if (Settings.EnableLogs) LoggingManager.Log("Adam69 Callouts: Sovereign Citizen callout failed - Player died.");

                    End();
                }

                if (Game.IsKeyDown(Settings.EndCall))
                {
                    End();
                }
            }
            catch (Exception ex)
            {
                if (Settings.EnableLogs)
                {
                    Game.LogTrivial("[Adam69 Callouts ERROR]: SovereignCitizen.Process Exception: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts: SovereignCitizen.Process Exception: " + ex.Message);
                }
            }
        }

        public override void End()
        {
            try
            {
                if (suspect != null && suspect.Exists()) suspect.Dismiss();
                if (passenger != null && passenger.Exists()) passenger.Dismiss();
                if (suspectVehicle != null && suspectVehicle.Exists()) suspectVehicle.Dismiss();
                if (suspectBlip != null && suspectBlip.Exists()) suspectBlip.Delete();

                Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "Sovereign Citizen", "~b~You~w~: Dispatch, we are ~g~CODE 4~w~. Show me back 10-8.");
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

                if (Settings.MissionMessages)
                {
                    BigMessageThread bigMessage = new BigMessageThread();
                    bigMessage.MessageInstance.ShowColoredShard("Callout Completed!", "Situation resolved. You are now ~g~CODE 4~w~.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
                }
            }
            catch {  }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: Sovereign Citizen callout ended CODE 4.");
                LoggingManager.Log("Adam69 Callouts: Sovereign Citizen callout ended CODE 4.");
            }
        }
    }
}