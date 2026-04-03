using CalloutInterfaceAPI;
using Adam69Callouts.Common;

namespace Adam69Callouts.Callouts
{
    [CalloutInterface("[Adam69 Callouts] Suspicious Person", CalloutProbability.Medium, "Suspicious Person Reported", "Code 2", "LSPD")]
    public class SuspiciousPerson : Callout
    {
        private static Ped suspect;
        private static Blip susBlip;
        private static Vector3 spawnpoint;
        private static readonly string[] wepList = new string[] { "WEAPON_PISTOL", "WEAPON_COMBATPISTOL", "WEAPON_COMBATMG", "WEAPON_TACTICALRIFLE", "weapon_snspistol", "weapon_marksmanpistol", "weapon_doubleaction" };
        private static int counter;
        private static string malefemale;
        private static string copGender;
        private static readonly Random random = new Random();

        private enum SuspiciousPersonScenario
        {
            Compliant,
            Flees,
            Attacks,
            Innocent
        }
        private SuspiciousPersonScenario scenario;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = World.GetNextPositionOnStreet(MainPlayer.Position.Around(1000f));
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 100f);
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_SuspiciousPerson_Audio");
            CalloutInterfaceAPI.Functions.SendMessage(this, "Citizen's report of a suspicious person.");
            CalloutMessage = "Suspicious Person Reported";
            CalloutPosition = spawnpoint;

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (Settings.EnableLogs)
            {
                Game.LogTrivial("[Adam69 Callouts LOG]: Suspicious Person callout accepted!");
            }
            else
            {
                Settings.EnableLogs = false;
            }

            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Suspicious Person", "~b~Dispatch~w~: The suspect has been spotted! Respond ~r~Code 2~w~.");

            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio("Adam69Callouts_Respond_Code_2_Audio");

            scenario = (SuspiciousPersonScenario)random.Next(Enum.GetValues(typeof(SuspiciousPersonScenario)).Length);

            suspect = new Ped(spawnpoint)
            {
                IsPersistent = true,
                BlockPermanentEvents = true
            };
            suspect.IsValid();
            suspect.Exists();

            suspect.Tasks.PlayAnimation(new AnimationDictionary("anim@heists@fleeca_bank@ig_7_jetski_owner"), "owner_idle", -1f, AnimationFlags.Loop);

            susBlip = suspect.AttachBlip();
            susBlip.Color = System.Drawing.Color.Red;
            susBlip.Alpha = 0.5f;
            susBlip.IsRouteEnabled = true;
            susBlip.Exists();

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
            if(susBlip != null && susBlip.Exists()) susBlip.Delete();

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            if (MainPlayer.DistanceTo(suspect) <= 10f)
            {

                if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                {
                    counter++;
                    HandleInteraction();
                }
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

        private void HandleInteraction()
        {
            switch (scenario)
            {
                case SuspiciousPersonScenario.Compliant:
                    HandleCompliantScenario();
                    break;
                case SuspiciousPersonScenario.Flees:
                    HandleFleesScenario();
                    break;
                case SuspiciousPersonScenario.Attacks:
                    HandleAttacksScenario();
                    break;
                case SuspiciousPersonScenario.Innocent:
                    HandleInnocentScenario();
                    break;
            }
        }

        // Add these methods to the class:
        private void HandleCompliantScenario()
        {
            switch (counter)
            {
                case 1:
                    Game.DisplaySubtitle($"~b~You~w~: Hey there, {malefemale}. What's going on?");
                    break;
                case 2:
                    Game.DisplaySubtitle("~r~Suspect~w~: Just waiting for a friend, officer.");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Got any weapons on you?");
                    break;
                case 4:
                    Game.DisplaySubtitle("~r~Suspect~w~: No, you can check.");
                    break;
                case 5:
                    Game.DisplaySubtitle("~b~You~w~: Thanks for cooperating. You're free to go.");
                    End();
                    break;
            }
        }

        private void HandleAttacksScenario()
        {
            switch (counter)
            {
                case 1:
                    Game.DisplaySubtitle($"~b~You~w~: Excuse me, {malefemale}, what are you doing here?");
                    break;
                case 2:
                    Game.DisplaySubtitle("~r~Suspect~w~: None of your business!");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: I need to check for weapons.");
                    break;
                case 4:
                    Game.DisplaySubtitle("~r~Suspect~w~: Over my dead body!");
                    suspect.Tasks.FightAgainst(MainPlayer);
                    suspect.Armor = 1500;
                    if (suspect != null && suspect.Exists() && suspect.IsValid())
                        SafeInventory.SafeGiveWeapon(suspect, wepList[random.Next(wepList.Length)], 500, true);
                    susBlip.Color = System.Drawing.Color.Red;
                    break;
            }
        }

        private void HandleInnocentScenario()
        {
            switch (counter)
            {
                case 1:
                    Game.DisplaySubtitle($"~b~You~w~: Hi, {malefemale}, any reason you're here?");
                    break;
                case 2:
                    Game.DisplaySubtitle("~r~Suspect~w~: Just taking a walk, officer.");
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Sorry to bother you. Have a good day.");
                    End();
                    break;
            }
        }

        private void HandleFleesScenario()
        {
            switch (counter)
            {
                case 1:
                    Game.DisplaySubtitle($"~b~You~w~: Hey, {malefemale}, can I talk to you?");
                    GameFiber.Wait(1000); // Wait a second before continuing
                    suspect.Tasks.ReactAndFlee(MainPlayer);
                    break;
                case 2:
                    Game.DisplaySubtitle("~r~Suspect~w~: Uh... I gotta go!");
                    GameFiber.Wait(500); // Wait half a second before continuing
                    suspect.Tasks.ReactAndFlee(MainPlayer);
                    susBlip.Color = System.Drawing.Color.Yellow;
                    break;
                case 3:
                    Game.DisplaySubtitle("~b~You~w~: Stop! Police!");
                    GameFiber.Wait(1000); // Wait a second before continuing
                    if (suspect.IsRunning || suspect.IsSprinting || suspect.IsWalking)
                    {
                        Game.DisplaySubtitle("~b~Dispatch~w~: Suspect is fleeing! Pursue and apprehend!");
                    }
                    else
                    {
                        Game.DisplaySubtitle("~b~Dispatch~w~: Suspect is not fleeing. Proceed with caution.");
                    }
                    break;
            }
        }



        public override void End()
        {
            if (suspect != null && suspect.Exists()) suspect.Delete();
            if (susBlip != null && susBlip.Exists()) susBlip.Delete();
            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "~w~Suspicious Person", "~b~You~w~: Dispatch, we are ~g~Code 4~w~. Show me back 10-8..");
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

            if (Settings.EnableLogs)
            {
                Game.LogTrivial("Adam69 Callouts [LOG]: Suspicious Person callout is code 4!");
            }
            else
            {
                Settings.EnableLogs = false;
            }

            base.End();
        }
    }
}