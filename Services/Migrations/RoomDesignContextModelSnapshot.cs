﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Services;

namespace Services.Migrations
{
    [DbContext(typeof(RoomDesignContext))]
    partial class RoomDesignContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Models.Design", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("DesignId")
                        .UseIdentityColumn();

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RoomId")
                        .HasColumnType("int");

                    b.Property<string>("Route")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("RoomId");

                    b.ToTable("Designs");
                });

            modelBuilder.Entity("Models.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ProductId")
                        .UseIdentityColumn();

                    b.Property<bool>("HasPerson")
                        .HasColumnType("bit");

                    b.Property<int>("Length")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Photo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("Price")
                        .HasColumnType("float");

                    b.Property<int>("Width")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("Models.ProductPlacement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ProductPlacementId")
                        .UseIdentityColumn();

                    b.Property<int>("DesignId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Rotation")
                        .HasColumnType("int");

                    b.Property<int>("X")
                        .HasColumnType("int");

                    b.Property<int>("Y")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DesignId");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductPlacements");
                });

            modelBuilder.Entity("Models.Room", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("RoomId")
                        .UseIdentityColumn();

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Positions")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("Models.Design", b =>
                {
                    b.HasOne("Models.Room", "Room")
                        .WithMany()
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");
                });

            modelBuilder.Entity("Models.ProductPlacement", b =>
                {
                    b.HasOne("Models.Design", "Design")
                        .WithMany("ProductPlacements")
                        .HasForeignKey("DesignId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Design");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("Models.Design", b =>
                {
                    b.Navigation("ProductPlacements");
                });
#pragma warning restore 612, 618
        }
    }
}
