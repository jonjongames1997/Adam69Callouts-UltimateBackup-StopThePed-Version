using System.Globalization;

namespace Adam69Callouts
{
    internal static class Settings
    {
        internal static bool VehicleBlockingSidewalk = true;
        internal static bool BicyclePursuit = true;
        internal static bool PersonCarryingAConcealedWeapon = true;
        internal static bool Loitering = true;
        internal static bool VehicleBlockingCrosswalk = true;
        internal static bool BicycleBlockingRoadway = true;
        internal static bool SuspiciousVehicle = true;
        internal static bool AbandonedVehicle = true;
        internal static bool DrugsFound = true;
        internal static bool SuspiciousPerson { get; set; } = false; // Default to false for complete rewrite of the callout
        internal static bool OfficerDown = true;
        internal static bool DerangedDrunkenFeller = false; // Disabled for a complete rewrite
        internal static bool DeadBirdOnTheRoad = true;
        internal static bool KnifeAttack = true;
        internal static bool HelpMessages { get; set; }
        internal static bool MissionMessages { get; set; }
        internal static Keys EndCall { get; set; } = Keys.End;
        internal static Keys Dialog { get; set; } = Keys.Y;
        internal static Keys PickUp { get; set; } = Keys.E;
        internal static Keys CallAnimalControlKey { get; set; } = Keys.K;
        internal static Keys CallAmbulanceKey { get; set; } = Keys.NumPad1;
        internal static Keys RequestVehicleInfo { get; set; } = Keys.P;
        internal static Keys RequestTowTruck { get; set; } = Keys.L;
        public static bool EnableLogs { get; set; }
        internal static bool LSIAAirportIncident { get; set; } = true;
        internal static bool IndecentExposure { get; set; } = true;
        internal static bool IllegalHuntingBlaineCounty { get; set; } = true;
        internal static bool LostDogCallout { get; set; } = true;
        internal static bool SpectrumAlertFlorida { get; set; } = true;
        internal static bool SoveriegnCitizen { get; set; } = true;
        internal static bool TrafficAccident { get; set; } = true;
        internal static bool RoadDebris { get; set; } = true;
        internal static bool DisabledVehicle { get; set; } = true;
        internal static bool TrafficLightOut { get; set; } = true;
        internal static Keys RequestCode2BackUp { get; set; } = Keys.NumPad1;
        internal static Keys RequestCode3BackUp { get; set; } = Keys.NumPad2;
        internal static Keys RequestPanicBackUp { get; set; } = Keys.NumPad0;
        internal static Keys RequestK9Unit { get; set; } = Keys.NumPad3;
        internal static bool ToplessBeachgoer { get; set; } = true;

        // Traffic settings (configurable via INI)
        internal static float TrafficStopRadius { get; set; } = 60f; // meters
        internal static float TrafficDensityMultiplier { get; set; } = 0f; //0 = no traffic,1 = normal
        internal static float TrafficRestoreMultiplier { get; set; } = 1f; // value to restore to when callout ends

        internal static void LoadSettings()
        {
            Game.Console.Print("[LOG]: Loading config file from Adam69 Callouts");
            InitializationFile initializationFile = new("Plugins\\LSPDFR\\Adam69Callouts\\Adam69Callouts.ini");
            initializationFile.Create();
            Game.LogTrivial("Initializing config for Adam69 Callouts....");
            Settings.VehicleBlockingSidewalk = initializationFile.ReadBoolean("Callouts", "VehicleBlockingSidewalk", true);
            Settings.BicyclePursuit = initializationFile.ReadBoolean("Callouts", "BicyclePursuit", true);
            Settings.PersonCarryingAConcealedWeapon = initializationFile.ReadBoolean("Callouts", "PersonCarryingAConcealedWeapon", true);
            Settings.Loitering = initializationFile.ReadBoolean("Callouts", "Loitering", true);
            Settings.VehicleBlockingCrosswalk = initializationFile.ReadBoolean("Callouts", "VehicleBlockingCrosswalk", true);
            Settings.BicycleBlockingRoadway = initializationFile.ReadBoolean("Callouts", "BicycleBlockingRoadway", true);
            Settings.SuspiciousVehicle = initializationFile.ReadBoolean("Callouts", "SuspiciousVehicle", true);
            Settings.AbandonedVehicle = initializationFile.ReadBoolean("Callouts", "AbandonedVehicle", true);
            Settings.DrugsFound = initializationFile.ReadBoolean("Callouts", "DrugsFound", true);
            Settings.SuspiciousPerson = initializationFile.ReadBoolean("Callouts", "SuspiciousPerson", false);
            Settings.OfficerDown = initializationFile.ReadBoolean("Callouts", "OfficerDown", true);
            Settings.DerangedDrunkenFeller = initializationFile.ReadBoolean("Callouts", "DerangedDrunkenFeller", false);
            Settings.DeadBirdOnTheRoad = initializationFile.ReadBoolean("Callouts", "DeadBirdOnTheRoad", true);
            Settings.KnifeAttack = initializationFile.ReadBoolean("Callouts", "KnifeAttack", true);
            Settings.DisabledVehicle = initializationFile.ReadBoolean("Callouts", "DisabledVehicle", true);
            Settings.IllegalHuntingBlaineCounty = initializationFile.ReadBoolean("Callouts", "IllegalHuntingBlaineCounty", true);
            Settings.IndecentExposure = initializationFile.ReadBoolean("Callouts", "IndecentExposure", true);
            Settings.LostDogCallout = initializationFile.ReadBoolean("Callouts", "LostDog", true);
            Settings.LSIAAirportIncident = initializationFile.ReadBoolean("Callouts", "LSIAAirportIncident", true);
            Settings.RoadDebris = initializationFile.ReadBoolean("Callouts", "RoadDebris", true);
            Settings.SoveriegnCitizen = initializationFile.ReadBoolean("Callouts", "SoveriegnCitizen", true);
            Settings.SpectrumAlertFlorida = initializationFile.ReadBoolean("Callouts", "SpectrumAlertFlorida", true);
            Settings.TrafficAccident = initializationFile.ReadBoolean("Callouts", "TrafficAccident", true);
            Settings.TrafficLightOut = initializationFile.ReadBoolean("Callouts", "TrafficLightOut", true);
            HelpMessages = initializationFile.ReadBoolean("Settings", "HelpMessages", true);
            MissionMessages = initializationFile.ReadBoolean("Settings", "MissionMessages", true);
            EndCall = initializationFile.ReadEnum<Keys>("Keys", "EndCall", Keys.End);
            Dialog = initializationFile.ReadEnum<Keys>("Keys", "Dialog", Keys.Y);
            PickUp = initializationFile.ReadEnum<Keys>("Keys", "PickUp", Keys.E);
            RequestVehicleInfo = initializationFile.ReadEnum<Keys>("Keys", "RequestVehicleInfo", Keys.P);
            CallAnimalControlKey = initializationFile.ReadEnum<Keys>("Keys", "CallAnimalControlKey", Keys.NumPad1);
            CallAmbulanceKey = initializationFile.ReadEnum<Keys>("Keys", "CallAmbulanceKey", Keys.K);
            RequestTowTruck = initializationFile.ReadEnum<Keys>("Keys", "RequestTowTruck", Keys.L);
            RequestCode2BackUp = initializationFile.ReadEnum<Keys>("Keys", "RequestCode2BackUp", Keys.NumPad1);
            RequestCode3BackUp = initializationFile.ReadEnum<Keys>("Keys", "RequestCode3BackUp", Keys.NumPad2);
            RequestPanicBackUp = initializationFile.ReadEnum<Keys>("Keys", "RequestPanicBackUp", Keys.NumPad0);
            RequestK9Unit = initializationFile.ReadEnum<Keys>("Keys", "RequestK9Unit", Keys.NumPad3);
            Settings.ToplessBeachgoer = initializationFile.ReadBoolean("Callouts", "ToplessBeachgoer", true);

            // Read traffic settings (as strings then parse to allow safe parsing)
            var radiusStr = initializationFile.ReadString("Traffic", "StopRadius", Settings.TrafficStopRadius.ToString(CultureInfo.InvariantCulture));
            if (!float.TryParse(radiusStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var radius))
            {
                radius = Settings.TrafficStopRadius;
            }
            TrafficStopRadius = radius;

            var densityStr = initializationFile.ReadString("Traffic", "DensityMultiplier", Settings.TrafficDensityMultiplier.ToString(CultureInfo.InvariantCulture));
            if (!float.TryParse(densityStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var density))
            {
                density = Settings.TrafficDensityMultiplier;
            }
            TrafficDensityMultiplier = density;

            var restoreStr = initializationFile.ReadString("Traffic", "RestoreMultiplier", Settings.TrafficRestoreMultiplier.ToString(CultureInfo.InvariantCulture));
            if (!float.TryParse(restoreStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var restore))
            {
                restore = Settings.TrafficRestoreMultiplier;
            }
            TrafficRestoreMultiplier = restore;
        }

        internal static void SaveConfigSettings()
        {
            var ini = new InitializationFile("Plugins\\LSPDFR\\Adam69Callouts\\Adam69Callouts.ini");
            ini.Create();
            ini.Write("Callouts", "VehicleBlockingSidewalk", true);
            ini.Write("Callouts", "BicyclePursuit", true);
            ini.Write("Callouts", "PersonCarryingAConcealedWeapon", true);
            ini.Write("Callouts", "Loitering", true);
            ini.Write("Callouts", "VehicleBlockingCrosswalk", true);
            ini.Write("Callouts", "BicycleBlockingRoadway", true);
            ini.Write("Callouts", "SuspiciousVehicle", true);
            ini.Write("Callouts", "AbandonedVehicle", true);
            ini.Write("Callouts", "DrugsFound", true);
            ini.Write("Callouts", "SuspiciousPerson", false);
            ini.Write("Callouts", "OfficerDown", true);
            ini.Write("Callouts", "DerangedDrunkenFeller", false);
            ini.Write("Callouts", "DeadBirdOnTheRoad", true);
            ini.Write("Callouts", "KnifeAttack", true);
            ini.Write("Settings", "HelpMessages", true);
            ini.Write("Settings", "MissionMessages", true);
            ini.Write("Keys", "EndCall", Keys.End);
            ini.Write("Keys", "Dialog", Keys.Y);
            ini.Write("Keys", "PickUp", Keys.E);
            ini.Write("Keys", "CallAnimalControlKey", Keys.K);
            ini.Write("Keys", "CallAmbulanceKey", Keys.NumPad1);
            ini.Write("Keys", "RequestVehicleInfo", Keys.P);
            ini.Write("Keys", "RequestTowTruck", Keys.L);
            ini.Write("Settings", "EnableLogs", false);
            ini.Write("Callouts", "LSIAAirportIncident", true);
            ini.Write("Callouts", "IndecentExposure", true);
            ini.Write("Callouts", "IllegalHuntingBlaineCounty", true);
            ini.Write("Callouts", "SpectrumAlertFlorida", true);
            ini.Write("Callouts", "LostDog", true);
            ini.Write("Callouts", "SoveriegnCitizen", true);
            ini.Write("Callouts", "TrafficAccident", true);
            ini.Write("Callouts", "RoadDebris", true);
            ini.Write("Callouts", "DisabledVehicle", true);
            ini.Write("Callouts", "TrafficLightOut", true);
            ini.Write("Callouts", "ToplessBeachgoer", true);
            ini.Write("Keys", "RequestCode2BackUp", Keys.NumPad1);
            ini.Write("Keys", "RequestCode3BackUp", Keys.NumPad2);
            ini.Write("Keys", "RequestPanicBackUp", Keys.NumPad0);
            ini.Write("Keys", "RequestK9Unit", Keys.NumPad3);

            // Traffic settings
            ini.Write("Traffic", "StopRadius", TrafficStopRadius);
            ini.Write("Traffic", "DensityMultiplier", TrafficDensityMultiplier);
            ini.Write("Traffic", "RestoreMultiplier", TrafficRestoreMultiplier);

            ini.ReCreate();
        }

        public static readonly string PluginVersion = "0.4.7";
    }
}