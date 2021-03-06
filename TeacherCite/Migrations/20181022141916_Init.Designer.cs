﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
using KovalukApp.Models;

namespace KovalukApp.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20181022141916_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.3-rtm-10026")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("KovalukApp.Models.Answer", b =>
                {
                    b.Property<int>("AnswerID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("AnswerDate");

                    b.Property<int>("StTaskID");

                    b.Property<string>("TextData");

                    b.Property<string>("UserID");

                    b.HasKey("AnswerID");

                    b.ToTable("Answers");
                });

            modelBuilder.Entity("KovalukApp.Models.AppFile", b =>
                {
                    b.Property<int>("AppFileID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<string>("FileExtansion");

                    b.Property<string>("FileName");

                    b.Property<string>("FileSize");

                    b.Property<byte[]>("Value");

                    b.HasKey("AppFileID");

                    b.ToTable("Files");

                    b.HasDiscriminator<string>("Discriminator").HasValue("AppFile");
                });

            modelBuilder.Entity("KovalukApp.Models.DocPage", b =>
                {
                    b.Property<int>("DocPageID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Data");

                    b.Property<DateTime>("Date");

                    b.Property<int>("GroupID");

                    b.Property<string>("Name");

                    b.HasKey("DocPageID");

                    b.ToTable("Documentation");
                });

            modelBuilder.Entity("KovalukApp.Models.Group", b =>
                {
                    b.Property<int>("GroupID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("GroupName");

                    b.Property<int>("YearOfStudy");

                    b.HasKey("GroupID");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("KovalukApp.Models.Lecture", b =>
                {
                    b.Property<int>("LectureID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<string>("Description");

                    b.Property<int>("GroupID");

                    b.Property<string>("Place");

                    b.HasKey("LectureID");

                    b.ToTable("Lectures");
                });

            modelBuilder.Entity("KovalukApp.Models.News", b =>
                {
                    b.Property<int>("NewsID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<DateTime>("PublicationDate");

                    b.Property<string>("TextData");

                    b.HasKey("NewsID");

                    b.ToTable("News");
                });

            modelBuilder.Entity("KovalukApp.Models.StTask", b =>
                {
                    b.Property<int>("StTaskID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CurrentCost");

                    b.Property<DateTime>("DeadLine");

                    b.Property<string>("Description");

                    b.Property<int>("DocPageID");

                    b.Property<bool>("IsChecked");

                    b.Property<int>("MaxCost");

                    b.Property<string>("Name");

                    b.Property<string>("UserID");

                    b.HasKey("StTaskID");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("KovalukApp.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<byte[]>("Avatar");

                    b.Property<bool>("BLocked");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Description");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");

                    b.HasDiscriminator<string>("Discriminator").HasValue("User");
                });

            modelBuilder.Entity("KovalukApp.Models.Visit", b =>
                {
                    b.Property<int>("VisitID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("LectureID");

                    b.Property<string>("VisitorID");

                    b.HasKey("VisitID");

                    b.HasIndex("LectureID");

                    b.ToTable("Visits");
                });

            modelBuilder.Entity("KovalukApp.Models.DocFile", b =>
                {
                    b.HasBaseType("KovalukApp.Models.AppFile");

                    b.Property<string>("Description");

                    b.Property<int>("DocPageId");

                    b.ToTable("DocFile");

                    b.HasDiscriminator().HasValue("DocFile");
                });

            modelBuilder.Entity("KovalukApp.Models.TaskFile", b =>
                {
                    b.HasBaseType("KovalukApp.Models.AppFile");

                    b.Property<string>("Description")
                        .HasColumnName("TaskFile_Description");

                    b.Property<int>("StTaskId");

                    b.ToTable("TaskFile");

                    b.HasDiscriminator().HasValue("TaskFile");
                });

            modelBuilder.Entity("KovalukApp.Models.StudentUser", b =>
                {
                    b.HasBaseType("KovalukApp.Models.User");

                    b.Property<int>("GroupID");

                    b.Property<int>("NumberOfStudentBook");

                    b.ToTable("StudentUser");

                    b.HasDiscriminator().HasValue("StudentUser");
                });

            modelBuilder.Entity("KovalukApp.Models.TeacherUser", b =>
                {
                    b.HasBaseType("KovalukApp.Models.User");


                    b.ToTable("TeacherUser");

                    b.HasDiscriminator().HasValue("TeacherUser");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("KovalukApp.Models.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("KovalukApp.Models.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("KovalukApp.Models.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("KovalukApp.Models.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("KovalukApp.Models.Visit", b =>
                {
                    b.HasOne("KovalukApp.Models.Lecture")
                        .WithMany("Visits")
                        .HasForeignKey("LectureID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
