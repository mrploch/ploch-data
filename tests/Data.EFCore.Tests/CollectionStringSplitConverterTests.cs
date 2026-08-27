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

        // Without this the queries below are satisfied from the change tracker's identity map and
        // the converter's read path is never invoked.
        DbContext.ChangeTracker.Clear();

        // Match the complete serialised list exactly, mirroring the converter's write format —
        // every element is escaped, and only a null element becomes an empty segment. Searching
        // for a single element could match the wrong entity if the generated lists share a value.
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
        // Match the complete serialised list exactly, mirroring the converter's write format —
        // every element is escaped, and only a null element becomes an empty segment. Searching
        // for a single element could match the wrong entity if the generated lists share a value.
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
        // Mirror the converter's write format exactly — every element is escaped, and only a
        // null element becomes an empty segment, which an int can never be — so the expected
        // string cannot diverge from the stored value regardless of the generated data.
        var serialisedSecondList = string.Join(",", secondIntList.Select(v => Uri.EscapeDataString(v.ToString(CultureInfo.InvariantCulture))));

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
        // Truncate to whole seconds. The converter serialises via Convert.ToString, whose invariant
        // general format has no fractional-seconds component, so sub-second precision cannot survive
        // a round-trip — a known limitation tracked in #121. AutoFixture generates sub-second
        // precision, so without this the fixture would assert a guarantee the format does not make.
        firstDateTimeList = [.. firstDateTimeList.Select(TruncateToSeconds)];
        secondDateTimeList = [.. secondDateTimeList.Select(TruncateToSeconds)];

        // Match the complete serialised list exactly, mirroring the converter's write format —
        // every element is escaped, and only a null element becomes an empty segment, which a
        // DateTime can never be. Searching for a single element could match the wrong entity if
        // the generated lists share a value.
        var serialisedSecondList = string.Join(",", secondDateTimeList.Select(v => Uri.EscapeDataString(v.ToString(CultureInfo.InvariantCulture))));

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
                           // Kind is Unspecified deliberately: the converter serialises via
                           // Convert.ToString, which carries no offset, so a round-trip cannot
                           // preserve DateTimeKind. Asserting against Unspecified keeps the
                           // expectation honest rather than relying on DateTime equality
                           // ignoring Kind.
                           var dates = new List<DateTime>
                           {
                               new(2024, 3, 15, 13, 45, 30, DateTimeKind.Unspecified),
                               new(2025, 12, 1, 8, 5, 59, DateTimeKind.Unspecified),
                           };
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
        // Only a null element is written as an empty segment, so a zero is written verbatim
        // ("1,0,2") and reads straight back. The reader's empty-segment branch still maps to
        // default(TValue), which keeps payloads written by earlier versions ("1,,2") readable.
        var ints = new List<int> { 1, 0, 2 };
        var entity = CreateFullyPopulatedEntity();
        entity.IntCollection = ints;

        DbContext.TestEntities.Add(entity);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        var reloaded = DbContext.TestEntities.Single(t => t.Id == entity.Id);

        reloaded.IntCollection.Should().Equal(ints);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_a_single_default_valued_element()
    {
        // Regression guard for the cardinality defect: while the writer stored any element equal
        // to default(TValue) as an empty segment, a one-element collection holding that default
        // serialised to the empty payload — indistinguishable from an empty collection — and
        // silently reloaded as empty. Writing every non-null element verbatim gives the empty
        // payload exactly one meaning.
        var entity = CreateFullyPopulatedEntity();
        entity.IntCollection = [0];

        DbContext.TestEntities.Add(entity);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        var reloaded = DbContext.TestEntities.Single(t => t.Id == entity.Id);

        reloaded.IntCollection.Should().Equal(0);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_a_single_default_valued_decimal_element()
    {
        // The same cardinality guard for a second value type, so the fix cannot regress for one
        // TValue while passing for another.
        var entity = CreateFullyPopulatedEntity();
        entity.DecimalCollection = [0m];

        DbContext.TestEntities.Add(entity);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        var reloaded = DbContext.TestEntities.Single(t => t.Id == entity.Id);

        reloaded.DecimalCollection.Should().Equal(0m);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_write_null_string_elements_as_empty_segments()
    {
        // Pins the one format invariant the converter guarantees: an empty segment means the
        // element was null. A null element survives a round-trip exactly.
        var strings = new List<string> { "alpha", null!, "beta" };
        var entity = CreateFullyPopulatedEntity();
        entity.StringCollection = strings;

        DbContext.TestEntities.Add(entity);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        var reloaded = DbContext.TestEntities.Single(t => t.Id == entity.Id);

        reloaded.StringCollection.Should().Equal(strings);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_read_an_empty_string_element_back_as_null_until_the_format_is_revised()
    {
        // Pins a KNOWN LIMITATION rather than desired behaviour: an empty string element is written
        // as an empty segment, which is also the encoding for null, so it reads back as null.
        // Tracked in #121.
        //
        // Deliberately asserted rather than skipped. A skipped test protects nothing today, whereas
        // this one fails the moment the behaviour drifts. The name says "until the format is
        // revised" so that whoever fixes #121 knows this test is expected to change with it and
        // does not mistake the failure for a regression.
        var entity = CreateFullyPopulatedEntity();
        entity.StringCollection = ["alpha", string.Empty];

        DbContext.TestEntities.Add(entity);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        var reloaded = DbContext.TestEntities.Single(t => t.Id == entity.Id);

        reloaded.StringCollection.Should().Equal("alpha", null);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_still_read_payloads_written_in_the_legacy_empty_segment_encoding()
    {
        // Pins the backward-compatibility claim made in RELEASE_NOTES. Earlier versions wrote any
        // element equal to default(TValue) as an empty segment, so [1, 0, 2] was stored as "1,,2".
        // The read path is unchanged and must keep decoding that, even though the writer now
        // produces "1,0,2". Exercised through the converter directly rather than through EF,
        // because the payload cannot be produced by the current writer.
        var converter = new CollectionStringSplitConverter<int>();

        var decoded = (ICollection<int>)converter.ConvertFromProvider("1,,2")!;

        decoded.Should().Equal(1, 0, 2);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_write_a_default_element_verbatim_rather_than_as_an_empty_segment()
    {
        // The write half of the same compatibility story: the new encoding is "1,0,2", not "1,,2".
        var converter = new CollectionStringSplitConverter<int>();

        var encoded = (string)converter.ConvertToProvider(new List<int> { 1, 0, 2 })!;

        encoded.Should().Be("1,0,2");
    }

    [Fact]
    public void CollectionStringSplitConverter_should_read_a_single_empty_or_null_string_collection_back_as_empty_until_the_format_is_revised()
    {
        // Pins the residual half of the cardinality hole that this change did NOT close, so it is
        // covered rather than merely described in prose. For a reference type an empty segment is
        // produced by both null and "", so a one-element collection holding either is
        // indistinguishable from an empty collection. Tracked in #121, and expected to change when
        // that is fixed.
        var converter = new CollectionStringSplitConverter<string>();

        ((string)converter.ConvertToProvider(new List<string> { string.Empty })!).Should().BeEmpty();
        ((string)converter.ConvertToProvider(new List<string> { null! })!).Should().BeEmpty();
        ((ICollection<string>)converter.ConvertFromProvider(string.Empty)!).Should().BeEmpty();
    }

    [Fact]
    public void CollectionStringSplitConverter_should_keep_elements_intact_when_the_separator_is_escapable()
    {
        // Escaping is what protects a separator occurring inside an element. The default "," is
        // escaped to %2C, so an element containing a comma survives.
        var converter = new CollectionStringSplitConverter<string>();

        var encoded = (string)converter.ConvertToProvider(new List<string> { "a,b", "c" })!;
        var decoded = (ICollection<string>)converter.ConvertFromProvider(encoded)!;

        encoded.Should().Be("a%2Cb,c");
        decoded.Should().Equal("a,b", "c");
    }

    [Theory]
    [InlineData("-")]
    [InlineData(".")]
    [InlineData("_")]
    [InlineData("~")]
    [InlineData("x")]
    [InlineData("7")]
    [InlineData("a-b")]
    public void CollectionStringSplitConverter_constructor_should_reject_a_separator_that_escaping_leaves_unchanged(string separator)
    {
        // Uri.EscapeDataString passes the RFC 3986 unreserved characters (A-Z a-z 0-9 - . _ ~)
        // through unescaped, so a separator drawn only from that set cannot be distinguished from
        // the same character inside an element: "a-b" would be written as "a-b" and read back as
        // two elements. Previously accepted and silently corrupting; now rejected at construction.
        var act = () => new CollectionStringSplitConverter<string>(separator);

        act.Should().Throw<ArgumentException>().WithParameterName(nameof(separator));
    }

    [Theory]
    [InlineData(",")]
    [InlineData(";")]
    [InlineData("|")]
    [InlineData("::")]
    public void CollectionStringSplitConverter_constructor_should_accept_a_separator_containing_an_escapable_character(string separator)
    {
        // The mirror of the guard: any separator escaping changes is safe, because an occurrence
        // inside an element is escaped and therefore cannot be mistaken for a delimiter.
        var converter = new CollectionStringSplitConverter<string>(separator);

        var encoded = (string)converter.ConvertToProvider(new List<string> { $"a{separator}b", "c" })!;
        var decoded = (ICollection<string>)converter.ConvertFromProvider(encoded)!;

        decoded.Should().Equal($"a{separator}b", "c");
    }

    [Fact]
    public void CollectionStringSplitConverter_constructor_should_reject_an_empty_separator()
    {
        var act = () => new CollectionStringSplitConverter<string>(string.Empty);

        act.Should().Throw<ArgumentException>().WithParameterName("separator");
    }

    [Fact]
    public void CollectionStringSplitConverter_constructor_should_reject_a_null_separator()
    {
        var act = () => new CollectionStringSplitConverter<string>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("separator");
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_a_null_collection_as_null()
    {
        // convertNulls defaults to true, so EF invokes the converter for nulls rather than
        // short-circuiting them. Both lambdas previously assumed a non-null argument, so a null
        // collection threw ArgumentNullException out of Enumerable.Select during SaveChanges and a
        // NULL column would have thrown NullReferenceException on read. Null now maps to null in
        // both directions, which also makes a null collection distinguishable from an empty one.
        var converter = new CollectionStringSplitConverter<string>();

        converter.ConvertToProvider(null).Should().BeNull();
        converter.ConvertFromProvider(null).Should().BeNull();
    }

    [Fact]
    public void CollectionStringSplitConverter_should_distinguish_a_null_collection_from_an_empty_one()
    {
        var converter = new CollectionStringSplitConverter<int>();

        converter.ConvertToProvider(null).Should().BeNull();
        converter.ConvertToProvider(new List<int>()).Should().Be(string.Empty);

        converter.ConvertFromProvider(null).Should().BeNull();
        ((ICollection<int>)converter.ConvertFromProvider(string.Empty)!).Should().BeEmpty();
    }

    [Fact]
    public void CollectionStringSplitConverter_should_save_and_reload_a_null_collection_on_an_optional_property()
    {
        // The end-to-end form of the null guard. This is the path that threw, as a
        // DbUpdateException wrapping "ArgumentNullException: Value cannot be null. (Parameter
        // 'source')" from Enumerable.Select inside SaveChanges. It uses the nullable property
        // because the converter now writes SQL NULL for a null collection, which a NOT NULL column
        // correctly rejects.
        var entity = CreateFullyPopulatedEntity();
        entity.OptionalStringCollection = null;

        DbContext.TestEntities.Add(entity);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        var reloaded = DbContext.TestEntities.Single(t => t.Id == entity.Id);

        reloaded.OptionalStringCollection.Should().BeNull();
        reloaded.IntCollection.Should().Equal(1, 2);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_distinguish_null_from_empty_on_an_optional_property_end_to_end()
    {
        // A null collection and an empty one must remain distinguishable after a database
        // round-trip, not merely at the converter level.
        var withNull = CreateFullyPopulatedEntity();
        withNull.OptionalStringCollection = null;
        var withEmpty = CreateFullyPopulatedEntity();
        withEmpty.OptionalStringCollection = [];

        DbContext.TestEntities.AddRange(withNull, withEmpty);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        DbContext.TestEntities.Single(t => t.Id == withNull.Id).OptionalStringCollection.Should().BeNull();
        DbContext.TestEntities.Single(t => t.Id == withEmpty.Id).OptionalStringCollection.Should().NotBeNull().And.BeEmpty();
    }

    private static ConverterTestEntity CreateFullyPopulatedEntity()
    {
        // Every collection is populated so that a test which exercises one property is not also
        // silently exercising the empty-collection path on the others. Each test overwrites the
        // single collection it cares about.
        return new()
        {
            StringCollection = ["alpha", "beta"],
            IntCollection = [1, 2],
            DatesCollection = [new(2024, 1, 2, 3, 4, 5, DateTimeKind.Unspecified)],
            DecimalCollection = [1.5m],
        };
    }

    private static DateTime TruncateToSeconds(DateTime value) =>
        new(value.Ticks - (value.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Unspecified);

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

        // Without this the query is satisfied from the change tracker's identity map, so the
        // converter's read path is never invoked and the assertions below only compare an entity
        // with itself. Clearing forces a real materialisation from the database.
        DbContext.ChangeTracker.Clear();

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

        // CS8620: the property is ICollection<string>? while the converter is declared over the
        // non-nullable ICollection<string>. The variance is safe here precisely because of the
        // change under test — the converter now maps null to null in both directions rather than
        // throwing, which is what makes it usable for an optional property at all.
#pragma warning disable CS8620
        modelBuilder.Entity<ConverterTestEntity>().Property(e => e.OptionalStringCollection).HasConversion(new CollectionStringSplitConverter<string>());
#pragma warning restore CS8620
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

    /// <summary>
    ///     Declared nullable on purpose, so EF Core maps it to a nullable column and the converter's
    ///     null handling can be exercised end to end. The non-nullable properties above map to
    ///     NOT NULL columns, where a null collection is correctly rejected by the database.
    /// </summary>
    public virtual ICollection<string>? OptionalStringCollection { get; set; }
}
