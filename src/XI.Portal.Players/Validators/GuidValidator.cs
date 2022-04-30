using System;
using System.Text.RegularExpressions;
using XI.Portal.Players.Interfaces;

namespace XI.Portal.Players.Validators
{
    public class GuidValidator : IGuidValidator
    {
        public bool IsValid(string gameType, string guid)
        {
            string regex;
            switch (gameType)
            {
                case "CallOfDuty2":
                    regex = @"^([a-z0-9]{4,32})$";
                    break;
                case "CallOfDuty4":
                    regex = @"^([a-z0-9]{32})$";
                    break;
                case "CallOfDuty5":
                    regex = @"^([a-z0-9]{4,32})$";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameType), "Game type is unsupported");
            }

            return Regex.Match(guid, regex).Success;
        }
    }
}