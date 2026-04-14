using CalloutInterfaceAPI;

namespace Adam69Callouts.Callouts
{

    [CalloutInterface("[Adam69 Callouts] Dead Bird On The Road", CalloutProbability.Medium, "A dead bird is on the road.", "Code 2", "SAFW")]
    public class DeadBirdOnTheRoad : Callout
    {
        private static readonly string[] animalList = new string[] { "a_c_chickenhawk", "a_c_hen", "a_c_pigeon", "a_c_seagull", "a_c_crow" };
        private static Vector3 spawnpoint;
        private static Ped deadBird;
        private static Blip deadBirdBlip;

        public override bool OnBeforeCalloutDisplayed()
        {
            List<Vector3> list = new()
            {
                new(894.278137f, 420.9684f, 119.312004f),
                new(-1641.65662f, 986.3751f, 152.623215f),
                new(-1048.19531f, -1520.81824f, 5.106961f),
            };
            spawnpoint = LocationChooser.ChooseNearestLocation(list);
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("CRIME_ANIMAL_KILLED_02", spawnpoint);
            CalloutInterfaceAPI.Functions.SendMessage(this, "A dead bird is on the road.");
            CalloutMessage = "Dead Bird Reported";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Dead Bird On The Road callout has been accepted!");
            }
            else
            {
                Settings.EnableLogs = false;
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Dead Bird On The Road", "~b~Dispatch~w~: A dead bird has been reported on the road. Respond ~y~Code 2~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio_01");

            deadBird = new Ped(animalList[new Random().Next((int)animalList.Length)], spawnpoint, 0f);
            deadBird.IsPersistent = true;
            deadBird.BlockPermanentEvents = true;
            deadBird.Kill();
            deadBird.IsValid();

            NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(deadBird, "Hit By Vehicle", 1f, 1f);

            deadBirdBlip = deadBird.AttachBlip();
            deadBirdBlip.Color = System.Drawing.Color.Red;
            deadBirdBlip.IsRouteEnabled = true;

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (deadBird.Exists()) deadBird.Delete();
            if (deadBirdBlip.Exists()) deadBirdBlip.Delete();

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {

            if (Game.IsKeyDown(System.Windows.Forms.Keys.NumPad1))
            {
                StopThePed.API.Functions.callAnimalControl();
                Game.DisplaySubtitle("~b~You~w~: Dispatch, requesting Animal Control to my 20.");
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_AnimalControl_Audio_01");
            }

            if (MainPlayer.DistanceTo(deadBird) <= 5f)
            {
                if (Settings.HelpMessages)
                {
                    Game.DisplayHelp("Press ~y~" + Settings.CallAnimalControlKey.ToString() + "~w~ to call animal control");
                }
                else
                {
                    Settings.HelpMessages = false;
                    if (Settings.EnableLogs)
                    {
                        Game.LogTrivial("[LOG]: Help messages are disabled in the config file.");
                        LoggingManager.Log("[LOG]: Help messages are disabled in the config file.");
                    }
                    else
                    {
                        Settings.EnableLogs = false;
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
                    if (Settings.EnableLogs)
                    {
                        Game.LogTrivial("[LOG]: Mission messages are disabled in the config file.");
                    }
                    else
                    {
                        Settings.EnableLogs = false;
                    }
                }

                End();
            }

            if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
            {
                LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

                End();
            }

            base.Process();
        }

        public override void End()
        {
            if (deadBird) deadBird.Dismiss();
            if (deadBirdBlip) deadBirdBlip.Delete();
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");
            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Dead Bird On The Road", "~b~You~w~: We are Code 4. Show me back 10-8!");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new();
                bigMessage.MessageInstance.ShowColoredShard("MISSION SUCCESS!", "You have successfully cleared the dead bird from the road.", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }
            else
            {
                Settings.MissionMessages = false;
                Game.LogTrivial("[LOG]: Mission messages are disabled in the config file.");
            }

            base.End();

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Dead Bird On The Road callout has ended!");
            }
        }
    }
}
