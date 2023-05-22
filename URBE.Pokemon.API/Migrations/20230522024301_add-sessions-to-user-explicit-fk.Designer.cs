﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using URBE.Pokemon.API.Services;

#nullable disable

namespace URBE.Pokemon.API.Migrations
{
    [DbContext(typeof(UrbeContext))]
    [Migration("20230522024301_add-sessions-to-user-explicit-fk")]
    partial class addsessionstouserexplicitfk
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.ExecutionLogEntry", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Area")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClientName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("ExceptionDumpPath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExceptionMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExceptionType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("JsonProperties")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("LogEventLevel")
                        .HasColumnType("int");

                    b.Property<string>("LoggerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("SessionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("TraceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ExecutionLog", (string)null);
                });

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.MailConfirmationRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("ClaimedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("ClaimedById")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreationDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("DispatchedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ClaimedById");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("MailConfirmationRequests");
                });

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.PokemonList", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("PokemonList");
                });

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.PokemonReference", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset>("CreationDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("ListId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("PokemonId")
                        .HasColumnType("int");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ListId");

                    b.HasIndex("UserId");

                    b.ToTable("PokemonReference");
                });

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.Server", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("HeartbeatInterval")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("LastHeartbeat")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("Registered")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.Session", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<long>("Expiration")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("LastUsed")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.User", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreationDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsMailConfirmed")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset>("LastModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("UserPermissions")
                        .HasColumnType("bigint");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.MailConfirmationRequest", b =>
                {
                    b.HasOne("URBE.Pokemon.API.Models.Database.Server", "ClaimedBy")
                        .WithMany()
                        .HasForeignKey("ClaimedById");

                    b.HasOne("URBE.Pokemon.API.Models.Database.User", "User")
                        .WithOne("MailConfirmationRequest")
                        .HasForeignKey("URBE.Pokemon.API.Models.Database.MailConfirmationRequest", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ClaimedBy");

                    b.Navigation("User");
                });

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.PokemonList", b =>
                {
                    b.HasOne("URBE.Pokemon.API.Models.Database.User", "User")
                        .WithMany("PokemonLists")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.PokemonReference", b =>
                {
                    b.HasOne("URBE.Pokemon.API.Models.Database.PokemonList", "List")
                        .WithMany("Pokemon")
                        .HasForeignKey("ListId");

                    b.HasOne("URBE.Pokemon.API.Models.Database.User", "User")
                        .WithMany("VisitHistory")
                        .HasForeignKey("UserId");

                    b.Navigation("List");

                    b.Navigation("User");
                });

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.Session", b =>
                {
                    b.HasOne("URBE.Pokemon.API.Models.Database.User", "User")
                        .WithMany("Sessions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.PokemonList", b =>
                {
                    b.Navigation("Pokemon");
                });

            modelBuilder.Entity("URBE.Pokemon.API.Models.Database.User", b =>
                {
                    b.Navigation("MailConfirmationRequest");

                    b.Navigation("PokemonLists");

                    b.Navigation("Sessions");

                    b.Navigation("VisitHistory");
                });
#pragma warning restore 612, 618
        }
    }
}
