using CalloutInterfaceAPI;
using System;
using System.Drawing;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts] Disabled Vehicle", CalloutProbability.Medium, "Disabled vehicle blocking traffic", "Code 2", "LEO")]
    public class DisabledVehicle : Callout
    {
        private static readonly Random rng = new Random();

        private static readonly string[] vehicleModels = new string[]
        {
            "blista", "dilettante", "panto", "prairie", "asea", "ingot", "intruder",
            "minivan", "minivan2", "rumpo", "rumpo2", "journey", "youga", "youga2"
        };

        private static readonly string[] breakdownReasons = new string[]
        {
            "Engine failure - car won't start",
            "Flat tire - driver needs assistance",
            "Out of gas - blocking the roadway",
            "Overheating - smoke from engine",
            "Transmission problems - stuck in gear"
        };

        private Vector3 spawnpoint;
        private Vehicle disabledVehicle;
        private Ped driver;
        private Blip vehicleBlip;
        private Blip driverBlip;
        private bool hasSpokenToDriver;
        private bool hasClearedScene;
        private string breakdownReason;
        private int conversationCounter;
        private bool isAngry;
        private bool hasHazardLights;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = World.GetNextPositionOnStreet(MainPlayer.Position.Around2D(200f, 400f));

            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            CalloutInterfaceAPI.Functions.SendMessage(this, "Disabled vehicle blocking traffic. Traffic control needed.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_SUSPICIOUS_VEHICLE IN_OR_ON_POSITION", spawnpoint);
            CalloutMessage = "Disabled Vehicle";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Disabled Vehicle callout has been accepted!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Disabled Vehicle callout has been accepted!");
            }

            breakdownReason = breakdownReasons[rng.Next(breakdownReasons.Length)];

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Disabled Vehicle", $"~b~Dispatch~w~: {breakdownReason}. Respond ~r~Code 2~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            // Create disabled vehicle
            string model = vehicleModels[rng.Next(vehicleModels.Length)];
            float heading = World.GetStreetHash(spawnpoint);

            disabledVehicle = new Vehicle(model, spawnpoint, heading)
            {
                IsPersistent = true,
                IsEngineOn = false
            };

            // Apply breakdown-specific damage
            if (breakdownReason.Contains("Overheating"))
            {
                disabledVehicle.EngineHealth = 100f;
            }
            else if (breakdownReason.Contains("Flat tire"))
            {
                NativeFunction.Natives.SET_VEHICLE_TYRE_BURST(disabledVehicle, rng.Next(4), true, 1000f);
            }

            // Create driver
            driver = disabledVehicle.CreateRandomDriver();
            driver.IsPersistent = true;
            driver.BlockPermanentEvents = true;

            // Random chance driver is frustrated/angry
            isAngry = rng.Next(3) == 0;

            // Driver behavior
            GameFiber.Sleep(500);
            driver.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(3000);

            if (isAngry)
            {
                // Angry driver on phone yelling
                GameFiber.Sleep(1000);
                driver.Tasks.PlayAnimation(new AnimationDictionary("cellphone@"), "cellphone_call_listen_base", -1f, AnimationFlags.Loop);
            }
            else
            {
                // Calm driver looking at engine/tire
                GameFiber.Sleep(1000);
                driver.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", -1f, AnimationFlags.Loop);
            }

            // Enable hazard lights randomly
            hasHazardLights = rng.Next(2) == 0;
            if (hasHazardLights)
            {
                NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(disabledVehicle, 0, true);
                NativeFunction.Natives.SET_VEHICLE_INDICATOR_LIGHTS(disabledVehicle, 1, true);
            }

            // Create blips
            vehicleBlip = disabledVehicle.AttachBlip();
            vehicleBlip.Color = Color.Yellow;
            vehicleBlip.IsRouteEnabled = true;

            driverBlip = driver.AttachBlip();
            driverBlip.Color = Color.Orange;
            driverBlip.Alpha = 0.5f;

            hasSpokenToDriver = false;
            hasClearedScene = false;
            conversationCounter = 0;

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            SafeDelete(disabledVehicle);
            SafeDelete(driver);
            SafeDeleteBlip(vehicleBlip);
            SafeDeleteBlip(driverBlip);

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            try
            {
                // Speak with driver
                if (MainPlayer.DistanceTo(driver) <= 4f && conversationCounter < 7)
                {
                    if (Settings.HelpMessages && conversationCounter == 0)
                    {
                        Game.DisplayHelp("Press ~y~Y~w~ to speak with the driver.");
                    }

                    if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                    {
                        conversationCounter++;
                        HandleConversation();
                    }
                }

                // Check if player is dead
                if (MainPlayer.IsDead)
                {
                    if (Settings.MissionMessages)
                    {
                        BigMessageThread bigMessage = new BigMessageThread();
                        bigMessage.MessageInstance.ShowColoredShard("Callout Failed!", "You are now ~r~CODE 4~w~.", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                    }
                    End();
                    return;
                }

                // End call key
                if (Game.IsKeyDown(Settings.EndCall))
                {
                    if (Settings.MissionMessages)
                    {
                        BigMessageThread bigMessage = new BigMessageThread();
                        bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "Scene cleared. You are now ~r~CODE 4~w~.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
                    }
                    End();
                    return;
                }
            }
            catch (Exception ex)
            {
                if (Settings.EnableLogs)
                {
                    Game.LogTrivial("Adam69 Callouts [LOG]: Error in Disabled Vehicle callout. Error: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts [LOG]: ERROR: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts [LOG]: ERROR: " + ex.StackTrace);
                }
            }

            base.Process();
        }

        private void HandleConversation()
        {
            if (driver == null || !driver.Exists()) return;

            string gender = driver.IsMale ? "Sir" : "Ma'am";

            switch (conversationCounter)
            {
                case 1:
                    driver.Tasks.Clear();
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(driver, MainPlayer, -1);
                    Game.DisplaySubtitle($"~b~You~w~: Hello {gender}, having car trouble?");
                    GameFiber.Wait(1000);
                    break;
                case 2:
                    if (isAngry)
                    {
                        driver.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                        Game.DisplaySubtitle("~y~Driver~w~: YES! This piece of junk broke down in the middle of traffic! I'm going to be late for work!");
                    }
                    else
                    {
                        driver.Tasks.PlayAnimation(new AnimationDictionary("gestures@m@standing@casual"), "gesture_shrug_hard", -1f, AnimationFlags.None);
                        Game.DisplaySubtitle($"~y~Driver~w~: Yeah officer, {breakdownReason.ToLower()}. I don't know what happened.");
                    }
                    GameFiber.Wait(1000);
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Do you have roadside assistance or need me to call a tow truck?");
                    GameFiber.Wait(1000);
                    break;
                case 4:
                    driver.Tasks.PlayAnimation(new AnimationDictionary("cellphone@"), "cellphone_call_listen_base", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~y~Driver~w~: I already called AAA, they said 30-45 minutes. Can you believe that?");
                    GameFiber.Wait(1000);
                    break;
                case 5:
                    Game.DisplaySubtitle("~b~You~w~: I'll help direct traffic around your vehicle until the tow arrives. Can I see your license and registration?");
                    GameFiber.Wait(1000);
                    break;
                case 6:
                    driver.Tasks.Clear();
                    driver.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_a", -1f, AnimationFlags.None);
                    Game.DisplaySubtitle("~y~Driver~w~: Here you go, officer. Thank you for helping out.");
                    GameFiber.Wait(1000);
                    break;
                case 7:
                    driver.Tasks.PlayAnimation(new AnimationDictionary("rcmjosh1"), "idle", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~w~Everything checks out. You may clear the scene when the tow truck arrives.");
                    hasSpokenToDriver = true;
                    GameFiber.Wait(1000);
                    break;
            }
        }

        public override void End()
        {
            SafeDelete(disabledVehicle);
            SafeDismiss(driver);
            SafeDeleteBlip(vehicleBlip);
            SafeDeleteBlip(driverBlip);

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Disabled Vehicle", "~b~You~w~: Dispatch, tow truck en route. Scene cleared. We are ~g~Code 4~w~.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();
                bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "Return to patrol!", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Disabled Vehicle callout is code 4!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Disabled Vehicle callout is code 4!");
            }
        }

        #region Helpers
        private void SafeDelete(Vehicle veh)
        {
            if (veh != null && veh.Exists()) veh.Delete();
        }

        private void SafeDelete(Ped ped)
        {
            if (ped != null && ped.Exists()) ped.Delete();
        }

        private void SafeDismiss(Ped ped)
        {
            if (ped != null && ped.Exists()) ped.Dismiss();
        }

        private void SafeDeleteBlip(Blip blip)
        {
            if (blip != null && blip.Exists()) blip.Delete();
        }
        #endregion
    }
}