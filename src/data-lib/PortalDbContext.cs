﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace XtremeIdiots.Portal.DataLib
{
    public partial class PortalDbContext : DbContext
    {
        public PortalDbContext()
        {
        }

        public PortalDbContext(DbContextOptions<PortalDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AdminAction> AdminActions { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<BanFileMonitor> BanFileMonitors { get; set; }
        public virtual DbSet<ChatLog> ChatLogs { get; set; }
        public virtual DbSet<Demo> Demoes { get; set; }
        public virtual DbSet<DemoAuthKey> DemoAuthKeys { get; set; }
        public virtual DbSet<FileMonitor> FileMonitors { get; set; }
        public virtual DbSet<GameServer> GameServers { get; set; }
        public virtual DbSet<GameServerEvent> GameServerEvents { get; set; }
        public virtual DbSet<GameServerMap> GameServerMaps { get; set; }
        public virtual DbSet<GameServerStat> GameServerStats { get; set; }
        public virtual DbSet<LivePlayer> LivePlayers { get; set; }
        public virtual DbSet<LivePlayerLocation> LivePlayerLocations { get; set; }
        public virtual DbSet<Map> Maps { get; set; }
        public virtual DbSet<MapVote> MapVotes { get; set; }
        public virtual DbSet<Player2> Player2s { get; set; }
        public virtual DbSet<PlayerAlias> PlayerAliases { get; set; }
        public virtual DbSet<PlayerIpAddress> PlayerIpAddresses { get; set; }
        public virtual DbSet<RconMonitor> RconMonitors { get; set; }
        public virtual DbSet<SystemLog> SystemLogs { get; set; }
        public virtual DbSet<UserLog> UserLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Scaffolding:ConnectionString", "Data Source=(local);Initial Catalog=database;Integrated Security=true");

            modelBuilder.Entity<AdminAction>(entity =>
            {
                entity.Property(e => e.AdminActionId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.Admin)
                    .WithMany(p => p.AdminActions)
                    .HasForeignKey(d => d.AdminId)
                    .HasConstraintName("FK_dbo.AdminActions_dbo.AspNetUsers_Admin_Id");

                entity.HasOne(d => d.PlayerPlayer)
                    .WithMany(p => p.AdminActions)
                    .HasForeignKey(d => d.PlayerPlayerId)
                    .HasConstraintName("FK_dbo.AdminActions_dbo.Player2_Player_PlayerId");
            });

            modelBuilder.Entity<AspNetUser>(entity =>
            {
                entity.HasMany(d => d.Roles)
                    .WithMany(p => p.Users)
                    .UsingEntity<Dictionary<string, object>>(
                        "AspNetUserRole",
                        l => l.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId").HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId"),
                        r => r.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId").HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId"),
                        j =>
                        {
                            j.HasKey("UserId", "RoleId").HasName("PK_dbo.AspNetUserRoles");

                            j.ToTable("AspNetUserRoles");

                            j.HasIndex(new[] { "RoleId" }, "IX_RoleId");

                            j.HasIndex(new[] { "UserId" }, "IX_UserId");

                            j.IndexerProperty<string>("UserId").HasMaxLength(128);

                            j.IndexerProperty<string>("RoleId").HasMaxLength(128);
                        });
            });

            modelBuilder.Entity<AspNetUserClaim>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId");
            });

            modelBuilder.Entity<AspNetUserLogin>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey, e.UserId })
                    .HasName("PK_dbo.AspNetUserLogins");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId");
            });

            modelBuilder.Entity<BanFileMonitor>(entity =>
            {
                entity.Property(e => e.BanFileMonitorId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.GameServerServer)
                    .WithMany(p => p.BanFileMonitors)
                    .HasForeignKey(d => d.GameServerServerId)
                    .HasConstraintName("FK_dbo.BanFileMonitors_dbo.GameServers_GameServer_ServerId");
            });

            modelBuilder.Entity<ChatLog>(entity =>
            {
                entity.Property(e => e.ChatLogId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.GameServerServer)
                    .WithMany(p => p.ChatLogs)
                    .HasForeignKey(d => d.GameServerServerId)
                    .HasConstraintName("FK_dbo.ChatLogs_dbo.GameServers_GameServer_ServerId");

                entity.HasOne(d => d.PlayerPlayer)
                    .WithMany(p => p.ChatLogs)
                    .HasForeignKey(d => d.PlayerPlayerId)
                    .HasConstraintName("FK_dbo.ChatLogs_dbo.Player2_Player_PlayerId");
            });

            modelBuilder.Entity<Demo>(entity =>
            {
                entity.Property(e => e.DemoId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Demos)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.Demoes_dbo.AspNetUsers_User_Id");
            });

            modelBuilder.Entity<FileMonitor>(entity =>
            {
                entity.Property(e => e.FileMonitorId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.GameServerServer)
                    .WithMany(p => p.FileMonitors)
                    .HasForeignKey(d => d.GameServerServerId)
                    .HasConstraintName("FK_dbo.FileMonitors_dbo.GameServers_GameServer_ServerId");
            });

            modelBuilder.Entity<GameServer>(entity =>
            {
                entity.HasKey(e => e.ServerId)
                    .HasName("PK_dbo.GameServers");

                entity.Property(e => e.ServerId).HasDefaultValueSql("newsequentialid()");

                entity.Property(e => e.LiveLastUpdated).HasDefaultValueSql("'1900-01-01t00:00:00.000'");
            });

            modelBuilder.Entity<GameServerEvent>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.GameServer)
                    .WithMany(p => p.GameServerEvents)
                    .HasForeignKey(d => d.GameServerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GameServerEvents_GameServer");
            });

            modelBuilder.Entity<GameServerMap>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.GameServer)
                    .WithMany(p => p.GameServerMaps)
                    .HasForeignKey(d => d.GameServerId)
                    .HasConstraintName("FK_GameServerMaps_GameServer");

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.GameServerMaps)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("FK_GameServerMaps_Map");
            });

            modelBuilder.Entity<GameServerStat>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.GameServer)
                    .WithMany(p => p.GameServerStats)
                    .HasForeignKey(d => d.GameServerId)
                    .HasConstraintName("FK_GameServerStats_GameServer");
            });

            modelBuilder.Entity<LivePlayer>(entity =>
            {
                entity.Property(e => e.LivePlayerId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.GameServerServer)
                    .WithMany(p => p.LivePlayers)
                    .HasForeignKey(d => d.GameServerServerId)
                    .HasConstraintName("FK_dbo.LivePlayers_dbo.GameServers_GameServer_ServerId");
            });

            modelBuilder.Entity<LivePlayerLocation>(entity =>
            {
                entity.Property(e => e.LivePlayerLocationId).HasDefaultValueSql("newsequentialid()");
            });

            modelBuilder.Entity<Map>(entity =>
            {
                entity.Property(e => e.MapId).HasDefaultValueSql("newsequentialid()");
            });

            modelBuilder.Entity<MapVote>(entity =>
            {
                entity.Property(e => e.MapVoteId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.MapMap)
                    .WithMany(p => p.MapVotes)
                    .HasForeignKey(d => d.MapMapId)
                    .HasConstraintName("FK_dbo.MapVotes_dbo.Maps_Map_MapId");

                entity.HasOne(d => d.PlayerPlayer)
                    .WithMany(p => p.MapVotes)
                    .HasForeignKey(d => d.PlayerPlayerId)
                    .HasConstraintName("FK_dbo.MapVotes_dbo.Player2_Player_PlayerId");
            });

            modelBuilder.Entity<Player2>(entity =>
            {
                entity.HasKey(e => e.PlayerId)
                    .HasName("PK_dbo.Player2");

                entity.Property(e => e.PlayerId).HasDefaultValueSql("newsequentialid()");
            });

            modelBuilder.Entity<PlayerAlias>(entity =>
            {
                entity.Property(e => e.PlayerAliasId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.PlayerPlayer)
                    .WithMany(p => p.PlayerAliases)
                    .HasForeignKey(d => d.PlayerPlayerId)
                    .HasConstraintName("FK_dbo.PlayerAlias_dbo.Player2_Player_PlayerId");
            });

            modelBuilder.Entity<PlayerIpAddress>(entity =>
            {
                entity.Property(e => e.PlayerIpAddressId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.PlayerPlayer)
                    .WithMany(p => p.PlayerIpAddresses)
                    .HasForeignKey(d => d.PlayerPlayerId)
                    .HasConstraintName("FK_dbo.PlayerIpAddresses_dbo.Player2_Player_PlayerId");
            });

            modelBuilder.Entity<RconMonitor>(entity =>
            {
                entity.Property(e => e.RconMonitorId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.GameServerServer)
                    .WithMany(p => p.RconMonitors)
                    .HasForeignKey(d => d.GameServerServerId)
                    .HasConstraintName("FK_dbo.RconMonitors_dbo.GameServers_GameServer_ServerId");
            });

            modelBuilder.Entity<SystemLog>(entity =>
            {
                entity.Property(e => e.SystemLogId).HasDefaultValueSql("newsequentialid()");
            });

            modelBuilder.Entity<UserLog>(entity =>
            {
                entity.Property(e => e.UserLogId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.ApplicationUser)
                    .WithMany(p => p.UserLogs)
                    .HasForeignKey(d => d.ApplicationUserId)
                    .HasConstraintName("FK_dbo.UserLogs_dbo.AspNetUsers_ApplicationUser_Id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}