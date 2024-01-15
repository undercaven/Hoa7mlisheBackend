using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using hoa7mlishe.API.Database.Models;

namespace hoa7mlishe.API.Database.Context;

public partial class Hoa7mlisheContext : DbContext
{
    public Hoa7mlisheContext()
    {
    }

    public Hoa7mlisheContext(DbContextOptions<Hoa7mlisheContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CardPack> CardPacks { get; set; }

    public virtual DbSet<CardSeason> CardSeasons { get; set; }

    public virtual DbSet<CollectedCard> CollectedCards { get; set; }

    public virtual DbSet<DeathClock> DeathClocks { get; set; }

    public virtual DbSet<CardInfo> FileInfos { get; set; }

    public virtual DbSet<FileInterface> FileInterfaces { get; set; }

    public virtual DbSet<Hoa7mlisheFile> Hoa7mlisheFiles { get; set; }

    public virtual DbSet<IvonovQuote> IvonovQuotes { get; set; }

    public virtual DbSet<TradeContent> TradeContents { get; set; }

    public virtual DbSet<TradeOffer> TradeOffers { get; set; }

    public virtual DbSet<User> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#if DEBUG
                => optionsBuilder.UseSqlServer("Server=.\\HOASERVER_DEV;Database=Hoa7mlishe;User Id=sa;Password=chuchikmuchik;TrustServerCertificate=true", x => x.UseHierarchyId());
#else
        => optionsBuilder.UseSqlServer("Server=localhost\\HOASERVER_PROD; Database = Hoa7mlishe; TrustServerCertificate=True; Trusted_Connection = True; User Id=sa; Password=123asd123;", x => x.UseHierarchyId());
#endif
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<CardPack>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CollectedCard>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Card).WithMany(p => p.CollectedCards)
                .HasForeignKey(d => d.CardId)
                .HasConstraintName("FK_CollectedCards_FileInfos");

            entity.HasOne(d => d.User).WithMany(p => p.CollectedCards)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_CollectedCards_Users");
        });

        modelBuilder.Entity<DeathClock>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("PK_Deathclocks");

            entity.Property(e => e.Name).HasMaxLength(30);
            entity.Property(e => e.DeathTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<CardInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_FileInfo");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Tag).HasMaxLength(20);

            entity.HasOne(d => d.File).WithMany(p => p.FileInfos)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_FileInfos_FileInterfaces");

            entity.HasOne(d => d.Season).WithMany(p => p.FileInfos)
                .HasForeignKey(d => d.SeasonId)
                .HasConstraintName("FK_FileInfos_CardSeasons");
        });

        modelBuilder.Entity<FileInterface>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK_FileInterface");

            entity.Property(e => e.RecordId).ValueGeneratedNever();

            entity.HasOne(d => d.PathLocatorNavigation).WithMany(p => p.FileInterfaces)
                .HasForeignKey(d => d.PathLocator)
                .HasConstraintName("FK_FileInterface_hoa7mlishe_Files");
        });

        modelBuilder.Entity<Hoa7mlisheFile>(entity =>
        {
            entity.HasKey(e => e.PathLocator)
                .HasName("PK__Hoa7mlis__5A5B77D5DFD949F1")
                .IsClustered(false);

            entity.ToTable("hoa7mlishe_Files");

            entity.HasIndex(e => e.StreamId, "UQ__Hoa7mlis__9DD95BAF63BDB814").IsUnique();

            entity.HasIndex(e => new { e.ParentPathLocator, e.Name }, "UQ__Hoa7mlis__A236CBB3265C11CB").IsUnique();

            entity.Property(e => e.PathLocator)
                .HasDefaultValueSql("(convert(hierarchyid, '/' +     convert(varchar(20), convert(bigint, substring(convert(binary(16), newid()), 1, 6))) + '.' +     convert(varchar(20), convert(bigint, substring(convert(binary(16), newid()), 7, 6))) + '.' +     convert(varchar(20), convert(bigint, substring(convert(binary(16), newid()), 13, 4))) + '/'))")
                .HasColumnName("path_locator");
            entity.Property(e => e.CachedFileSize)
                .HasComputedColumnSql("(datalength(file_stream))", true)
                .HasColumnName("cached_file_size");
            entity.Property(e => e.CreationTime)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("creation_time");
            entity.Property(e => e.FileStream).HasColumnName("file_stream");
            entity.Property(e => e.FileType)
                .HasMaxLength(255)
                .HasComputedColumnSql("(getfileextension([name]))", true)
                .HasColumnName("file_type");
            entity.Property(e => e.IsArchive)
                .IsRequired()
                .HasDefaultValueSql("((1))")
                .HasColumnName("is_archive");
            entity.Property(e => e.IsDirectory).HasColumnName("is_directory");
            entity.Property(e => e.IsHidden).HasColumnName("is_hidden");
            entity.Property(e => e.IsOffline).HasColumnName("is_offline");
            entity.Property(e => e.IsReadonly).HasColumnName("is_readonly");
            entity.Property(e => e.IsSystem).HasColumnName("is_system");
            entity.Property(e => e.IsTemporary).HasColumnName("is_temporary");
            entity.Property(e => e.LastAccessTime)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("last_access_time");
            entity.Property(e => e.LastWriteTime)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("last_write_time");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ParentPathLocator)
                .HasComputedColumnSql("(case when [path_locator].[GetLevel]()=(1) then NULL else [path_locator].[GetAncestor]((1)) end)", true)
                .HasColumnName("parent_path_locator");
            entity.Property(e => e.StreamId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("stream_id");

            entity.HasOne(d => d.ParentPathLocatorNavigation).WithMany(p => p.InverseParentPathLocatorNavigation)
                .HasForeignKey(d => d.ParentPathLocator)
                .HasConstraintName("FK__Hoa7mlish__paren__4E88ABD4");
        });

        modelBuilder.Entity<IvonovQuote>(entity =>
        {
            entity.ToTable("Ivonov_Quotes");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<TradeContent>(entity =>
        {
            entity.HasKey(e => new { e.TradeId, e.CardId });

            entity.HasOne(d => d.Card).WithMany(p => p.TradeContents)
                .HasForeignKey(d => d.CardId)
                .HasConstraintName("FK_TradeContents_FileInfos");

            entity.HasOne(d => d.Trade).WithMany(p => p.TradeContents)
                .HasForeignKey(d => d.TradeId)
                .HasConstraintName("FK_TradeContents_TradeOffers");
        });

        modelBuilder.Entity<TradeOffer>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Receiver).WithMany(p => p.TradeOfferReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TradeOffers_Users1");

            entity.HasOne(d => d.Sender).WithMany(p => p.TradeOfferSenders)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK_TradeOffers_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Login).HasMaxLength(30);
            entity.Property(e => e.Password)
                .HasMaxLength(16)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
