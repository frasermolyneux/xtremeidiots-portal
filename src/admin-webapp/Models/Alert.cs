﻿namespace XtremeIdiots.Portal.AdminWebApp.Models
{
    public class Alert
    {
        public string Message;
        public string Type;

        public Alert(string message, string type)
        {
            Message = message;
            Type = type;
        }
    }
}