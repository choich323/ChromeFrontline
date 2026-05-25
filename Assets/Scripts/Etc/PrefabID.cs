using UnityEngine;

public enum PrefabID
{
    None = 0,
    
    GameField = 50,
    HeadQuarter = 51,
    
    // Entity
    EntitySingle = 1001,
    EntityArea,
    EntitySweep,
    EntityPierce,
    
    Police = 2001,
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
    VoidBullet,
    SonicDisruptor,
    
    SteelBaton = 2501,
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
    ChimeraBeast,
    Overload,
    BloodyCanon,
    DestroyLaser,
    
    // spawner
    EntitySpawner = 5001,
    
    
    // UI
    UIHUDPanel = 10001,
    UIConfirm,
    UIOption,
    UIHqManagement,
    UISlotUnit,
    UIEntityUnit,
    UIPlayBtnGroup,
    UINotice,
    UISlotUpgradeUnit,
    
    UIEntityStat = 10011,
    UIResult,
}
