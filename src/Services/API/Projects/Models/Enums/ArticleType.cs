using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projects.Models.Enums;

public enum ArticleType
{
    None,
    
    // Power Distribution
    CablesAndWires,
    CircuitBreakers,
    DistributionBoards,
    Transformers,

    // Lighting
    LightFixtures,
    LightSwitches,
    EmergencyLighting,

    // Power Outlets and Accessories
    PowerSockets,
    ExtensionBoxes,

    // Conduits and Cable Management
    Conduits,
    CableTrays,
    CableGlands,
    CableTiesAndAccessories,

    // Safety and Protection
    SurgeProtectors,
    GroundingAndBondingEquipment,
    SmokeDetectorsAndFireAlarms,

    // Automation and Control
    Relays,
    Timers,
    Sensors,
    Controllers,

    // Renewable Energy Systems
    SolarPanels,
    Inverters,
    EnergyStorageSystems,

    // Communication Systems
    IntercomSystems,
    NetworkingEquipment,
    TelephoneSystems,

    // Heating, Ventilation, and Air Conditioning (HVAC)
    Thermostats,
    Fans,
    HVACControls,

    // Specialized Electrical Equipment
    ElectricVehicleChargingStations,
    SecuritySystems,
    UPSAndBackupPowerSupplies,

    // Miscellaneous
    ToolsAndTestingEquipment,
    DecorativeLighting
}
