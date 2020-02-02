﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WaitTime.Entities
{
    public partial class WaitTimeContext : DbContext
    {
        public virtual DbSet<Lift> Lifts { get; set; }
        public virtual DbSet<Resort> Resorts { get; set; }
        public virtual DbSet<Uplift> Uplifts { get; set; }
        public virtual DbSet<Activity> Activities { get; set; }
        public virtual DbSet<ActivitySyncBatch> ActivitySyncBatches { get; set; }
        public virtual DbSet<ActivitySyncBatchLocation> ActivitySyncBatchLocations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(@"Server=(local)\SQL2012;Database=LiftLines;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Lift>(entity =>
            {
                entity.ToTable("Lift");
                entity.Property(e => e.LiftID).HasColumnName("LiftID");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.OsmId).HasColumnName("OsmID");

                entity.Property(e => e.OsmLink)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ResortID).HasColumnName("ResortID");

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.HasOne(d => d.Resort)
                    .WithMany(p => p.Lifts)
                    .HasForeignKey(d => d.ResortID)
                    .HasConstraintName("FK_Lift_Resort");
            });

            modelBuilder.Entity<Resort>(entity =>
            {
                entity.ToTable("Resort");
                entity.Property(e => e.ResortID).HasColumnName("ResortID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Uplift>(entity =>
            {
                entity.ToTable("Uplift");
                entity.Property(e => e.UpliftID).HasColumnName("UpliftID");

                entity.Property(e => e.LiftID).HasColumnName("LiftID");

                entity.Property(e => e.TrackID).HasColumnName("TrackID");

                entity.HasOne(d => d.Lift)
                    .WithMany(p => p.Uplift)
                    .HasForeignKey(d => d.LiftID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Uplift_Lift");
            });

            modelBuilder.Entity<Activity>(entity =>
            {
                entity.ToTable("Activity");
            });

            modelBuilder.Entity<ActivitySyncBatch>(entity =>
            {
                entity.ToTable("ActivitySyncBatch");

                entity.HasOne(e => e.Activity)
                    .WithMany(e => e.Batches)
                    .HasForeignKey(d => d.ActivityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ActivitySyncBatch_Activity");
            });

            modelBuilder.Entity<ActivitySyncBatchLocation>(entity =>
            {
                entity.ToTable("ActivitySyncBatchLocation");

                entity.HasOne(e => e.Batch)
                    .WithMany(e => e.Locations)
                    .HasForeignKey(d => d.ActivitySyncBatchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ActivitySyncBatchLocation_ActivitySyncBatch");
            });
        }
    }
}
