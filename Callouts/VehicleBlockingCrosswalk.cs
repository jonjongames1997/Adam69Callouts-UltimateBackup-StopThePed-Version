using CalloutInterfaceAPI;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts] Vehicle Blocking Crosswalk", CalloutProbability.Medium, "Citizen's reporting a vehicle blocking crosswalk", "CODE 2", "LSCSO")]
    public class VehicleBlockingCrosswalk : Callout
    {
        private Vehicle motorVehicle;
        private Vector3 spawnpoint;
        private Blip vehBlip;

        /// <summary>
        /// Called before the callout is displayed to the player.
        /// </summary>
        /// <returns>True if the callout should be displayed, otherwise false.</returns>
        public override bool OnBeforeCalloutDisplayed()
        {
            var list = new List<Vector3>
            {
                new(103.17f, -1344.18f, 29.04f),
                new(-752.53f, -1118.11f, 10.27f),
                new(-657.13f, 280.97f, 80.86f),
                new(-103.97f, 239.40f, 97.87f),
                new(-454.79f, -260.03f, 3596f),
                new(-1310.03f, -70.61f, 47.95f),
                new(-2155.47f, -341.27f, 13.21f),
            };
            spawnpoint = LocationChooser.ChooseNearestLocation(list);
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("Adam69Callouts_Vehicle_Obstruction_Crime_01", spawnpoint);
            CalloutInterfaceAPI.Functions.SendMessage(this, "A vehicle blocking crosswalk");
            CalloutMessage = "Vehicle Blocking Crosswalk Reported";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Vehicle Blocking Crosswalk callout has been accepted!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Vehicle Blocking Crosswalk callout has been accepted!");
            }
            else
            {
                Settings.EnableLogs = false;
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Vehicle Blocking Crosswalk", "~b~Dispatch~w~: The vehicle has been located. Respond ~y~Code 2~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            motorVehicle = new Vehicle(spawnpoint)
            {
                IsPersistent = true,
                IsStolen = false
            };

            vehBlip = motorVehicle.AttachBlip();
            vehBlip.Color = System.Drawing.Color.BurlyWood;
            vehBlip.IsRouteEnabled = true;

            return base.OnCalloutAccepted();
        }

        /// <summary>
        /// Called when the callout is not accepted by the player.
        /// </summary>
        public override void OnCalloutNotAccepted()
        {
            if (motorVehicle.Exists()) motorVehicle.Delete();
            if (vehBlip.Exists()) vehBlip.Delete();

            base.OnCalloutNotAccepted();
        }

        /// <summary>
        /// Called every frame while the callout is active.
        /// </summary>
        public override void Process()
        {
            if (Game.IsKeyDown(System.Windows.Forms.Keys.P))
            {
                PolicingRedefined.API.VehicleAPI.RunVehicleThroughDispatch(motorVehicle, true, true, true);
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69_Callouts_Request_Vehicle_Info_Audio");
                LoggingManager.Log("Adam69 Callouts [LOG]: Player has requested vehicle information.");
                Game.LogTrivial("Adam69 Callouts [LOG]: Player has requested vehicle information.");
            }

            if (MainPlayer.DistanceTo(motorVehicle) <= 10f)
            {
                Game.DisplaySubtitle("Check the vehicle record, search the vehicle (If you have probable cause), then tow the vehicle.", 5000);
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

            base.Process();
        }

        /// <summary>
        /// Called when the callout ends.
        /// </summary>
        public override void End()
        {
            if (motorVehicle.Exists()) motorVehicle.Delete();
            if (vehBlip.Exists()) vehBlip.Delete();
            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Vehicle Blocking Crosswalk", "~b~You~w~: Dispatch, we are ~g~CODE 4~w~. Show me back 10-8.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();
                bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "You are now ~g~CODE 4~w~.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }
            else
            {
                Settings.MissionMessages = false;
            }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Vehicle Blocking Crosswalk callout is Code 4!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Vehicle Blocking Crosswalk callout is Code 4!");
            }
            else
            {
                Settings.EnableLogs = false;
            }
        }
    }
}