using System.ComponentModel.DataAnnotations;

namespace XI.CommonTypes
{
    public enum GameType
    {
        Unknown = 0,
        [Display(Name = "Call of Duty 2")] CallOfDuty2 = 1,
        [Display(Name = "Call of Duty 4")] CallOfDuty4 = 2,
        [Display(Name = "Call of Duty 5")] CallOfDuty5 = 3,
        Insurgency = 4,

        [Display(Name = "Ark Survival Evolved")]
        ArkSurvivalEvolved = 5,
        [Display(Name = "Battlefield 1")] Battlefield1 = 6,
        [Display(Name = "Battlefield 2")] Battlefield3 = 7,
        [Display(Name = "Battlefield 4")] Battlefield4 = 8,
        [Display(Name = "Battlefield 5")] Battlefield5 = 9,

        [Display(Name = "Battlefield Bad Company 2")]
        BattlefieldBadCompany2 = 10,
        [Display(Name = "Crysis Wars")] CrysisWars = 11,
        [Display(Name = "Left 4 Dead 2")] Left4Dead2 = 12,
        Minecraft = 13,

        [Display(Name = "PLAYERUNKNOWNS BATTLEGROUND")]
        PlayerUnknownsBattleground = 14,

        [Display(Name = "Rising Storm Vietnam")]
        RisingStormVietnam = 15,
        Rust = 16,
        [Display(Name = "War Thunder")] WarThunder = 17,
        [Display(Name = "World Of Warships")] WorldOfWarships = 18,
        [Display(Name = "World War 3")] WorldWar3 = 19,
        [Display(Name = "Unreal Tournament")] UnrealTournament2004 = 20,
        [Display(Name = "ARMA")] Arma = 21,
        [Display(Name = "ARMA 2")] Arma2 = 22,
        [Display(Name = "ARMA 3")] Arma3 = 23
    }
}