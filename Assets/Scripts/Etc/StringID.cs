using UnityEngine;

public enum StringID
{
    None = 0,
    
    Yes = 101,
    No,
    ConfirmRestartStage = 201,
    ConfirmExitStage,
    
    Option = 251,
    Sound,
    Sensitivity,
    Frame,
    Quality,
    Language,
    Frame30,
    Frame60,
    FrameMax,
    QualityLow,
    QualityMedium,
    QualityHigh,
    LangEnglish,
    LangKorean,
    
    HqInfo = 301,
    Hp,
    Shield,
    Entity,
    MenuSelect,
    LaneSelect,
    SlotSelect,
    EntitySelect,
    EntityStat,
    Top,
    Mid,
    Bottom,
    Slot,
    Lv,
    Producing,
    Produce,
    EntityUpgrade,
    HqUpgrade,
    ConfirmEntitySelect,
    StopProducing,
    ConfirmStopProducing,
    NowProducingEntity,
    
    // type tag
    Melee = 9001,
    Range,
    
    // combat role tag
    MainDps = 9101,
    MainTank,
    
    // Entity Name
    Pioneer = 10001,
    Alien,
}
