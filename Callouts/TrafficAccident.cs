using CalloutInterfaceAPI;
using System;
using System.Drawing;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts] Traffic Accident", CalloutProbability.High, "Motor vehicle accident reported", "Code 3", "LEO")]
    public class TrafficAccident : Callout
    {
        private static readonly Random rng = new Random();

        private static readonly string[] vehicleModels = new string[]
        {
            "blista", "dilettante", "panto", "prairie", "rhapsody", "asea", "asterope",
            "cog55", "cognoscenti", "emperor", "fugitive", "ingot", "intruder", "premier",
            "primo", "primo2", "regina", "stanier", "stratum", "stretch", "superd", "surge",
            "tailgater", "warrener", "washington", "oracle", "oracle2", "fusilade", "penumbra"
        };

        private static readonly string[] injuryTypes = new string[]
        {
            "No injuries reported",
            "Minor injuries - requesting EMS",
            "Moderate injuries - EMS en route",
            "Serious injuries - requesting paramedics Code 3"
        };

        private Vector3 spawnpoint;
        private Vector3 vehicle1Pos;
        private Vector3 vehicle2Pos;
        private Vehicle vehicle1;
        private Vehicle vehicle2;
        private Ped driver1;
        private Ped driver2;
        private Blip accidentBlip;
        private Blip vehicle1Blip;
        private Blip vehicle2Blip;
        private bool hasInvestigated;
        private bool hasClearedScene;
        private string injuryStatus;
        private int accidentType; // 0 = rear-end, 1 = T-bone, 2 = head-on
        private int conversationCounter;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = World.GetNextPositionOnStreet(MainPlayer.Position.Around2D(300f, 500f));

            vehicle1Pos = spawnpoint;
            accidentType = rng.Next(3);

            // Position vehicles based on accident type
            switch (accidentType)
            {
                case 0: // Rear-end
                    vehicle2Pos = spawnpoint + (MathHelper.ConvertHeadingToDirection(World.GetStreetHash(spawnpoint)) * 5f);
                    break;
                case 1: // T-bone
                    vehicle2Pos = spawnpoint + (MathHelper.ConvertHeadingToDirection(World.GetStreetHash(spawnpoint) + 90f) * 3f);
                    break;
                case 2: // Head-on
                    vehicle2Pos = spawnpoint + (MathHelper.ConvertHeadingToDirection(World.GetStreetHash(spawnpoint) + 180f) * 4f);
                    break;
            }

            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            CalloutInterfaceAPI.Functions.SendMessage(this, "Traffic accident reported. Units respond Code 3.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_TRAFFIC_COLLISION IN_OR_ON_POSITION", spawnpoint);
            CalloutMessage = "Traffic Accident";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Traffic Accident callout has been accepted!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Traffic Accident callout has been accepted!");
            }

            injuryStatus = injuryTypes[rng.Next(injuryTypes.Length)];

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Traffic Accident", $"~b~Dispatch~w~: {injuryStatus}. Respond ~r~Code 3~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("CRIME_TRAFFIC_COLLISION");

            // Create vehicles
            string model1 = vehicleModels[rng.Next(vehicleModels.Length)];
            string model2 = vehicleModels[rng.Next(vehicleModels.Length)];

            float heading = World.GetStreetHash(spawnpoint);

            vehicle1 = new Vehicle(model1, vehicle1Pos, heading)
            {
                IsPersistent = true
            };

            vehicle2 = new Vehicle(model2, vehicle2Pos, heading + (accidentType == 1 ? 90f : (accidentType == 2 ? 180f : 0f)))
            {
                IsPersistent = true
            };

            // Add damage to vehicles
            ApplyAccidentDamage(vehicle1);
            ApplyAccidentDamage(vehicle2);

            // Create drivers
            driver1 = vehicle1.CreateRandomDriver();
            driver1.IsPersistent = true;
            driver1.BlockPermanentEvents = true;

            driver2 = vehicle2.CreateRandomDriver();
            driver2.IsPersistent = true;
            driver2.BlockPermanentEvents = true;

            // Make drivers exit and stand near vehicles
            GameFiber.Sleep(500);
            driver1.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(3000);
            driver2.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(3000);

            // Drivers either argue or check damage
            if (rng.Next(2) == 0)
            {
                // Make them face each other and argue
                GameFiber.Sleep(1000);
                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(driver1, driver2, -1);
                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(driver2, driver1, -1);

                GameFiber.Sleep(500);
                driver1.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                driver2.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
            }
            else
            {
                // Make them inspect damage
                driver1.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", -1f, AnimationFlags.Loop);
                driver2.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@depressed@idle_a"), "idle_a", -1f, AnimationFlags.Loop);
            }

            // Create blips
            accidentBlip = new Blip(spawnpoint)
            {
                Color = Color.Orange,
                IsRouteEnabled = true,
                Sprite = BlipSprite.GetawayCar
            };

            vehicle1Blip = vehicle1.AttachBlip();
            vehicle1Blip.Color = Color.Red;
            vehicle1Blip.Alpha = 0.5f;

            vehicle2Blip = vehicle2.AttachBlip();
            vehicle2Blip.Color = Color.Red;
            vehicle2Blip.Alpha = 0.5f;

            hasInvestigated = false;
            hasClearedScene = false;
            conversationCounter = 0;

            return base.OnCalloutAccepted();
        }

        private void ApplyAccidentDamage(Vehicle veh)
        {
            if (veh == null || !veh.Exists()) return;

            // Apply visual damage
            veh.Deform(veh.Position, 1000f, rng.Next(50, 150));

            // Break windows randomly
            if (rng.Next(2) == 0) NativeFunction.Natives.SMASH_VEHICLE_WINDOW(veh, 0);
            if (rng.Next(2) == 0) NativeFunction.Natives.SMASH_VEHICLE_WINDOW(veh, 1);

            // Damage specific parts
            NativeFunction.Natives.SET_VEHICLE_DOOR_BROKEN(veh, rng.Next(6), rng.Next(2) == 0);

            // Engine smoke for severe accidents
            if (rng.Next(3) == 0)
            {
                veh.EngineHealth = rng.Next(100, 400);
            }
        }

        public override void OnCalloutNotAccepted()
        {
            SafeDelete(vehicle1);
            SafeDelete(vehicle2);
            SafeDelete(driver1);
            SafeDelete(driver2);
            SafeDeleteBlip(accidentBlip);
            SafeDeleteBlip(vehicle1Blip);
            SafeDeleteBlip(vehicle2Blip);

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            try
            {
                // Player arrived at scene
                if (MainPlayer.DistanceTo(spawnpoint) <= 30f && !hasInvestigated)
                {
                    if (Settings.HelpMessages)
                    {
                        Game.DisplayHelp("Investigate the accident scene. Press ~y~Y~w~ to interview drivers.");
                    }
                    hasInvestigated = true;
                }

                // Interview drivers
                if ((MainPlayer.DistanceTo(driver1) <= 4f || MainPlayer.DistanceTo(driver2) <= 4f) && conversationCounter < 7)
                {
                    if (Settings.HelpMessages && conversationCounter == 0)
                    {
                        Game.DisplayHelp("Press ~y~Y~w~ to speak with the drivers.");
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
                }

                // End call key
                if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
                {
                    if (Settings.MissionMessages)
                    {
                        BigMessageThread bigMessage = new BigMessageThread();
                        bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "Scene cleared. You are now ~r~CODE 4~w~.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
                    }
                    End();
                }
            }
            catch (Exception ex)
            {
                if (Settings.EnableLogs)
                {
                    Game.LogTrivial("Adam69 Callouts [LOG]: Error in Traffic Accident callout. Error: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts [LOG]: ERROR: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts [LOG]: ERROR: " + ex.StackTrace);
                }
            }

            base.Process();
        }

        private void HandleConversation()
        {
            if ((driver1 == null || !driver1.Exists()) && (driver2 == null || !driver2.Exists())) return;

            Ped activePed = MainPlayer.DistanceTo(driver1) <= MainPlayer.DistanceTo(driver2) ? driver1 : driver2;
            Ped otherPed = activePed == driver1 ? driver2 : driver1;

            string gender = activePed.IsMale ? "Sir" : "Ma'am";
            string otherGender = otherPed.IsMale ? "he" : "she";

            switch (conversationCounter)
            {
                case 1:
                    activePed.Tasks.Clear();
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(activePed, MainPlayer, -1);
                    Game.DisplaySubtitle("~b~You~w~: Is everyone okay here? Anyone need medical attention?");
                    GameFiber.Wait(1000);
                    break;
                case 2:
                    activePed.Tasks.PlayAnimation(new AnimationDictionary("gestures@m@standing@casual"), "gesture_hand_down", -1f, AnimationFlags.None);
                    Game.DisplaySubtitle($"~y~Driver~w~: I'm fine officer, just shaken up. {(injuryStatus.Contains("No injuries") ? "We're both okay." : "But I think the other driver might be hurt.")}");
                    GameFiber.Wait(1000);
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Can you tell me what happened?");
                    GameFiber.Wait(1000);
                    break;
                case 4:
                    string[] excuses = new string[]
                    {
                        $"~y~Driver~w~: {otherGender.ToUpper()} ran the red light! I didn't have time to stop!",
                        $"~y~Driver~w~: {otherGender.ToUpper()} came out of nowhere! {otherGender.ToUpper()} wasn't paying attention!",
                        $"~y~Driver~w~: I was just driving normally when {otherGender} rear-ended me!",
                        $"~y~Driver~w~: {otherGender.ToUpper()} was on their phone and drifted into my lane!"
                    };
                    activePed.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle(excuses[rng.Next(excuses.Length)]);
                    GameFiber.Wait(1000);
                    break;
                case 5:
                    NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(otherPed, MainPlayer, -1);
                    otherPed.Tasks.PlayAnimation(new AnimationDictionary("anim@amb@casino@brawl@fights@argue@"), "arguement_loop_mp_m_brawler_01", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~o~Other Driver~w~: That's not true! I had the right of way! This is all your fault!");
                    GameFiber.Wait(1000);
                    break;
                case 6:
                    Game.DisplaySubtitle("~b~You~w~: Alright, calm down. I'll need to see both your licenses and registrations. DOT will be here shortly to clear the vehicles.");
                    GameFiber.Wait(1000);
                    break;
                case 7:
                    activePed.Tasks.PlayAnimation(new AnimationDictionary("rcmjosh1"), "idle", -1f, AnimationFlags.Loop);
                    otherPed.Tasks.PlayAnimation(new AnimationDictionary("rcmjosh1"), "idle", -1f, AnimationFlags.Loop);
                    Game.DisplaySubtitle("~w~Investigation complete. You may clear the scene when ready.");
                    GameFiber.Wait(1000);
                    break;
            }
        }

        public override void End()
        {
            SafeDelete(vehicle1);
            SafeDelete(vehicle2);
            SafeDismiss(driver1);
            SafeDismiss(driver2);
            SafeDeleteBlip(accidentBlip);
            SafeDeleteBlip(vehicle1Blip);
            SafeDeleteBlip(vehicle2Blip);

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Traffic Accident", "~b~You~w~: Dispatch, accident scene cleared. We are ~g~Code 4~w~.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();
                bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "Return to patrol!", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Traffic Accident callout is code 4!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Traffic Accident callout is code 4!");
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