﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserPlatform.Shared.IPL.Context;

#nullable disable

namespace UserPlatform.Shared.Migrations
{
    [DbContext(typeof(UserContext))]
    [Migration("20240419094459_user_added_last_login")]
    partial class user_added_last_login
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("UserPlatform.Shared.DL.Models.RefreshToken", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Revoked")
                        .HasColumnType("bit");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.ComplexProperty<Dictionary<string, object>>("User", "UserPlatform.Shared.DL.Models.RefreshToken.User#ReferenceId", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<Guid>("Id")
                                .HasColumnType("uniqueidentifier");
                        });

                    b.HasKey("Id");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("UserPlatform.Shared.DL.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("datetime2");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.ComplexProperty<Dictionary<string, object>>("Contact", "UserPlatform.Shared.DL.Models.User.Contact#UserContact", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<string>("Email")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Phone")
                                .HasColumnType("nvarchar(max)");
                        });

                    b.ComplexProperty<Dictionary<string, object>>("Location", "UserPlatform.Shared.DL.Models.User.Location#UserLocation", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");
                        });

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("UserPlatform.Shared.DL.Models.User", b =>
                {
                    b.OwnsMany("Shared.DDD.ReferenceId", "Orders", b1 =>
                        {
                            b1.Property<Guid>("UserId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<Guid>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("uniqueidentifier");

                            b1.HasKey("UserId", "Id");

                            b1.ToTable("Users_Orders");

                            b1.WithOwner()
                                .HasForeignKey("UserId");
                        });

                    b.OwnsMany("Shared.DDD.ReferenceId", "RefreshTokens", b1 =>
                        {
                            b1.Property<Guid>("UserId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<Guid>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("uniqueidentifier");

                            b1.HasKey("UserId", "Id");

                            b1.ToTable("Users_RefreshTokens");

                            b1.WithOwner()
                                .HasForeignKey("UserId");
                        });

                    b.Navigation("Orders");

                    b.Navigation("RefreshTokens");
                });
#pragma warning restore 612, 618
        }
    }
}