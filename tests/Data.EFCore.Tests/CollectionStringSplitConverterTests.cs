using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ploch.Data.EFCore.IntegrationTesting;
using Ploch.Data.Model;
using Ploch.TestingSupport.XUnit3.AutoMoq;

namespace Ploch.Data.EFCore.Tests;

public class CollectionStringSplitConverterTests : DataIntegrationTest<ConverterTestDbContext>
{
    [Theory]
    [AutoMockData]
    public void CollectionStringSplitConverter_should_convert_to_and_from_string_list(List<string> firstList, List<string> secondList)
    {
        DbContext.TestEntities.Add(new() { StringCollection = firstList });
        DbContext.TestEntities.Add(new() { StringCollection = secondList });
        DbContext.SaveChanges();

        // Match the complete serialised list exactly, mirroring the converter's write format
        // (Uri.EscapeDataString per element, string.Empty for default values) — searching for a
        // single element could match the wrong entity if the generated lists share a value.
        var serialisedSecondList = string.Join(",", secondList.Select(v => v != null ? Uri.EscapeDataString(v) : string.Empty));

        var entity = DbContext.TestEntities.Skip(1).First();
        var queriedEntity = DbContext.TestEntities.FirstOrDefault(t => (string)(object)t.StringCollection == serialisedSecondList);

        entity.Should().BeEquivalentTo(queriedEntity);
        entity.StringCollection.Should().HaveCount(secondList.Count);
        entity.StringCollection.Should().Contain(secondList);
    }

    [Theory]
    [AutoMockData]
    public void CollectionStringSplitConverter_should_handle_string_list(List<string> firstStringList, List<string> secondStringList)
    {
        // Match the complete serialised list exactly, mirroring the converter's write format
        // (Uri.EscapeDataString per element, string.Empty for default values) — searching for a
        // single element could match the wrong entity if the generated lists share a value.
        var serialisedSecondList = string.Join(",", secondStringList.Select(v => v != null ? Uri.EscapeDataString(v) : string.Empty));

        ValidateConverterEntities(e => e.StringCollection,
                                  (e, v) => e.StringCollection = v,
                                  firstStringList,
                                  secondStringList,
                                  t => (string)(object)t.StringCollection == serialisedSecondList);
    }

    [Theory]
    [AutoMockData]
    public void CollectionStringSplitConverter_should_handle_int_list(List<int> firstIntList, List<int> secondIntList)
    {
        // Match the complete serialised list exactly rather than searching for a single
        // element: a short digit substring such as "4" can also match inside another
        // entity's values (e.g. "147"), which made this test fail intermittently.
        // Mirror the converter's write format exactly (Uri.EscapeDataString per element,
        // string.Empty for default values) so the expected string cannot diverge from the
        // stored value regardless of the generated data.
        var serialisedSecondList = string.Join(",", secondIntList.Select(v => v != 0 ? Uri.EscapeDataString(v.ToString(CultureInfo.InvariantCulture)) : string.Empty));

        ValidateConverterEntities(e => e.IntCollection,
                                  (e, v) => e.IntCollection = v,
                                  firstIntList,
                                  secondIntList,
                                  t => (string)(object)t.IntCollection == serialisedSecondList);
    }

    [Theory]
    [AutoMockData]
    public void CollectionStringSplitConverter_should_handle_datetime_list(List<DateTime> firstDateTimeList, List<DateTime> secondDateTimeList)
    {
        // Match the complete serialised list exactly, mirroring the converter's write format
        // (Uri.EscapeDataString per element, string.Empty for default values) — searching for a
        // single element could match the wrong entity if the generated lists share a value.
        var serialisedSecondList = string.Join(",", secondDateTimeList.Select(v => v != default ? Uri.EscapeDataString(v.ToString(CultureInfo.InvariantCulture)) : string.Empty));

        ValidateConverterEntities(e => e.DatesCollection,
                                  (e, v) => e.DatesCollection = v,
                                  firstDateTimeList,
                                  secondDateTimeList,
                                  t => (string)(object)t.DatesCollection == serialisedSecondList);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_datetime_list_under_non_invariant_culture()
    {
        // de-DE formats dates as "15.03.2024 13:45:30". Before the invariant-culture fix the
        // write path used the current culture while the read path parsed invariantly, so a
        // round-trip under a non-invariant culture corrupted data or threw FormatException.
        RunWithCulture("de-DE",
                       () =>
                       {
                           var dates = new List<DateTime> { new(2024, 3, 15, 13, 45, 30), new(2025, 12, 1, 8, 5, 59) };
                           var entity = CreateFullyPopulatedEntity();
                           entity.DatesCollection = dates;

                           DbContext.TestEntities.Add(entity);
                           DbContext.SaveChanges();
                           DbContext.ChangeTracker.Clear();

                           var reloaded = DbContext.TestEntities.Single(t => t.Id == entity.Id);

                           reloaded.DatesCollection.Should().Equal(dates);
                       });
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_decimal_list_under_non_invariant_culture()
    {
        // de-DE uses a comma as the decimal separator ("1234,56"). Before the invariant-culture
        // fix the write path produced culture-specific strings that the invariant read path could
        // not parse back, so a round-trip under a non-invariant culture threw FormatException.
        RunWithCulture("de-DE",
                       () =>
                       {
                           var decimals = new List<decimal> { 1234.56m, 0.5m, 42m };
                           var entity = CreateFullyPopulatedEntity();
                           entity.DecimalCollection = decimals;

                           DbContext.TestEntities.Add(entity);
                           DbContext.SaveChanges();
                           DbContext.ChangeTracker.Clear();

                           var reloaded = DbContext.TestEntities.Single(t => t.Id == entity.Id);

                           reloaded.DecimalCollection.Should().Equal(decimals);
                       });
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_string_elements_containing_the_separator()
    {
        // The writer escapes each element (a comma becomes %2C); the reader must therefore split
        // the payload BEFORE unescaping the segments, or an element containing the separator is
        // torn apart on read.
        var strings = new List<string> { "alpha,beta", "gamma", "de,l,ta" };
        var entity = CreateFullyPopulatedEntity();
        entity.StringCollection = strings;

        DbContext.TestEntities.Add(entity);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        var reloaded = DbContext.TestEntities.Single(t => t.Id == entity.Id);

        reloaded.StringCollection.Should().Equal(strings);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_an_empty_value_typed_collection()
    {
        // An empty collection is stored as an empty string; the reader must map it back to an
        // empty collection instead of trying to convert a single empty segment to TValue.
        var entity = CreateFullyPopulatedEntity();
        entity.IntCollection = [];

        DbContext.TestEntities.Add(entity);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        var reloaded = DbContext.TestEntities.Single(t => t.Id == entity.Id);

        reloaded.IntCollection.Should().BeEmpty();
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_default_elements_as_defaults()
    {
        // The writer stores default values as empty segments ("1,,2"); the reader maps an empty
        // segment back to default(TValue) instead of throwing FormatException.
        var ints = new List<int> { 1, 0, 2 };
        var entity = CreateFullyPopulatedEntity();
        entity.IntCollection = ints;

        DbContext.TestEntities.Add(entity);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        var reloaded = DbContext.TestEntities.Single(t => t.Id == entity.Id);

        reloaded.IntCollection.Should().Equal(ints);
    }

    private static ConverterTestEntity CreateFullyPopulatedEntity()
    {
        // Every collection is populated with non-default values: an empty collection is stored
        // as an empty string, which the converter's read path would then fail to parse back for
        // the value-typed collections when the whole entity is materialised from the database.
        return new()
        {
            StringCollection = ["alpha", "beta"],
            IntCollection = [1, 2],
            DatesCollection = [new(2024, 1, 2, 3, 4, 5)],
            DecimalCollection = [1.5m],
        };
    }

    private static void RunWithCulture(string cultureName, Action action)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

        try
        {
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private void ValidateConverterEntities<TValue>(Func<ConverterTestEntity, ICollection<TValue>> entityPropertyFunc,
                                                   Action<ConverterTestEntity, List<TValue>> entitySetPropertyFunc,
                                                   List<TValue> firstList,
                                                   List<TValue> secondList,
                                                   Expression<Func<ConverterTestEntity, bool>> findEntityExpression)
    {
        var converterTestEntity1 = new ConverterTestEntity();
        entitySetPropertyFunc(converterTestEntity1, firstList);
        var converterTestEntity2 = new ConverterTestEntity();
        entitySetPropertyFunc(converterTestEntity2, secondList);

        DbContext.TestEntities.Add(converterTestEntity1);
        DbContext.TestEntities.Add(converterTestEntity2);
        DbContext.SaveChanges();

        var queriedEntity = DbContext.TestEntities.FirstOrDefault(findEntityExpression);

        queriedEntity.Should().BeEquivalentTo(converterTestEntity2);
        entityPropertyFunc(queriedEntity).Should().HaveCount(secondList.Count);
        entityPropertyFunc(queriedEntity).Should().Contain(secondList);
    }
}

public class ConverterTestDbContext(DbContextOptions<ConverterTestDbContext> options) : DbContext(options)
{
    public DbSet<ConverterTestEntity> TestEntities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConverterTestEntity>().Property(e => e.StringCollection).HasConversion(new CollectionStringSplitConverter<string>());
        modelBuilder.Entity<ConverterTestEntity>().Property(e => e.IntCollection).HasConversion(new CollectionStringSplitConverter<int>());
        modelBuilder.Entity<ConverterTestEntity>().Property(e => e.DatesCollection).HasConversion(new CollectionStringSplitConverter<DateTime>());
        modelBuilder.Entity<ConverterTestEntity>().Property(e => e.DecimalCollection).HasConversion(new CollectionStringSplitConverter<decimal>());
        base.OnModelCreating(modelBuilder);
    }
}

public class ConverterTestEntity : IHasId<int>
{
    [Key]
    public int Id { get; set; }

    public virtual ICollection<string> StringCollection { get; set; } = new List<string>();

    public virtual ICollection<int> IntCollection { get; set; } = new List<int>();

    public virtual ICollection<DateTime> DatesCollection { get; set; } = new List<DateTime>();

    public virtual ICollection<decimal> DecimalCollection { get; set; } = [];
}
