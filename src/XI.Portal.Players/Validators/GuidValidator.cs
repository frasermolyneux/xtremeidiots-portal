using System;
using System.Text.RegularExpressions;
using XI.CommonTypes;
using XI.Portal.Players.Interfaces;

namespace XI.Portal.Players.Validators
{
    public class GuidValidator : IGuidValidator
    {
        public bool IsValid(GameType gameType, string guid)
        {
            string regex;
            switch (gameType)
            {
                case GameType.CallOfDuty2:
                    regex = @"^([a-z0-9]{4,32})$";
                    break;
                case GameType.CallOfDuty4:
                    regex = @"^([a-z0-9]{32})$";
                    break;
                case GameType.CallOfDuty5:
                    regex = @"^([a-z0-9]{4,32})$";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), "Game type is unsupported");
            }

            return Regex.Match(guid, regex).Success;
        }
    }
}