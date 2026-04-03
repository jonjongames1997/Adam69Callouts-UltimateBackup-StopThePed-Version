using CalloutInterfaceAPI;
using Adam69Callouts.Common;

namespace Adam69Callouts.Callouts
{

    [CalloutInterface("[Adam69 Callouts] - Deranged Drunken Feller", CalloutProbability.Medium, "Reports of a drunken feller", "Code 2", "LSPD")]


    public class DerangedDrunkenFeller : Callout
    {
        private static Ped suspect;
        private static Vector3 spawnpoint;
        private static Blip blip;
        private static int counter;
        private static string malefemale;
        private static int copgender;
        private static readonly string[] wepList = new string[] { "weapon_pistol", "weapon_combatmg", "weapon_combatpistol" };

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = World.GetNextPositionOnStreet(MainPlayer.Position.Around(500f));
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            CalloutInterfaceAPI.Functions.SendMessage(this, "A Deranged Drunken Feller has been reported in the area. Respond Code 2.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("SUSPICIOUS_PERSON_02", spawnpoint);
            CalloutMessage = "Deranged Drunken Feller Reported";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Deranged Drunken Feller callout has been accepted!");
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "Deranged Drunken Feller", "~b~Dispatch~w~: Suspect has been located. Respond ~r~Code 2~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            blip = suspect.AttachBlip();
            blip.Color = System.Drawing.Color.Red;
            blip.IsRouteEnabled = true;

            if (suspect.IsMale)
                malefemale = "Sir";
            else
                malefemale = "Ma'am";

            counter = 0;

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (suspect != null && suspect.Exists()) suspect.Delete();
            if (blip != null && blip.Exists()) blip.Delete();

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            if (MainPlayer.DistanceTo(suspect) <= 10f)
            {

                if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                {
                    counter++;
                    try
                    {
                        if (counter == 1)
                        {
                            NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(suspect, MainPlayer, -1);
                            Game.DisplaySubtitle("~b~You~w~: What goin' on, feller? Have anything to drink today?");
                        }
                        if (counter == 2)
                        {
                            suspect.Tasks.PlayAnimation(new AnimationDictionary("random@drunk_driver_1"), "drunk_driver_stand_loop_dd2", -1f, AnimationFlags.Loop);
                            Game.DisplaySubtitle("~r~Suspect~w~: *slurring* What you want, officer pigfucker?");
                        }
                        if (counter == 3)
                        {
                            suspect.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_stand_impatient@female@no_sign@base"), "base", -1f, AnimationFlags.Loop);
                            Game.DisplaySubtitle("~b~You~w~: Ok, we'll do a few tests to see if you're drunk.");
                        }
                        if (counter == 4)
                        {
                            suspect.Tasks.PlayAnimation(new AnimationDictionary("random@drunk_driver_1"), "drunk_driver_stand_loop_dd2", -1f, AnimationFlags.Loop);
                            Game.DisplaySubtitle("~r~Suspect~w~: *slurring* You got to catch me first, donut eater.");
                        }
                        if (counter == 5)
                        {
                            Game.DisplaySubtitle("Convo ended. Chase and arrest the suspect.");
                            suspect.Tasks.FightAgainst(MainPlayer);
                            suspect.Armor = 500;
                            if (suspect != null && suspect.Exists() && suspect.IsValid())
                                SafeInventory.SafeGiveWeapon(suspect, wepList[new Random().Next((int)wepList.Length)], 500, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Settings.EnableLogs)
                        {
                            Game.LogTrivial("Adam69 Callouts [LOG]: Exception in Deranged Drunken Feller callout: " + ex.Message);
                            Game.LogTrivial("Adam69 Callouts [LOG]: Exception in Deranged Drunken Feller callout: " + ex.StackTrace);
                            LoggingManager.Log("Adam69 Callouts [LOG]: Exception in Deranged Drunken Feller callout: " + ex.Message);
                            LoggingManager.Log("Adam69 Callouts [LOG]: Exception in Deranged Drunken Feller callout: " + ex.StackTrace);
                        }
                        else
                        {
                            Settings.EnableLogs = false;
                        }
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
                    Game.LogTrivial("[LOG]: Mission messages are disabled in the config file.");
                }

                End();
            }

            if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
            {
                if (Settings.MissionMessages)
                {
                    BigMessageThread bigMessage = new BigMessageThread();
                    bigMessage.MessageInstance.ShowColoredShard("Callout Complete!", "Get back out there and protect the citizens, officer", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
                }
                else
                {
                    Settings.MissionMessages = false;
                    Game.LogTrivial("[LOG]: Mission messages are disabled in the config file.");
                    LoggingManager.Log("Adam69 Callouts [LOG]: Mission messages are disabled in the config file.");
                }

                End();
            }

            base.Process();
        }

        public override void End()
        {
            if (suspect != null && suspect.Exists()) suspect.Dismiss();
            if (blip != null && blip.Exists()) blip.Delete();
            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Deranged Drunken Feller", "~b~You~w~: Dispatch, we are ~g~Code 4~w~. Show me back 10-8.");
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Code_4_Audio");

            if (Settings.MissionMessages)
            {
                BigMessageThread bigMessage = new BigMessageThread();

                bigMessage.MessageInstance.ShowColoredShard("~g~Code 4", "Suspect Neutralized!", RAGENativeUI.HudColor.Green, RAGENativeUI.HudColor.Black, 5000);
            }
            else
            {
                Settings.MissionMessages = false;
                Game.LogTrivial("[LOG]: Mission messages are disabled in the config file.");
            }

            base.End();

            if (Settings.EnableLogs)
            {

                Game.LogTrivial("Adam69 Callouts [LOG]: Deranged Drunken Feller callout is CODE 4!");
                LoggingManager.Log("Adam69 Callouts [LOG]: Deranged Drunken Feller callout is CODE 4!");
            }
            else
            {
                Settings.EnableLogs = false;
            }
        }
    }
}