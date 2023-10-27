﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VideoStore.Movies.Infrastrucutre;

#nullable disable

namespace VideoStore.Ordering.Migrations
{
    [DbContext(typeof(OrderingContext))]
    [Migration("20230424191712_UpdateIdempotentConsumerEntityWithMessageProcessedColumn")]
    partial class UpdateIdempotentConsumerEntityWithMessageProcessedColumn
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("VideoStore.Ordering.Models.IdempotentConsumer", b =>
                {
                    b.Property<Guid>("MessageId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Consumer")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("MessageProcessed")
                        .HasColumnType("datetime2");

                    b.HasKey("MessageId", "Consumer");

                    b.ToTable("IdempotentConsumers");
                });

            modelBuilder.Entity("VideoStore.Ordering.Models.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("UserEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("VideoStore.Ordering.Models.Order", b =>
                {
                    b.OwnsMany("VideoStore.Ordering.Models.Movie", "Movies", b1 =>
                        {
                            b1.Property<int>("OrderId")
                                .HasColumnType("int");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            b1.Property<int>("MovieRefId")
                                .HasColumnType("int");

                            b1.Property<string>("MovieTitle")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("OrderId", "Id");

                            b1.ToTable("Orders");

                            b1.ToJson("Movies");

                            b1.WithOwner()
                                .HasForeignKey("OrderId");
                        });

                    b.Navigation("Movies");
                });
#pragma warning restore 612, 618
        }
    }
}
