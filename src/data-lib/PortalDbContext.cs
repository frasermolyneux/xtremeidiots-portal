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
        public virtual DbSet<BanFileMonitor> BanFileMonitors { get; set; }
        public virtual DbSet<ChatMessage> ChatMessages { get; set; }
        public virtual DbSet<Demo> Demoes { get; set; }
        public virtual DbSet<GameServer> GameServers { get; set; }
        public virtual DbSet<GameServerEvent> GameServerEvents { get; set; }
        public virtual DbSet<GameServerStat> GameServerStats { get; set; }
        public virtual DbSet<LivePlayer> LivePlayers { get; set; }
        public virtual DbSet<Map> Maps { get; set; }
        public virtual DbSet<MapVote> MapVotes { get; set; }
        public virtual DbSet<Player> Players { get; set; }
        public virtual DbSet<PlayerAlias> PlayerAliases { get; set; }
        public virtual DbSet<PlayerIpAddress> PlayerIpAddresses { get; set; }
        public virtual DbSet<RecentPlayer> RecentPlayers { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<UserProfile> UserProfiles { get; set; }
        public virtual DbSet<UserProfileClaim> UserProfileClaims { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Scaffolding:ConnectionString", "Data Source=(local);Initial Catalog=database;Integrated Security=true");

            modelBuilder.Entity<AdminAction>(entity =>
            {
                entity.Property(e => e.AdminActionId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.AdminActions)
                    .HasForeignKey(d => d.PlayerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.AdminActions_dbo.Players_PlayerId");

                entity.HasOne(d => d.UserProfile)
                    .WithMany(p => p.AdminActions)
                    .HasForeignKey(d => d.UserProfileId)
                    .HasConstraintName("FK_dbo.AdminActions_dbo.UserProfiles_UserProfileId");
            });

            modelBuilder.Entity<BanFileMonitor>(entity =>
            {
                entity.Property(e => e.BanFileMonitorId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.GameServer)
                    .WithMany(p => p.BanFileMonitors)
                    .HasForeignKey(d => d.GameServerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.BanFileMonitors_dbo.GameServers_GameServerId");
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.Property(e => e.ChatMessageId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.GameServer)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.GameServerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.ChatMessages_dbo.GameServers_GameServerId");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.PlayerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.ChatMessages_dbo.Players_PlayerId");
            });

            modelBuilder.Entity<Demo>(entity =>
            {
                entity.Property(e => e.DemoId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.UserProfile)
                    .WithMany(p => p.Demos)
                    .HasForeignKey(d => d.UserProfileId)
                    .HasConstraintName("FK_dbo.Demoes_dbo.UserProfiles_Id");
            });

            modelBuilder.Entity<GameServer>(entity =>
            {
                entity.Property(e => e.GameServerId).HasDefaultValueSql("newsequentialid()");

                entity.Property(e => e.FtpPort).HasDefaultValueSql("21");

                entity.Property(e => e.LiveLastUpdated).HasDefaultValueSql("'1900-01-01t00:00:00.000'");
            });

            modelBuilder.Entity<GameServerEvent>(entity =>
            {
                entity.Property(e => e.GameServerEventId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.GameServer)
                    .WithMany(p => p.GameServerEvents)
                    .HasForeignKey(d => d.GameServerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GameServerEvents_GameServer");
            });

            modelBuilder.Entity<GameServerStat>(entity =>
            {
                entity.Property(e => e.GameServerStatId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.GameServer)
                    .WithMany(p => p.GameServerStats)
                    .HasForeignKey(d => d.GameServerId)
                    .HasConstraintName("FK_GameServerStats_GameServer");
            });

            modelBuilder.Entity<LivePlayer>(entity =>
            {
                entity.Property(e => e.LivePlayerId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.GameServer)
                    .WithMany(p => p.LivePlayers)
                    .HasForeignKey(d => d.GameServerId)
                    .HasConstraintName("FK_dbo.LivePlayers_dbo.GameServers_GameServer_GameServerId");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.LivePlayers)
                    .HasForeignKey(d => d.PlayerId)
                    .HasConstraintName("FK_dbo.LivePlayers_dbo.Players_PlayerId");
            });

            modelBuilder.Entity<Map>(entity =>
            {
                entity.Property(e => e.MapId).HasDefaultValueSql("newsequentialid()");
            });

            modelBuilder.Entity<MapVote>(entity =>
            {
                entity.Property(e => e.MapVoteId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.Map)
                    .WithMany(p => p.MapVotes)
                    .HasForeignKey(d => d.MapId)
                    .HasConstraintName("FK_dbo.MapVotes_dbo.Maps_Map_MapId");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.MapVotes)
                    .HasForeignKey(d => d.PlayerId)
                    .HasConstraintName("FK_dbo.MapVotes_dbo.Player2_Player_PlayerId");
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.Property(e => e.PlayerId).HasDefaultValueSql("newsequentialid()");
            });

            modelBuilder.Entity<PlayerAlias>(entity =>
            {
                entity.Property(e => e.PlayerAliasId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.PlayerAliases)
                    .HasForeignKey(d => d.PlayerId)
                    .HasConstraintName("FK_dbo.PlayerAlias_dbo.Players_PlayerId");
            });

            modelBuilder.Entity<PlayerIpAddress>(entity =>
            {
                entity.Property(e => e.PlayerIpAddressId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.PlayerIpAddresses)
                    .HasForeignKey(d => d.PlayerId)
                    .HasConstraintName("FK_dbo.PlayerIpAddresses_dbo.Players_PlayerId");
            });

            modelBuilder.Entity<RecentPlayer>(entity =>
            {
                entity.Property(e => e.RecentPlayerId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.GameServer)
                    .WithMany(p => p.RecentPlayers)
                    .HasForeignKey(d => d.GameServerId)
                    .HasConstraintName("FK_dbo.RecentPlayers_dbo.GameServers_GameServerId");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.RecentPlayers)
                    .HasForeignKey(d => d.PlayerId)
                    .HasConstraintName("FK_dbo.RecentPlayers_dbo.Players_PlayerId");
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.Property(e => e.ReportId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.AdminUserProfile)
                    .WithMany(p => p.ReportAdminUserProfiles)
                    .HasForeignKey(d => d.AdminUserProfileId)
                    .HasConstraintName("FK_dbo.Reports_dbo.AdminUserProfiles_Id");

                entity.HasOne(d => d.GameServer)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.GameServerId)
                    .HasConstraintName("FK_dbo.Reports_dbo.GameServers_GameServerId");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.PlayerId)
                    .HasConstraintName("FK_dbo.Reports_dbo.Players_PlayerId");

                entity.HasOne(d => d.UserProfile)
                    .WithMany(p => p.ReportUserProfiles)
                    .HasForeignKey(d => d.UserProfileId)
                    .HasConstraintName("FK_dbo.Reports_dbo.UserProfiles_Id");
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.Property(e => e.UserProfileId).HasDefaultValueSql("newsequentialid()");
            });

            modelBuilder.Entity<UserProfileClaim>(entity =>
            {
                entity.Property(e => e.UserProfileClaimId).HasDefaultValueSql("newsequentialid()");

                entity.HasOne(d => d.UserProfile)
                    .WithMany(p => p.UserProfileClaims)
                    .HasForeignKey(d => d.UserProfileId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.UserProfileClaims_dbo.UserProfiles_Id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}