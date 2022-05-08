﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.DataLib
{
    [Index("Timestamp", Name = "IX_Timestamp")]
    public partial class SystemLog
    {
        [Key]
        public Guid SystemLogId { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime Timestamp { get; set; }
    }
}