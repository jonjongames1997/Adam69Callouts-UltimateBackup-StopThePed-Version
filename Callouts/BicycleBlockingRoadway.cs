using CalloutInterfaceAPI;
using Rage;
using Rage.Native;

namespace Adam69Callouts.Callouts
{

    [CalloutInterface("[Adam69 Callouts] Bicycle Blocking Roadway", CalloutProbability.Medium, "Bicycle obstructing roadway", "Code2", "LSPD")]

    public class BicycleBlockingRoadway : Callout
    {
        private static readonly string[] BicycleList = new string[] { "bmx", "cruiser", "fixter", "scorcher", "tribike", "tribike2", "tribike3" };
        private static Vehicle thebike;
        private static Blip blip;
        private static Vector3 spawnpoint;

        // Traffic control
        private static bool trafficStopped = false;
        private static bool trafficOverrideEnabled = false;

        // Saved multipliers (best-effort; game does not expose getters reliably, so we use configured restore value)
        private static float savedVehicleDensity = 1f;
        private static float savedRandomVehicleDensity = 1f;
        private static float savedParkedVehicleDensity = 1f;

        public override bool OnBeforeCalloutDisplayed()
        {
            List<Vector3> list = new()
            {
                new(-1146.43f, -1496.17f, 4.37f),
                new(898.76f, -2458.87f, 28.61f),
                new(969.30f, -1436.41f, 31.41f),
                new(-113.87f, 6417.88f, 31.44f),
                new(1694.77f, 4643.75f, 43.62f),
                new(1704.11f, 3808.54f, 35.01f),
            };
            spawnpoint = LocationChooser.ChooseNearestLocation(list);
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            CalloutInterfaceAPI.Functions.SendMessage(this, "Reports of a bicycle blocking traffic");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("Adam69Callouts_Bicycle_Blocking_Roadway", spawnpoint);
            CalloutMessage = "Abandoned bicycle left in the street";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: Bicycle Blocking Roadway callout accepted!");
            }
            else
            {
                Settings.EnableLogs = false;
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Bicycle Blocking Roadway", "~b~Dispatch~w~: The vehicle has been spotted! Respond ~r~Code2~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            thebike = new Vehicle(BicycleList[new Random().Next((int)BicycleList.Length)], spawnpoint, 0f);
            thebike.IsValid();
            thebike.IsPersistent = true;
            thebike.Exists();

            thebike.IsStolen = false;

            blip = thebike.AttachBlip();
            blip.Color = System.Drawing.Color.Yellow;
            blip.Alpha = 0.8f;
            blip.IsRouteEnabled = true;
            blip.Exists();

            // Stop traffic around the spawn point
            StopTraffic();

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {

            if (thebike.Exists()) thebike.Delete();
            if (blip.Exists()) blip.Delete();

            // Ensure traffic restored if callout not accepted
            if (trafficStopped) RestoreTraffic();

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            // While traffic override is enabled, apply configured multipliers each frame (these natives are per-frame overrides)
            if (trafficOverrideEnabled)
            {
                try
                {
                    float target = Settings.TrafficDensityMultiplier;
                    NativeFunction.Natives.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(target);
                    NativeFunction.Natives.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(target);
                    NativeFunction.Natives.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(target);
                }
                catch
                {
                    // ignore any native call failures
                }
            }

            if (Game.IsKeyDown(System.Windows.Forms.Keys.L))
            {
                PolicingRedefined.API.BackupDispatchAPI.RequestTowServiceBackup();
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Tow_Truck_Audio");
            }

            if (MainPlayer.DistanceTo(thebike) <= 10f)
            {
                if (Settings.HelpMessages)
                {
                    Game.DisplayHelp("Deal with the situation as you see fit.", 5000);
                }
                else
                {
                    Settings.HelpMessages = false;
                }
            }

            if (MainPlayer.IsDead)
            {
                if (Settings.MissionMessages)
                {
                    BigMessageThread bigMessage = new BigMessageThread();

                    bigMessage.MessageInstance.ShowColoredShard("Callout Failed!", "You'll get 'em next time.", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                }
                else
                {
                    Settings.MissionMessages = false;
                }

                End();

                if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
                {
                    if (Settings.MissionMessages)
                    {
                        BigMessageThread bigMessage = new BigMessageThread();

                        bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "You are now ~g~CODE4~w~.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
                    }
                    else
                    {
                        Settings.MissionMessages = false;
                    }

                        End();
                }
            }

            base.Process();
        }

        public override void End()
        {
            if (thebike) thebike.Dismiss();
            if (blip) blip.Delete();
            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Vehicle Blocking Crosswalk", "~b~You~w~: Dispatch, we are ~g~CODE4~w~. Show me back 10-8.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

            // Restore traffic when the callout ends
            if (trafficStopped) RestoreTraffic();

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: Bicycle Blocking Roadway callout is CODE4!");
            }
            else
            {
                Settings.EnableLogs = false;
            }
        }

        private static void StopTraffic()
        {
            try
            {
                // Immediately clear nearby vehicles to reduce collisions
                NativeFunction.Natives.CLEAR_AREA_OF_VEHICLES(spawnpoint.X, spawnpoint.Y, spawnpoint.Z, Settings.TrafficStopRadius, false, false, false, false, false);

                // Save current multipliers (best effort). The game does not provide reliable GET natives for these, so use configured restore value.
                savedVehicleDensity = Settings.TrafficRestoreMultiplier;
                savedRandomVehicleDensity = Settings.TrafficRestoreMultiplier;
                savedParkedVehicleDensity = Settings.TrafficRestoreMultiplier;

                // Enable per-frame override in Process()
                trafficOverrideEnabled = true;
                trafficStopped = true;

                if (Settings.EnableLogs)
                {
                    Game.LogTrivial("[Adam69 Callouts LOG]: Traffic stopped around bicycle spawnpoint.");
                }
            }
            catch
            {
                // Fail silently; not critical
            }
        }

        private static void RestoreTraffic()
        {
            try
            {
                // Disable per-frame override so game returns to normal behavior
                trafficOverrideEnabled = false;

                // Apply saved multipliers for the current frame to reduce visual glitches (best-effort)
                NativeFunction.Natives.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(savedVehicleDensity);
                NativeFunction.Natives.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(savedRandomVehicleDensity);
                NativeFunction.Natives.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(savedParkedVehicleDensity);

                trafficStopped = false;

                if (Settings.EnableLogs)
                {
                    Game.LogTrivial("[Adam69 Callouts LOG]: Traffic restored after bicycle callout.");
                }
            }
            catch
            {
                // Fail silently
            }
        }

    }
}
