using CalloutInterfaceAPI;
using Rage;
using Rage.Native;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts] Traffic Light Out", CalloutProbability.Medium, "Traffic lights at an intersection are out, causing congestion and accidents", "Code 2", "LSPD")]
    public class TrafficLightOut : Callout
    {
        private static Vector3 spawnPoint;
        private static readonly Vector3[] Intersections = new Vector3[]
        {
            new(-1153.23f, -1427.56f, 4.38f),
            new(256.67f, -1703.98f, 29.30f),
            new(1024.14f, -2500.61f, 28.04f),
            new(200.12f, -1024.24f, 29.33f),
            new(-500.41f, -675.22f, 33.24f)
        };

        private static readonly string[] VehicleModels = new string[] { "stanier", "surge", "pranger", "panto", "burrito3" };
        private static List<Vehicle> spawnedVehicles = new();
        private static List<Ped> spawnedPeds = new();
        private static List<Blip> spawnedBlips = new();
        private static bool trafficStopped = false;
        private static bool trafficOverrideEnabled = false;
        private static float savedVehicleDensity = 1f;
        private static float savedRandomVehicleDensity = 1f;
        private static float savedParkedVehicleDensity = 1f;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnPoint = LocationChooser.ChooseNearestLocation(Intersections.ToList());
            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 100f);
            CalloutInterfaceAPI.Functions.SendMessage(this, "Traffic lights are out at an intersection, multiple vehicles stuck and minor collisions reported.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("TRAFFIC_ACCIDENT_01", spawnPoint);
            CalloutMessage = "Traffic Lights Out - Intersection";
            CalloutPosition = spawnPoint;
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs) Game.LogTrivial("[Adam69 Callouts LOG]: Traffic Light Out callout accepted!");

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "Traffic Light Out", "~b~Dispatch~w~: Traffic lights out. Multiple vehicles and possible minor collisions. Respond ~r~Code 2~w~.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            // Spawn a cluster of stopped vehicles in the intersection to simulate congestion
            for (int i = 0; i < 6; i++)
            {
                Vector3 vehPos = spawnPoint.Around2D(3f + i * 1.5f, 6f + (i % 3) * 1.2f);
                var model = VehicleModels[new Random().Next(VehicleModels.Length)];
                var veh = new Vehicle(model, vehPos);
                if (veh.Exists())
                {
                    veh.IsPersistent = true;
                    veh.IsEngineOn = false;
                    veh.Heading = 0f; // Or use a calculated heading if needed
                    veh.IsStolen = false;
                    spawnedVehicles.Add(veh);

                    var blip = veh.AttachBlip();
                    blip.Color = System.Drawing.Color.Yellow;
                    blip.Alpha = 0.8f;
                    spawnedBlips.Add(blip);

                    // small chance vehicle has a driver injured/unconscious
                    if (new Random().Next(0, 10) < 3)
                    {
                        var ped = veh.CreateRandomDriver();
                        if (ped.Exists())
                        {
                            ped.IsPersistent = true;
                            ped.BlockPermanentEvents = true;
                            ped.IsInvincible = false;
                            ped.IsRagdoll = true;
                            spawnedPeds.Add(ped);
                        }
                    }
                }
            }

            // Spawn a reporting traffic officer/ped asking for help
            var officer = new Ped("s_m_y_hwaycop_01", spawnPoint.Around2D(6f, 10f), 0f);
            if (officer.Exists())
            {
                officer.IsPersistent = true;
                officer.BlockPermanentEvents = true;
                officer.Tasks.PlayAnimation(new AnimationDictionary("random@trafficwarden@base"), "base", -1f, Rage.AnimationFlags.Loop);
                spawnedPeds.Add(officer);

                var oblip = officer.AttachBlip();
                oblip.Color = System.Drawing.Color.Green;
                oblip.Scale = 0.8f;
                spawnedBlips.Add(oblip);
            }

            // Stop traffic around the intersection to allow safe scene handling
            StopTraffic();

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            foreach (var v in spawnedVehicles) if (v != null && v.Exists()) v.Delete();
            foreach (var p in spawnedPeds) if (p != null && p.Exists()) p.Delete();
            foreach (var b in spawnedBlips) if (b != null && b.Exists()) b.Delete();

            if (trafficStopped) RestoreTraffic();

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            // Apply traffic density multipliers each frame while override enabled
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
                    // ignore native failures
                }
            }

            // Provide help hint when near the officer
            var officer = spawnedPeds.FirstOrDefault(p => p != null && p.Exists());
            if (officer != null && MainPlayer.DistanceTo(officer) <= 12f)
            {
                if (Settings.HelpMessages)
                {
                    Game.DisplayHelp("Press ~y~" + Settings.Dialog.ToString() + "~w~ to speak with the traffic officer for scene info.");
                }

                if (Game.IsKeyDown(Settings.Dialog))
                {
                    GameFiber.Sleep(300);
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(officer, MainPlayer, -1);
                    Game.DisplaySubtitle("~g~Officer~w~: Traffic lights are down. Multiple cars stuck in the intersection and a few minor fender-benders. We need you to direct traffic and request tow trucks.");
                }
            }

            // Allow requesting tow truck or backup
            if (Game.IsKeyDown(Settings.RequestTowTruck))
            {
                GameFiber.Sleep(200);
                PolicingRedefined.API.BackupDispatchAPI.RequestTowServiceBackup();
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Tow_Truck_Audio");
            }

            if (Game.IsKeyDown(Settings.RequestVehicleInfo))
            {
                GameFiber.Sleep(200);
                PolicingRedefined.API.InfoDispatchAPI.RunNearestVehicleThroughDispatch(true);
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Backup_Audio");
            }

            // Player death handling
            if (MainPlayer.IsDead)
            {
                if (Settings.MissionMessages)
                {
                    BigMessageThread bigMessage = new BigMessageThread();
                    bigMessage.MessageInstance.ShowColoredShard("MISSION FAILED!", "You have fallen in the line of duty.", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                }

                if (Settings.EnableLogs)
                {
                    LoggingManager.Log("Adam69 Callouts: Traffic Light Out callout failed - Player died.");
                }

                End();
            }

            // Manual end
            if (Game.IsKeyDown(Settings.EndCall))
            {
                End();
            }

            base.Process();
        }

        public override void End()
        {
            foreach (var v in spawnedVehicles) if (v != null && v.Exists()) v.Dismiss();
            foreach (var p in spawnedPeds) if (p != null && p.Exists()) p.Dismiss();
            foreach (var b in spawnedBlips) if (b != null && b.Exists()) b.Delete();

            if (trafficStopped) RestoreTraffic();

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "Traffic Light Out", "~b~You~w~: Dispatch, we are ~g~CODE 4~w~. Show me back 10-8.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();
                bigMessage.MessageInstance.ShowColoredShard("Callout Completed!", "Intersection cleared. You are now ~g~CODE 4~w~.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: Traffic Light Out callout ended CODE 4.");
            }
        }

        private static void StopTraffic()
        {
            try
            {
                // Clear vehicles in the immediate area to reduce collisions
                NativeFunction.Natives.CLEAR_AREA_OF_VEHICLES(spawnPoint.X, spawnPoint.Y, spawnPoint.Z, Settings.TrafficStopRadius, false, false, false, false, false);

                // Save "restore" values (best-effort)
                savedVehicleDensity = Settings.TrafficRestoreMultiplier;
                savedRandomVehicleDensity = Settings.TrafficRestoreMultiplier;
                savedParkedVehicleDensity = Settings.TrafficRestoreMultiplier;

                trafficOverrideEnabled = true;
                trafficStopped = true;

                if (Settings.EnableLogs)
                {
                    Game.LogTrivial("[Adam69 Callouts LOG]: Traffic stopped around intersection for Traffic Light Out callout.");
                }
            }
            catch
            {
                // fail silently
            }
        }

        private static void RestoreTraffic()
        {
            try
            {
                trafficOverrideEnabled = false;

                NativeFunction.Natives.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(savedVehicleDensity);
                NativeFunction.Natives.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(savedRandomVehicleDensity);
                NativeFunction.Natives.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME(savedParkedVehicleDensity);

                trafficStopped = false;

                if (Settings.EnableLogs)
                {
                    Game.LogTrivial("[Adam69 Callouts LOG]: Traffic restored after Traffic Light Out callout.");
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}