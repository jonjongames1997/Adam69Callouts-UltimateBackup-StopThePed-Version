using CalloutInterfaceAPI;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts]: Vehicle Blocking Sidewalk", CalloutProbability.Medium, "Reports of a vehicle blocking the sidewalk", "CODE 1", "LSPD")]
    public class VehicleBlockingSidewalk : Callout
    {
        private static Vehicle motorVehicle;
        private static Vector3 spawnpoint;
        private static Blip vehBlip;

        public override bool OnBeforeCalloutDisplayed()
        {
            var list = new List<Vector3>
            {
                new(-835.34f, -1137.58f, 7.29f),
                new(-1273.35f, -639.09f, 26.55f),
                new(-970.77f, -134.53f, 37.70f),
                new(295.24f, 180.78f, 103.77f),
                new(-452.79f, -265.15f, 35.87f),
            };
            spawnpoint = LocationChooser.ChooseNearestLocation(list);
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_ROAD_BLOCKADE_01", spawnpoint);
            CalloutInterfaceAPI.Functions.SendMessage(this, "A citizen reporting a vehicle blocking sidewalk.");
            CalloutMessage = "Vehicle Blocking Sidewalk Reported";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Vehicle Blocking Sidewalk callout has been accepted!");
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Vehicle Blocking Sidewalk", "~b~Dispatch~w~: Vehicle has been located. Respond ~g~Code 1~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_1_Audio");

            motorVehicle = new Vehicle(spawnpoint)
            {
                IsPersistent = true
            };
            motorVehicle.Exists();

            vehBlip = motorVehicle.AttachBlip();
            vehBlip.Color = System.Drawing.Color.AliceBlue;
            vehBlip.IsRouteEnabled = true;
            vehBlip.Exists();

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (motorVehicle != null && motorVehicle.Exists()) motorVehicle.Delete();
            if (vehBlip != null && vehBlip.Exists()) vehBlip.Delete();

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (Game.IsKeyDown(System.Windows.Forms.Keys.P))
            {
                if (motorVehicle != null && motorVehicle.Exists())
                {
                    PolicingRedefined.API.VehicleAPI.RunVehicleThroughDispatch(motorVehicle, true, true, true);
                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69_Callouts_Request_Vehicle_Info_Audio");
                    LoggingManager.Log("Adam69 Callouts - Vehicle Blocking Sidewalk [LOG]: Player has requested vehicle information.");
                    Game.LogTrivial("Adam69 Callouts - Vehicle Blocking Sidewalk [LOG]: Player has requested vehicle information.");
                }
            }

            if (motorVehicle != null && motorVehicle.Exists() && MainPlayer.DistanceTo(motorVehicle) <= 10f)
            {
                Game.DisplaySubtitle("Investigate the Vehicle, check vehicle record, then call tow truck", 5000);
            }

            if (MainPlayer.IsDead)
            {
                if (Settings.MissionMessages)
                {
                    BigMessageThread bigMessage = new BigMessageThread();
                    bigMessage.MessageInstance.ShowColoredShard("Callout Failed!", "You are now ~r~CODE 4~w~.", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                }
                else
                {
                    Settings.MissionMessages = false;
                }

                End();
            }

            if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
            {
               End();
            }
        }

        public override void End()
        {
            if (motorVehicle != null && motorVehicle.Exists()) motorVehicle.Delete();
            if (vehBlip != null && vehBlip.Exists()) vehBlip.Delete();
            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Vehicle Blocking Sidewalk", "~b~You~w~: Dispatch, we are ~g~Code 4~w~. Show me back 10-8.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();

                bigMessage.MessageInstance.ShowColoredShard("Callout Completed!", "You are now ~g~CODE 4~w~.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }
            else
            {
                Settings.MissionMessages = false;
            }

            base.End();

            if(Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Vehicle Blocking Sidewalk callout has ended!");
            }
        }
    }
}