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
    Security,
    SpecialForce,
    AssaultRifle,
    HydroPuncher,
    TheThunder,
    
    RevoltSingle = 2501,
    RevoltArea,
    SteelBaton = 2511,
    MasterGang,
    UnknownSamurai,
    SharpShooter,
    ChainsawBot,
    CentauriCanon,
    
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
    
    UIEntityStat = 10011,
    UIResult,
}
