using CalloutInterfaceAPI;
using System;
using System.Drawing;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts] Road Debris", CalloutProbability.Low, "Road debris hazard reported", "Code 2", "LEO")]
    public class RoadDebris : Callout
    {
        private static readonly Random rng = new Random();

        private static readonly string[] debrisObjects = new string[]
        {
            "prop_roadcone02a", "prop_roadcone02b", "prop_roadcone02c",
            "prop_barrier_work05", "prop_barrier_work06a",
            "prop_wheel_tyre", "prop_wheel_01", "prop_wheel_03",
            "prop_palette_01a", "prop_palette_02a",
            "prop_box_wood01a", "prop_box_wood05a",
            "prop_gas_tank_01a", "prop_gas_tank_02a"
        };

        private Vector3 spawnpoint;
        private Rage.Object[] debrisItems;
        private Blip debrisBlip;
        private bool hasCleared;
        private int itemsCleared;
        private int totalItems;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = World.GetNextPositionOnStreet(MainPlayer.Position.Around2D(200f, 400f));

            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            CalloutInterfaceAPI.Functions.SendMessage(this, "Road debris creating a traffic hazard. Units respond to clear roadway.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT CRIME_SUSPICIOUS_ACTIVITY IN_OR_ON_POSITION", spawnpoint);
            CalloutMessage = "Road Debris";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Road Debris callout has been accepted!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Road Debris callout has been accepted!");
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Road Debris", "~b~Dispatch~w~: Debris in roadway creating hazard. Clear the scene. Respond ~r~Code 2~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            // Create debris scatter
            totalItems = rng.Next(3, 8);
            debrisItems = new Rage.Object[totalItems];

            float heading = World.GetStreetHash(spawnpoint);
            Vector3 forward = MathHelper.ConvertHeadingToDirection(heading);
            Vector3 right = MathHelper.ConvertHeadingToDirection(heading + 90f);

            for (int i = 0; i < totalItems; i++)
            {
                string debrisModel = debrisObjects[rng.Next(debrisObjects.Length)];

                // Scatter debris across the road
                Vector3 offset = forward * rng.Next(-5, 5) + right * rng.Next(-3, 3);
                Vector3 debrisPos = spawnpoint + offset;

                debrisItems[i] = new Rage.Object(debrisModel, debrisPos)
                {
                    IsPersistent = true
                };

                // Random rotation for realistic scatter
                debrisItems[i].Rotation = new Rotator(rng.Next(-30, 30), rng.Next(-30, 30), rng.Next(0, 360));
            }

            // Create area blip
            debrisBlip = new Blip(spawnpoint)
            {
                Color = Color.Orange,
                IsRouteEnabled = true,
                Sprite = BlipSprite.CrateDrop
            };

            hasCleared = false;
            itemsCleared = 0;

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (debrisItems != null)
            {
                foreach (var item in debrisItems)
                {
                    SafeDelete(item);
                }
            }
            SafeDeleteBlip(debrisBlip);

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            try
            {
                // Check proximity to debris
                if (MainPlayer.DistanceTo(spawnpoint) <= 20f && !hasCleared)
                {
                    if (Settings.HelpMessages && itemsCleared == 0)
                    {
                        Game.DisplayHelp("Clear the debris from the roadway. Press ~y~E~w~ near items to remove them.");
                    }

                    // Check if player is near any debris item
                    for (int i = 0; i < debrisItems.Length; i++)
                    {
                        if (debrisItems[i] != null && debrisItems[i].Exists())
                        {
                            if (MainPlayer.DistanceTo(debrisItems[i]) <= 2f)
                            {
                                Game.DisplayHelp("Press ~y~E~w~ to clear this debris.");

                                if (Game.IsKeyDown(System.Windows.Forms.Keys.E))
                                {
                                    // Play pickup animation
                                    MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("anim@move_m@trash"), "pickup", -1f, AnimationFlags.UpperBodyOnly);
                                    GameFiber.Sleep(1500);

                                    // Delete the debris
                                    debrisItems[i].Delete();
                                    itemsCleared++;

                                    Game.DisplayNotification("~g~Debris cleared: " + itemsCleared + "/" + totalItems);

                                    if (itemsCleared >= totalItems)
                                    {
                                        hasCleared = true;
                                        Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Road Debris", "~g~All debris cleared! Roadway is safe.");

                                        if (Settings.MissionMessages)
                                        {
                                            BigMessageThread bigMessage = new BigMessageThread();
                                            bigMessage.MessageInstance.ShowColoredShard("Scene Cleared!", "All debris removed!", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
                                        }
                                    }

                                    GameFiber.Sleep(500); // Prevent double-clearing
                                }
                            }
                        }
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
                        bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "You are now ~r~CODE 4~w~.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
                    }
                    End();
                    return;
                }
            }
            catch (Exception ex)
            {
                if (Settings.EnableLogs)
                {
                    Game.LogTrivial("Adam69 Callouts [LOG]: Error in Road Debris callout. Error: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts [LOG]: ERROR: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts [LOG]: ERROR: " + ex.StackTrace);
                }
            }

            base.Process();
        }

        public override void End()
        {
            if (debrisItems != null)
            {
                foreach (var item in debrisItems)
                {
                    SafeDelete(item);
                }
            }
            SafeDeleteBlip(debrisBlip);

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~y~Road Debris", "~b~You~w~: Dispatch, roadway cleared. We are ~g~Code 4~w~.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();
                bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "Return to patrol!", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Road Debris callout is code 4!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Road Debris callout is code 4!");
            }
        }

        #region Helpers
        private void SafeDelete(Rage.Object obj)
        {
            if (obj != null && obj.Exists()) obj.Delete();
        }

        private void SafeDeleteBlip(Blip blip)
        {
            if (blip != null && blip.Exists()) blip.Delete();
        }
        #endregion
    }
}