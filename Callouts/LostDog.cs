using CalloutInterfaceAPI;
using Adam69Callouts.Common;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts] Lost Dog", CalloutProbability.Medium, "Caller reports a lost family dog wandering near streets or parks; possible hazard to traffic", "Code 2", "LSPD")]
    public class LostDog : Callout
    {
        private static Ped owner;
        private static Ped dog;
        private static Blip dogBlip;
        private static Blip ownerBlip;
        private static Vector3 spawnPoint;
        private static Vector3 dogLastSeen;
        private static int dialogStage;
        private static bool dogFriendly;
        private static readonly string[] dogModels = new string[] { "a_c_shepherd", "a_c_husky", "a_c_retriever", "a_c_poodle" };
        private static readonly string[] ownerModels = new string[] { "a_m_m_skater_01", "a_f_m_bevhills_01", "a_m_m_farmer_01" };
        private static bool dogIsLeashed = false;
        private static bool handingOver = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            List<Vector3> possible = new()
            {
                new(250.12f, -1500.42f, 28.57f),
                new(-1034.22f, -2738.51f, 20.17f),
                new(1235.33f, 2724.11f, 38.01f),
                new(-205.44f, 6316.12f, 31.49f),
                new(1754.22f, 3253.11f, 41.13f)
            };
            spawnPoint = LocationChooser.ChooseNearestLocation(possible);
            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 80f);
            CalloutInterfaceAPI.Functions.SendMessage(this, "Caller reports a lost dog possibly wandering into traffic. Owner on scene requesting assistance.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("ANIMAL_DISTURBANCE_01", spawnPoint);
            CalloutMessage = "Lost Dog Reported";
            CalloutPosition = spawnPoint;
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: Lost Dog callout accepted!");
                LoggingManager.Log("Adam69 Callouts: Lost Dog callout accepted!");
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "Lost Dog", "~b~Dispatch~w~: Reports of a lost dog wandering near the roads. Respond ~r~Code 2~w~.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            dogFriendly = new Random().Next(0, 10) > 2; // ~70% friendly
            dogLastSeen = spawnPoint.Around2D(8f, 40f);

            owner = new Ped(ownerModels[new Random().Next(ownerModels.Length)], spawnPoint.Around2D(2f, 6f), 0f);
            if (owner.Exists())
            {
                owner.IsPersistent = true;
                owner.BlockPermanentEvents = true;
                owner.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_wash@male@base"), "base", -1f, AnimationFlags.Loop);
                ownerBlip = owner.AttachBlip();
                ownerBlip.Color = System.Drawing.Color.Green;
                ownerBlip.Scale = 0.8f;
            }

            dog = new Ped(dogModels[new Random().Next(dogModels.Length)], dogLastSeen, 0f);
            if (dog.Exists())
            {
                dog.IsPersistent = true;
                dog.BlockPermanentEvents = true;
                dog.Tasks.PlayAnimation(new AnimationDictionary("creatures@dog@move"), "idle", -1f, AnimationFlags.Loop);
                dogBlip = dog.AttachBlip();
                dogBlip.Color = System.Drawing.Color.Yellow;
                dogBlip.IsRouteEnabled = true;
            }

            dialogStage = 0;
            dogIsLeashed = false;
            handingOver = false;

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (dog != null && dog.Exists()) dog.Delete();
            if (owner != null && owner.Exists()) owner.Delete();
            if (dogBlip != null && dogBlip.Exists()) dogBlip.Delete();
            if (ownerBlip != null && ownerBlip.Exists()) ownerBlip.Delete();

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            base.Process();

            try
            {
                // Talk to owner for details
                if (owner != null && owner.Exists() && MainPlayer.DistanceTo(owner) <= 12f && dialogStage == 0)
                {
                    if (Settings.HelpMessages)
                    {
                        Game.DisplayHelp("Press ~y~" + Settings.Dialog.ToString() + "~w~ to speak with the owner.");
                    }

                    if (Game.IsKeyDown(Settings.Dialog))
                    {
                        GameFiber.Sleep(400);
                        NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(owner, MainPlayer, -1);
                        Game.DisplaySubtitle("~g~Owner~w~: Officer, thank you! My dog slipped out of the gate and keeps running toward the road. Please help before it gets hit.");
                        GameFiber.Sleep(3500);
                        Game.DisplaySubtitle("~b~You~w~: I'll look for the dog. Keep an eye on traffic and try to stay with me.");
                        dialogStage++;
                    }
                }

                // Player interacts with dog
                if (dog != null && dog.Exists() && MainPlayer.DistanceTo(dog) <= 10f && dialogStage >= 1 && !dogIsLeashed)
                {
                    if (Settings.HelpMessages)
                    {
                        Game.DisplayHelp("Press ~y~" + Settings.Dialog.ToString() + "~w~ to calm the dog. Press ~y~" + Settings.CallAnimalControlKey.ToString() + "~w~ to call Animal Control.");
                    }

                    if (Game.IsKeyDown(Settings.CallAnimalControlKey))
                    {
                        GameFiber.Sleep(200);
                        Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "Animal Control", "~b~Dispatch~w~: Animal Control en route to handle the animal.");
                        LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Animal_Control_Audio");
                        if (dog.Exists()) dog.Dismiss();
                        if (dogBlip != null && dogBlip.Exists()) dogBlip.Delete();
                    }

                    if (Game.IsKeyDown(Settings.Dialog))
                    {
                        GameFiber.Sleep(300);

                        // Calm the dog and attach a "lead" behavior
                        if (dogFriendly)
                        {
                            NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(dog, MainPlayer, -1);
                            Game.DisplaySubtitle("~b~You~w~: It's okay, come here. Good dog.");
                            GameFiber.Sleep(1200);

                            // Play simple lead animation on the player to simulate leash
                            try
                            {
                                MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("mp_ped_interaction"), "handshake_guy_a", 2f, AnimationFlags.None);
                            }
                            catch { /* best-effort - ignore if anim missing */ }

                            // Make the dog follow closely with a stable offset
                            dog.Tasks.FollowToOffsetFromEntity(MainPlayer, new Vector3(0f, 1f, 0f));
                            dogIsLeashed = true;
                            if (dogBlip != null && dogBlip.Exists()) dogBlip.IsRouteEnabled = true;

                            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "Lost Dog", "~b~The dog is calm and will follow you. Lead it back to the owner.");
                        }
                        else
                        {
                            NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(dog, MainPlayer, -1);
                            Game.DisplaySubtitle("~r~The dog is frightened and won't approach. Consider calling Animal Control or lure with food.");
                            if (Settings.EnableLogs) LoggingManager.Log("Adam69 Callouts: Lost Dog is not friendly, suggest Animal Control.");
                        }
                    }
                }

                // Improved follow: if leashed, keep player playing a "lead" walk animation for realism
                if (dogIsLeashed && dog != null && dog.Exists())
                {
                    try
                    {
                        MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_walk_dog_lead@male@base"), "walk", 4f, AnimationFlags.Loop);
                    }
                    catch
                    {
                        // ignore missing animation at runtime; follow behavior still works
                    }
                }

                // When dog is following and player approaches owner, perform handover sequence
                if (dogIsLeashed && dog != null && dog.Exists() && owner != null && owner.Exists() && MainPlayer.DistanceTo(owner) <= 6f && !handingOver)
                {
                    handingOver = true;
                    GameFiber.StartNew(delegate
                    {
                        try
                        {
                            // Stop follow behavior
                            dog.Tasks.Clear();

                            // Move dog close to owner
                            Vector3 ownerStand = owner.Position.Around2D(0.5f, 1.2f);
                            dog.Tasks.GoToOffsetFromEntity(owner, 0.5f, 0f, 1.0f);

                            // Short pause to let dog move
                            GameFiber.Sleep(1000);

                            // Player plays a give/hand-off animation (best-effort)
                            try
                            {
                                MainPlayer.Tasks.PlayAnimation(new AnimationDictionary("mp_common"), "givetake1_a", 2f, AnimationFlags.None);
                            }
                            catch { }

                            // Owner plays a grateful/receive animation (best-effort)
                            try
                            {
                                owner.Tasks.PlayAnimation(new AnimationDictionary("mini@repair"), "fixing_a_ped", 2f, AnimationFlags.None);
                            }
                            catch { }

                            // Dog sits or plays a calm animation
                            try
                            {
                                dog.Tasks.PlayAnimation(new AnimationDictionary("creatures@dog@move"), "sit", 2f, AnimationFlags.None);
                            }
                            catch { }

                            GameFiber.Sleep(2000);

                            // Dialogue and finalization
                            Game.DisplaySubtitle("~g~Owner~w~: Oh my god, thank you so much! You're a lifesaver.");
                            GameFiber.Sleep(1800);
                            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "Lost Dog", "~b~You~w~: The dog has been reunited. Code 4.");

                            // Cleanup
                            if (dogBlip != null && dogBlip.Exists()) dogBlip.Delete();
                            if (ownerBlip != null && ownerBlip.Exists()) ownerBlip.Delete();

                            if (dog != null && dog.Exists()) dog.Dismiss();
                            if (owner != null && owner.Exists()) owner.Dismiss();

                            End();
                        }
                        catch (Exception ex)
                        {
                            if (Settings.EnableLogs)
                            {
                                Game.LogTrivial("[Adam69 Callouts ERROR]: Lost Dog handover exception: " + ex.Message);
                                LoggingManager.Log("Adam69 Callouts: Lost Dog handover exception: " + ex.Message);
                            }
                            handingOver = false;
                        }
                    });
                }

                // Player death or manual end
                if (MainPlayer.IsDead)
                {
                    if (Settings.MissionMessages)
                    {
                        BigMessageThread bigMessage = new BigMessageThread();
                        bigMessage.MessageInstance.ShowColoredShard("MISSION FAILED!", "You have fallen in the line of duty.", RAGENativeUI.HudColor.Red, RAGENativeUI.HudColor.Black, 5000);
                    }

                    if (Settings.EnableLogs)
                    {
                        LoggingManager.Log("Adam69 Callouts: Lost Dog callout failed - Player died.");
                    }

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
                    Game.LogTrivial("[Adam69 Callouts ERROR]: Lost Dog Process Exception: " + ex.Message);
                    LoggingManager.Log("Adam69 Callouts: Lost Dog Process Exception: " + ex.Message);
                }
            }
        }

        public override void End()
        {
            try
            {
                if (dog != null && dog.Exists()) dog.Dismiss();
                if (owner != null && owner.Exists()) owner.Dismiss();
                if (dogBlip != null && dogBlip.Exists()) dogBlip.Delete();
                if (ownerBlip != null && ownerBlip.Exists()) ownerBlip.Delete();

                Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "Lost Dog", "~b~You~w~: Dispatch, we are ~g~CODE 4~w~. Show me back 10-8.");
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

                if (Settings.MissionMessages)
                {
                    BigMessageThread bigMessage = new BigMessageThread();
                    bigMessage.MessageInstance.ShowColoredShard("Callout Completed!", "Dog reunited with owner. You are now ~g~CODE 4~w~.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
                }
            }
            catch
            {
                // ignore cleanup errors
            }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: Lost Dog callout ended CODE 4.");
                LoggingManager.Log("Adam69 Callouts: Lost Dog callout ended CODE 4.");
            }
        }
    }
}