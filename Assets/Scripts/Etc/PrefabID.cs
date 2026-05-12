using UnityEngine;

public enum PrefabID
{
    None = 0,
    
    GameField = 50,
    HeadQuarter = 51,
    
    // Entity
    SingleTargetEntity = 1001,
    AreaTargetEntity = 1002,
    
    PioneerSingle = 2001,
    PioneerArea,
    Police = 2011,
    Phalanx,
    Security,
    Agent,
    SpecialForce,
    Breacher,
    AssaultRifle,
    Commander,
    HydroPuncher,
    ShadowReaper,
    TheThunder,
    AegisStorm,
    ElectroBlade,
    PlasmaSlasher,
    BloodyCanon,
    SonicDisruptor,
    
    RevoltSingle = 2501,
    RevoltArea,
    SteelBaton = 2511,
    WireStriker,
    MasterGang,
    MadGas,
    UnknownSamurai,
    SteelWall,
    SharpShooter,
    DroneBomber,
    ChainsawBot,
    PyroManiac,
    CentauriCanon,
    MultiLauncher,
    Whiplash,
    Overload,
    VoidBullet,
    DestroyLaser,
    
    // spawner
    EntitySpawner = 5001,
    
    
    // UI
    UITopHUDPanel = 10001,
    UIConfirm,
    UIOption,
    UIHqManagement,
    UISlotUnit,
    UIEntityUnit,
    UIPauseBtn,
    UINotice,
    UISlotUpgradeUnit,
    
    UIEntityStat = 10011,
    UIResult,
}
