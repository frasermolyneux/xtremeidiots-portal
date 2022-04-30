﻿using System;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class IpAddressDto
    {
        public string Address { get; set; }
        public DateTime Added { get; set; }
        public DateTime LastUsed { get; set; }
    }
}