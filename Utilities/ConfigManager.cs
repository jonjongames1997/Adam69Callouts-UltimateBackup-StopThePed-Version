using Rage;
using System.IO;
using System.Globalization;

namespace Adam69Callouts.Utilities
{
    public static class ConfigManager
    {
        private static readonly string ConfigFilePath = "plugins\\LSPDFR\\Adam69Callouts\\Adam69Callouts.ini";
        private static InitializationFile config;

        private static bool EnableLogs { get; set; }
        private static bool VehicleBlockingDriveway { get; set; }
        private static bool AbandonedVehicle { get; set; }
        private static bool BicycleBlockingRoadway { get; set; }
        private static bool VehicleBlockingSidewalk { get; set; }
        private static bool BicyclePursuit { get; set; }
        private static bool PersonCarryingAConcealedWeapon { get; set; }
        private static bool Loitering { get; set; }
        private static bool VehicleBlockingCrosswalk { get; set; }
        private static bool SuspiciousVehicle { get; set; }
        private static bool DrugsFound { get; set; }
        private static bool SuspiciousPerson { get; set; }
        private static bool OfficerDown { get; set; }
        private static bool DerangedDrunkenFeller { get; set; }
        private static bool DeadBirdOnTheRoad { get; set; }
        private static bool KnifeAttack { get; set; }
        private static bool LSIAAirportIncident { get; set; }
        private static bool IndecentExposure { get; set; }
        private static bool IllegalHuntingBlaineCounty { get; set; }
        private static bool LostDogCallout { get; set; }
        private static bool SpectrumAlertFlorida { get; set; }
        private static bool SoveriegnCitizen { get; set; }
        private static bool NakedDrugUser { get; set; }
        private static bool TrafficAccident { get; set; }
        private static bool RoadDebris { get; set; }
        private static bool DisabledVehicle { get; set; }
        private static bool TrafficLightOut { get; set; }
        private static bool ToplessBeachgoer { get; set; }
        private static bool HelpMessages { get; set; }
        private static bool MissionMessages { get; set; }
        private static Keys EndCall { get; set; }
        private static Keys Dialog { get; set; }
        private static Keys PickUp { get; set; }
        private static Keys CallAnimalControlKey { get; set; }
        private static Keys CallAmbulanceKey { get; set; }
        private static Keys RequestVehicleInfo { get; set; }
        private static Keys RequestTowTruck { get; set; }
        private static Keys CallFireDepartmentKey { get; set; }
        private static Keys RequestCode2BackUp { get; set; }
        private static Keys RequestCode3BackUp { get; set; }
        private static Keys RequestPanicBackUp { get; set; }
        private static Keys RequestK9Unit { get; set; }
        private static float TrafficStopRadius { get; set; }
        private static float TrafficDensityMultiplier { get; set; }
        private static float TrafficRestoreMultiplier { get; set; }

        /// <summary>
        /// Public method to initialize the configuration system
        /// </summary>
        public static void Initialize()
        {
            LoadConfig();
            CreateConfig();
        }

        private static void LoadConfig()
        {
            Directory.CreateDirectory("Plugins\\LSPDFR\\Adam69Callouts");

            if (!File.Exists(ConfigFilePath))
            {
                Game.LogTrivial("Adam69 Callouts: Config file not found, creating default config.");
                LoggingManager.Log("Adam69 Callouts: Config file not found, creating default config.");

                File.WriteAllText(ConfigFilePath,
                    @"
Microsoft Keys Enum: https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?view=windowsdesktop-6.0

[General Settings]
 
[Keys]

Dialog = Y
EndCall = End
PickUp = E
CallAnimalControlKey = NumPad1 // this key option only works for certain callout scenarios. Will expand to existing callouts if needed. //
CallAmbulanceKey = K
RequestVehicleInfo = P // This only works with Suspicious Vehicle, Vehicle Blocking Sidewalk and Vehicle Blocking Crosswalk Callouts. 
RequestTowTruck = L // Only works in certain callouts that require a tow truck. //
RequestCode2BackUp = NumPad1 // Works in certain scenarios
RequestCode3BackUp = NumPad2 // Works in certain scenerios when necessary
RequestPanicBackUp = NumPad0 // Works in certain scenarios when necessary
RequestK9Unit = NumPad3 // Allows you to call for a K9 Unit upon request

If the P key interferes with the pause menu, go to mods/update/update.rpf/x64/data/control, 
you can remove this item line code in your default.meta:
""<Item>
	<Input>INPUT_FRONTEND_PAUSE</Input>
	<Source>IOMS_KEYBOARD</Source>
	<Parameters>
	<Item>KEY_P</Item>
	</Parameters>
</Item>""
This will not affect your ELS Panel UI. It only affects the pause menu bind to the P key.
------------------------------------

[Settings]

HelpMessages = true

MissionMessages = true // Show mission messages on screen. set to false to disable, set true to enable.

EnableLogs = false // Set to true to enable logs. Useful for debugging callouts.

[Traffic]

StopRadius = 60

DensityMultiplier = 0 // Set to 0 to disable traffic, set to 1 for normal traffic, set higher than 1 for increased traffic. //

RestoreMultiplier = 1 // Set to 0 to disable traffic restoration, set to 1 for normal restoration, set higher than 1 for increased restoration. //

-------------------------

// Set these to true or false //
// true - Callout will be active //
// false - Callout will be not active //

[Callouts]

VehicleParkingInHandicap = true
VehicleBlockingSidewalk = true
BicyclePursuit = true
PersonCarryingAConcealedWeapon = true
Loitering = true
VehicleBlockingCrosswalk = true
BicycleBlockingRoadway = true
SuspiciousVehicle = true
AbandonedVehicle = true
DrugsFound = true
SuspiciousPerson = false // Suspicious Person is currently disabled for a complete rewrite. It will be back in a future update. //
OfficerDown = true
DerangedDrunkenFeller = false // Disabled for a complete rewrite
DeadBirdOnTheRoad = true
KnifeAttack = true
LSIAAirportIncident = true
IndecentExposure = true
IllegalHuntingBlaineCounty = true
SpectrumAlertFlorida = true
LostDog = true
SoveriegnCitizen = true
NakedDrugUser = true // This is a beta/test callout. This will not be in the final build.
TrafficAccident = true
RoadDebris = true
DisabledVehicle = true
TrafficLightOut = true
ToplessBeachgoer = true // This is a beta/test callout. This will not be in the final build.

------------------------------

[Recommended Mods To Use with this callout pack]

Basic Parking Enforcement by ThruZZd: https://www.lcpdfr.com/downloads/gta5mods/scripts/33307-basicparkingenforcement-go-get-em-paydisplay-tickets-more/
Signs & Barricades DLC by PNWParksFan (HIGHLY Recommended): https://www.lcpdfr.com/downloads/gta5mods/misc/31234-traffic-signs-and-barricades-pack-lml-or-dlc-fivem-ready/
PoliceTape by PNWParksFan (HIGHLY Recommended): https://www.lcpdfr.com/downloads/gta5mods/scripts/19488-police-tape/
AI: Respond by VeteranFighter: https://www.lcpdfr.com/downloads/gta5mods/scripts/44660-airespond/
Vehicle Push by Faya: https://www.lcpdfr.com/downloads/gta5mods/scripts/39798-vehicle-push-push-any-vehicle-at-any-time/

----------------------------

[Socials] 

 //Twitch: https://www.twitch.tv/jonjongamesyt
 //YouTube: https://www.youtube.com/@DiverseGamerHubOfficial
 //GTA Mods: https://www.gta5-mods.com/users/JonJonGames
 //Discord: https://discord.gg/N9KgZx4KUn
 //LSPDFR Profile: https://www.lcpdfr.com/profile/534047-jonjongamesofficial/

 -----------------------------

[Note From Developer]

 Make sure to have a Open Interiors mod (Only need 1 interiors mod) and Policing Redefined Installed. Some callouts may require those mods. 

 Link to Enable All Interiors: https://www.gta5-mods.com/scripts/enable-all-interiors-wip
 Link to InteriorsV (Highly Recommended): https://www.gta5-mods.com/scripts/interiorsv-scripthookv
 Link to Policing Redefined (Highly Recommended): https://www.lcpdfr.com/downloads/gta5mods/scripts/52191-policing-redefined/
");


            }
        }

        private static void CreateConfig()
        {
            config = new InitializationFile(ConfigFilePath);
            config.Create();
            
            // Settings
            EnableLogs = config.ReadBoolean("Settings", "EnableLogs", false);
            HelpMessages = config.ReadBoolean("Settings", "HelpMessages", true);
            MissionMessages = config.ReadBoolean("Settings", "MissionMessages", true);
            
            // Callouts
            VehicleBlockingSidewalk = config.ReadBoolean("Callouts", "VehicleBlockingSidewalk", true);
            BicyclePursuit = config.ReadBoolean("Callouts", "BicyclePursuit", true);
            PersonCarryingAConcealedWeapon = config.ReadBoolean("Callouts", "PersonCarryingAConcealedWeapon", true);
            Loitering = config.ReadBoolean("Callouts", "Loitering", true);
            VehicleBlockingDriveway = config.ReadBoolean("Callouts", "VehicleParkingInHandicap", true);
            VehicleBlockingCrosswalk = config.ReadBoolean("Callouts", "VehicleBlockingCrosswalk", true);
            BicycleBlockingRoadway = config.ReadBoolean("Callouts", "BicycleBlockingRoadway", true);
            SuspiciousVehicle = config.ReadBoolean("Callouts", "SuspiciousVehicle", true);
            AbandonedVehicle = config.ReadBoolean("Callouts", "AbandonedVehicle", true);
            DrugsFound = config.ReadBoolean("Callouts", "DrugsFound", true);
            SuspiciousPerson = config.ReadBoolean("Callouts", "SuspiciousPerson", false);
            OfficerDown = config.ReadBoolean("Callouts", "OfficerDown", true);
            DerangedDrunkenFeller = config.ReadBoolean("Callouts", "DerangedDrunkenFeller", false);
            DeadBirdOnTheRoad = config.ReadBoolean("Callouts", "DeadBirdOnTheRoad", true);
            KnifeAttack = config.ReadBoolean("Callouts", "KnifeAttack", true);
            LSIAAirportIncident = config.ReadBoolean("Callouts", "LSIAAirportIncident", true);
            IndecentExposure = config.ReadBoolean("Callouts", "IndecentExposure", true);
            IllegalHuntingBlaineCounty = config.ReadBoolean("Callouts", "IllegalHuntingBlaineCounty", true);
            LostDogCallout = config.ReadBoolean("Callouts", "LostDog", true);
            SpectrumAlertFlorida = config.ReadBoolean("Callouts", "SpectrumAlertFlorida", true);
            SoveriegnCitizen = config.ReadBoolean("Callouts", "SoveriegnCitizen", true);
            NakedDrugUser = config.ReadBoolean("Callouts", "NakedDrugUser", true);
            TrafficAccident = config.ReadBoolean("Callouts", "TrafficAccident", true);
            RoadDebris = config.ReadBoolean("Callouts", "RoadDebris", true);
            DisabledVehicle = config.ReadBoolean("Callouts", "DisabledVehicle", true);
            TrafficLightOut = config.ReadBoolean("Callouts", "TrafficLightOut", true);
            ToplessBeachgoer = config.ReadBoolean("Callouts", "ToplessBeachgoer", true);
            
            // Keys
            EndCall = config.ReadEnum<Keys>("Keys", "EndCall", Keys.End);
            Dialog = config.ReadEnum<Keys>("Keys", "Dialog", Keys.Y);
            PickUp = config.ReadEnum<Keys>("Keys", "PickUp", Keys.E);
            CallAnimalControlKey = config.ReadEnum<Keys>("Keys", "CallAnimalControlKey", Keys.NumPad1);
            CallAmbulanceKey = config.ReadEnum<Keys>("Keys", "CallAmbulanceKey", Keys.K);
            RequestVehicleInfo = config.ReadEnum<Keys>("Keys", "RequestVehicleInfo", Keys.P);
            RequestTowTruck = config.ReadEnum<Keys>("Keys", "RequestTowTruck", Keys.L);
            RequestCode2BackUp = config.ReadEnum<Keys>("Keys", "RequestCode2BackUp", Keys.NumPad1);
            RequestCode3BackUp = config.ReadEnum<Keys>("Keys", "RequestCode3BackUp", Keys.NumPad2);
            RequestPanicBackUp = config.ReadEnum<Keys>("Keys", "RequestPanicBackUp", Keys.NumPad0);
            RequestK9Unit = config.ReadEnum<Keys>("Keys", "RequestK9Unit", Keys.NumPad3);

            // Traffic settings
            var radiusStr = config.ReadString("Traffic", "StopRadius", "60");
            if (!float.TryParse(radiusStr, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out var radius))
            {
                radius = 60f;
            }
            TrafficStopRadius = radius;

            var densityStr = config.ReadString("Traffic", "DensityMultiplier", "0");
            if (!float.TryParse(densityStr, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out var density))
            {
                density = 0f;
            }
            TrafficDensityMultiplier = density;

            var restoreStr = config.ReadString("Traffic", "RestoreMultiplier", "1");
            if (!float.TryParse(restoreStr, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out var restore))
            {
                restore = 1f;
            }
            TrafficRestoreMultiplier = restore;
        }
    }
}
