﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TemperatureDemoPlugin.Data;

#nullable disable

namespace TemperatureDemoPlugin.TemperatureMigrations
{
    [DbContext(typeof(TemperatureDemoContext))]
    partial class TemperatureDemoContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("TemperatureDemoPlugin.Data.DataEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CaptureTime")
                        .HasColumnType("TEXT");

                    b.Property<double?>("HumidityPercent")
                        .HasColumnType("REAL");

                    b.Property<int>("SensorId")
                        .HasColumnType("INTEGER");

                    b.Property<double?>("TemperatureC")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("SensorId");

                    b.ToTable("DataEntries");
                });

            modelBuilder.Entity("TemperatureDemoPlugin.Data.Sensor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("IpAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastUpdateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Sensors");
                });

            modelBuilder.Entity("TemperatureDemoPlugin.Data.DataEntry", b =>
                {
                    b.HasOne("TemperatureDemoPlugin.Data.Sensor", "Sensor")
                        .WithMany("DataEntries")
                        .HasForeignKey("SensorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sensor");
                });

            modelBuilder.Entity("TemperatureDemoPlugin.Data.Sensor", b =>
                {
                    b.Navigation("DataEntries");
                });
#pragma warning restore 612, 618
        }
    }
}