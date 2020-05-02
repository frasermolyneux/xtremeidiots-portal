using System;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy.Models;

// ReSharper disable UnusedMember.Global

namespace XI.Portal.Data.Legacy
{
    public class LegacyPortalContext : DbContext
    {
        public LegacyPortalContext()
        {
        }

        public LegacyPortalContext(DbContextOptions<LegacyPortalContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AdminActions> AdminActions { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<BanFileMonitors> BanFileMonitors { get; set; }
        public virtual DbSet<ChatLogs> ChatLogs { get; set; }
        public virtual DbSet<Demoes> Demoes { get; set; }
        public virtual DbSet<FileMonitors> FileMonitors { get; set; }
        public virtual DbSet<GameServers> GameServers { get; set; }
        public virtual DbSet<LivePlayerLocations> LivePlayerLocations { get; set; }
        public virtual DbSet<LivePlayers> LivePlayers { get; set; }
        public virtual DbSet<MapFiles> MapFiles { get; set; }
        public virtual DbSet<MapRotations> MapRotations { get; set; }
        public virtual DbSet<MapVotes> MapVotes { get; set; }
        public virtual DbSet<Maps> Maps { get; set; }
        public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }
        public virtual DbSet<Player2> Player2 { get; set; }
        public virtual DbSet<PlayerAlias> PlayerAlias { get; set; }
        public virtual DbSet<PlayerIpAddresses> PlayerIpAddresses { get; set; }
        public virtual DbSet<RconMonitors> RconMonitors { get; set; }
        public virtual DbSet<SystemLogs> SystemLogs { get; set; }
        public virtual DbSet<UserLogs> UserLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) throw new Exception("LegacyPortalContext has not had options configured!");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdminActions>(entity =>
            {
                entity.HasKey(e => e.AdminActionId)
                    .HasName("PK_dbo.AdminActions");

                entity.HasIndex(e => e.AdminActionId)
                    .HasName("IX_AdminActionId")
                    .IsUnique();

                entity.HasIndex(e => e.AdminId)
                    .HasName("IX_Admin_Id");

                entity.HasIndex(e => e.PlayerPlayerId)
                    .HasName("IX_Player_PlayerId");

                entity.Property(e => e.AdminActionId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.AdminId)
                    .HasColumnName("Admin_Id")
                    .HasMaxLength(128);

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.Expires).HasColumnType("datetime");

                entity.Property(e => e.PlayerPlayerId).HasColumnName("Player_PlayerId");

                entity.HasOne(d => d.Admin)
                    .WithMany(p => p.AdminActions)
                    .HasForeignKey(d => d.AdminId)
                    .HasConstraintName("FK_dbo.AdminActions_dbo.AspNetUsers_Admin_Id");

                entity.HasOne(d => d.PlayerPlayer)
                    .WithMany(p => p.AdminActions)
                    .HasForeignKey(d => d.PlayerPlayerId)
                    .HasConstraintName("FK_dbo.AdminActions_dbo.Player2_Player_PlayerId");
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasName("RoleNameIndex")
                    .IsUnique();

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId");
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new {e.LoginProvider, e.ProviderKey, e.UserId})
                    .HasName("PK_dbo.AspNetUserLogins");

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId");
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new {e.UserId, e.RoleId})
                    .HasName("PK_dbo.AspNetUserRoles");

                entity.HasIndex(e => e.RoleId)
                    .HasName("IX_RoleId");

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.Property(e => e.RoleId).HasMaxLength(128);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId");
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasIndex(e => e.UserName)
                    .HasName("UserNameIndex")
                    .IsUnique();

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.LockoutEndDateUtc).HasColumnType("datetime");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<BanFileMonitors>(entity =>
            {
                entity.HasKey(e => e.BanFileMonitorId)
                    .HasName("PK_dbo.BanFileMonitors");

                entity.HasIndex(e => e.BanFileMonitorId)
                    .HasName("IX_BanFileMonitorId")
                    .IsUnique();

                entity.HasIndex(e => e.GameServerServerId)
                    .HasName("IX_GameServer_ServerId");

                entity.Property(e => e.BanFileMonitorId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.GameServerServerId).HasColumnName("GameServer_ServerId");

                entity.Property(e => e.LastSync).HasColumnType("datetime");

                entity.HasOne(d => d.GameServerServer)
                    .WithMany(p => p.BanFileMonitors)
                    .HasForeignKey(d => d.GameServerServerId)
                    .HasConstraintName("FK_dbo.BanFileMonitors_dbo.GameServers_GameServer_ServerId");
            });

            modelBuilder.Entity<ChatLogs>(entity =>
            {
                entity.HasKey(e => e.ChatLogId)
                    .HasName("PK_dbo.ChatLogs");

                entity.HasIndex(e => e.ChatLogId)
                    .HasName("IX_ChatLogId")
                    .IsUnique();

                entity.HasIndex(e => e.GameServerServerId)
                    .HasName("IX_GameServer_ServerId");

                entity.HasIndex(e => e.PlayerPlayerId)
                    .HasName("IX_Player_PlayerId");

                entity.HasIndex(e => e.Timestamp)
                    .HasName("IX_Timestamp");

                entity.Property(e => e.ChatLogId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.GameServerServerId).HasColumnName("GameServer_ServerId");

                entity.Property(e => e.PlayerPlayerId).HasColumnName("Player_PlayerId");

                entity.Property(e => e.Timestamp).HasColumnType("datetime");

                entity.HasOne(d => d.GameServerServer)
                    .WithMany(p => p.ChatLogs)
                    .HasForeignKey(d => d.GameServerServerId)
                    .HasConstraintName("FK_dbo.ChatLogs_dbo.GameServers_GameServer_ServerId");

                entity.HasOne(d => d.PlayerPlayer)
                    .WithMany(p => p.ChatLogs)
                    .HasForeignKey(d => d.PlayerPlayerId)
                    .HasConstraintName("FK_dbo.ChatLogs_dbo.Player2_Player_PlayerId");
            });

            modelBuilder.Entity<Demoes>(entity =>
            {
                entity.HasKey(e => e.DemoId)
                    .HasName("PK_dbo.Demoes");

                entity.HasIndex(e => e.DemoId)
                    .HasName("IX_DemoId")
                    .IsUnique();

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_User_Id");

                entity.Property(e => e.DemoId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .HasColumnName("User_Id")
                    .HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Demoes)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.Demoes_dbo.AspNetUsers_User_Id");
            });

            modelBuilder.Entity<FileMonitors>(entity =>
            {
                entity.HasKey(e => e.FileMonitorId)
                    .HasName("PK_dbo.FileMonitors");

                entity.HasIndex(e => e.FileMonitorId)
                    .HasName("IX_FileMonitorId")
                    .IsUnique();

                entity.HasIndex(e => e.GameServerServerId)
                    .HasName("IX_GameServer_ServerId");

                entity.Property(e => e.FileMonitorId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.GameServerServerId).HasColumnName("GameServer_ServerId");

                entity.Property(e => e.LastRead).HasColumnType("datetime");

                entity.HasOne(d => d.GameServerServer)
                    .WithMany(p => p.FileMonitors)
                    .HasForeignKey(d => d.GameServerServerId)
                    .HasConstraintName("FK_dbo.FileMonitors_dbo.GameServers_GameServer_ServerId");
            });

            modelBuilder.Entity<GameServers>(entity =>
            {
                entity.HasKey(e => e.ServerId)
                    .HasName("PK_dbo.GameServers");

                entity.HasIndex(e => e.ServerId)
                    .HasName("IX_ServerId")
                    .IsUnique();

                entity.Property(e => e.ServerId).HasDefaultValueSql("(newsequentialid())");

#pragma warning disable 618
                entity.Property(e => e.LiveLastUpdated).HasColumnType("datetime");
#pragma warning restore 618

                entity.Property(e => e.Title).HasMaxLength(60);
            });

            modelBuilder.Entity<LivePlayerLocations>(entity =>
            {
                entity.HasKey(e => e.LivePlayerLocationId)
                    .HasName("PK_dbo.LivePlayerLocations");

                entity.HasIndex(e => e.LivePlayerLocationId)
                    .HasName("IX_LivePlayerLocationId")
                    .IsUnique();

                entity.Property(e => e.LivePlayerLocationId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.LastSeen).HasColumnType("datetime");
            });

            modelBuilder.Entity<LivePlayers>(entity =>
            {
                entity.HasKey(e => e.LivePlayerId)
                    .HasName("PK_dbo.LivePlayers");

                entity.HasIndex(e => e.GameServerServerId)
                    .HasName("IX_GameServer_ServerId");

                entity.HasIndex(e => e.LivePlayerId)
                    .HasName("IX_LivePlayerId")
                    .IsUnique();

                entity.Property(e => e.LivePlayerId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.GameServerServerId).HasColumnName("GameServer_ServerId");

                entity.HasOne(d => d.GameServerServer)
                    .WithMany(p => p.LivePlayers)
                    .HasForeignKey(d => d.GameServerServerId)
                    .HasConstraintName("FK_dbo.LivePlayers_dbo.GameServers_GameServer_ServerId");
            });

            modelBuilder.Entity<MapFiles>(entity =>
            {
                entity.HasKey(e => e.MapFileId)
                    .HasName("PK_dbo.MapFiles");

                entity.HasIndex(e => e.MapFileId)
                    .HasName("IX_MapFileId")
                    .IsUnique();

                entity.HasIndex(e => e.MapMapId)
                    .HasName("IX_Map_MapId");

                entity.Property(e => e.MapFileId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.MapMapId).HasColumnName("Map_MapId");

                entity.HasOne(d => d.MapMap)
                    .WithMany(p => p.MapFiles)
                    .HasForeignKey(d => d.MapMapId)
                    .HasConstraintName("FK_dbo.MapFiles_dbo.Maps_Map_MapId");
            });

            modelBuilder.Entity<MapRotations>(entity =>
            {
                entity.HasKey(e => e.MapRotationId)
                    .HasName("PK_dbo.MapRotations");

                entity.HasIndex(e => e.GameServerServerId)
                    .HasName("IX_GameServer_ServerId");

                entity.HasIndex(e => e.MapMapId)
                    .HasName("IX_Map_MapId");

                entity.HasIndex(e => e.MapRotationId)
                    .HasName("IX_MapRotationId")
                    .IsUnique();

                entity.Property(e => e.MapRotationId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.GameServerServerId).HasColumnName("GameServer_ServerId");

                entity.Property(e => e.MapMapId).HasColumnName("Map_MapId");

                entity.HasOne(d => d.GameServerServer)
                    .WithMany(p => p.MapRotations)
                    .HasForeignKey(d => d.GameServerServerId)
                    .HasConstraintName("FK_dbo.MapRotations_dbo.GameServers_GameServer_ServerId");

                entity.HasOne(d => d.MapMap)
                    .WithMany(p => p.MapRotations)
                    .HasForeignKey(d => d.MapMapId)
                    .HasConstraintName("FK_dbo.MapRotations_dbo.Maps_Map_MapId");
            });

            modelBuilder.Entity<MapVotes>(entity =>
            {
                entity.HasKey(e => e.MapVoteId)
                    .HasName("PK_dbo.MapVotes");

                entity.HasIndex(e => e.MapMapId)
                    .HasName("IX_Map_MapId");

                entity.HasIndex(e => e.MapVoteId)
                    .HasName("IX_MapVoteId")
                    .IsUnique();

                entity.HasIndex(e => e.PlayerPlayerId)
                    .HasName("IX_Player_PlayerId");

                entity.Property(e => e.MapVoteId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.MapMapId).HasColumnName("Map_MapId");

                entity.Property(e => e.PlayerPlayerId).HasColumnName("Player_PlayerId");

                entity.Property(e => e.Timestamp).HasColumnType("datetime");

                entity.HasOne(d => d.MapMap)
                    .WithMany(p => p.MapVotes)
                    .HasForeignKey(d => d.MapMapId)
                    .HasConstraintName("FK_dbo.MapVotes_dbo.Maps_Map_MapId");

                entity.HasOne(d => d.PlayerPlayer)
                    .WithMany(p => p.MapVotes)
                    .HasForeignKey(d => d.PlayerPlayerId)
                    .HasConstraintName("FK_dbo.MapVotes_dbo.Player2_Player_PlayerId");
            });

            modelBuilder.Entity<Maps>(entity =>
            {
                entity.HasKey(e => e.MapId)
                    .HasName("PK_dbo.Maps");

                entity.HasIndex(e => e.MapId)
                    .HasName("IX_MapId")
                    .IsUnique();

                entity.Property(e => e.MapId).HasDefaultValueSql("(newsequentialid())");
            });

            modelBuilder.Entity<MigrationHistory>(entity =>
            {
                entity.HasKey(e => new {e.MigrationId, e.ContextKey})
                    .HasName("PK_dbo.__MigrationHistory");

                entity.ToTable("__MigrationHistory");

                entity.Property(e => e.MigrationId).HasMaxLength(150);

                entity.Property(e => e.ContextKey).HasMaxLength(300);

                entity.Property(e => e.Model).IsRequired();

                entity.Property(e => e.ProductVersion)
                    .IsRequired()
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<Player2>(entity =>
            {
                entity.HasKey(e => e.PlayerId)
                    .HasName("PK_dbo.Player2");

                entity.HasIndex(e => e.GameType)
                    .HasName("IX_GameType");

                entity.HasIndex(e => e.PlayerId)
                    .HasName("IX_PlayerId")
                    .IsUnique();

                entity.Property(e => e.PlayerId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.FirstSeen).HasColumnType("datetime");

                entity.Property(e => e.LastSeen).HasColumnType("datetime");
            });

            modelBuilder.Entity<PlayerAlias>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasName("IX_Name");

                entity.HasIndex(e => e.PlayerAliasId)
                    .HasName("IX_PlayerAliasId")
                    .IsUnique();

                entity.HasIndex(e => e.PlayerPlayerId)
                    .HasName("IX_Player_PlayerId");

                entity.Property(e => e.PlayerAliasId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.Added).HasColumnType("datetime");

                entity.Property(e => e.LastUsed).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(60);

                entity.Property(e => e.PlayerPlayerId).HasColumnName("Player_PlayerId");

                entity.HasOne(d => d.PlayerPlayer)
                    .WithMany(p => p.PlayerAlias)
                    .HasForeignKey(d => d.PlayerPlayerId)
                    .HasConstraintName("FK_dbo.PlayerAlias_dbo.Player2_Player_PlayerId");
            });

            modelBuilder.Entity<PlayerIpAddresses>(entity =>
            {
                entity.HasKey(e => e.PlayerIpAddressId)
                    .HasName("PK_dbo.PlayerIpAddresses");

                entity.HasIndex(e => e.Address)
                    .HasName("IX_Address");

                entity.HasIndex(e => e.PlayerIpAddressId)
                    .HasName("IX_PlayerIpAddressId")
                    .IsUnique();

                entity.HasIndex(e => e.PlayerPlayerId)
                    .HasName("IX_Player_PlayerId");

                entity.Property(e => e.PlayerIpAddressId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.Added).HasColumnType("datetime");

                entity.Property(e => e.Address).HasMaxLength(60);

                entity.Property(e => e.LastUsed).HasColumnType("datetime");

                entity.Property(e => e.PlayerPlayerId).HasColumnName("Player_PlayerId");

                entity.HasOne(d => d.PlayerPlayer)
                    .WithMany(p => p.PlayerIpAddresses)
                    .HasForeignKey(d => d.PlayerPlayerId)
                    .HasConstraintName("FK_dbo.PlayerIpAddresses_dbo.Player2_Player_PlayerId");
            });

            modelBuilder.Entity<RconMonitors>(entity =>
            {
                entity.HasKey(e => e.RconMonitorId)
                    .HasName("PK_dbo.RconMonitors");

                entity.HasIndex(e => e.GameServerServerId)
                    .HasName("IX_GameServer_ServerId");

                entity.HasIndex(e => e.RconMonitorId)
                    .HasName("IX_RconMonitorId")
                    .IsUnique();

                entity.Property(e => e.RconMonitorId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.GameServerServerId).HasColumnName("GameServer_ServerId");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.MapRotationLastUpdated).HasColumnType("datetime");

                entity.Property(e => e.MonitorPlayerIps).HasColumnName("MonitorPlayerIPs");

                entity.HasOne(d => d.GameServerServer)
                    .WithMany(p => p.RconMonitors)
                    .HasForeignKey(d => d.GameServerServerId)
                    .HasConstraintName("FK_dbo.RconMonitors_dbo.GameServers_GameServer_ServerId");
            });

            modelBuilder.Entity<SystemLogs>(entity =>
            {
                entity.HasKey(e => e.SystemLogId)
                    .HasName("PK_dbo.SystemLogs");

                entity.HasIndex(e => e.SystemLogId)
                    .HasName("IX_SystemLogId")
                    .IsUnique();

                entity.HasIndex(e => e.Timestamp)
                    .HasName("IX_Timestamp");

                entity.Property(e => e.SystemLogId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.Timestamp).HasColumnType("datetime");
            });

            modelBuilder.Entity<UserLogs>(entity =>
            {
                entity.HasKey(e => e.UserLogId)
                    .HasName("PK_dbo.UserLogs");

                entity.HasIndex(e => e.ApplicationUserId)
                    .HasName("IX_ApplicationUser_Id");

                entity.HasIndex(e => e.Timestamp)
                    .HasName("IX_Timestamp");

                entity.HasIndex(e => e.UserLogId)
                    .HasName("IX_UserLogId")
                    .IsUnique();

                entity.Property(e => e.UserLogId).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.ApplicationUserId)
                    .HasColumnName("ApplicationUser_Id")
                    .HasMaxLength(128);

                entity.Property(e => e.Timestamp).HasColumnType("datetime");

                entity.HasOne(d => d.ApplicationUser)
                    .WithMany(p => p.UserLogs)
                    .HasForeignKey(d => d.ApplicationUserId)
                    .HasConstraintName("FK_dbo.UserLogs_dbo.AspNetUsers_ApplicationUser_Id");
            });
        }
    }
}