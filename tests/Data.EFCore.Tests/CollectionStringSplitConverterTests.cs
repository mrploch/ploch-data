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
    /// <summary>The format header every payload written by the current format starts with.</summary>
    private const string Header = "!1";

    public static TheoryData<string?[], string> EmptyAndNullStringMatrix() =>
        new()
        {
            { ["a", string.Empty], "!1,va,v" },
            { [string.Empty, "a"], "!1,v,va" },
            { [string.Empty, string.Empty], "!1,v,v" },
            { [string.Empty], "!1,v" },
            { [null], "!1,n" },
            { ["a", null], "!1,va,n" },
            { [null, "a"], "!1,n,va" },
            { [], "!1" },
        };

    [Theory]
    [AutoMockData]
    public void CollectionStringSplitConverter_should_convert_to_and_from_string_list(List<string> firstList, List<string> secondList)
    {
        DbContext.TestEntities.Add(CreateFullyPopulatedEntity(e => e.StringCollection = firstList));
        DbContext.TestEntities.Add(CreateFullyPopulatedEntity(e => e.StringCollection = secondList));
        DbContext.SaveChanges();

        // Without this the queries below are satisfied from the change tracker's identity map and
        // the converter's read path is never invoked.
        DbContext.ChangeTracker.Clear();

        // Match the complete serialised list exactly, mirroring the converter's write format.
        // Searching for a single element could match the wrong entity if the generated lists share
        // a value.
        var serialisedSecondList = Serialise(secondList, value => value);

        var entity = DbContext.TestEntities.Skip(1).First();
        var queriedEntity = DbContext.TestEntities.FirstOrDefault(t => (string)(object)t.StringCollection == serialisedSecondList);

        entity.Should().BeEquivalentTo(queriedEntity);
        entity.StringCollection.Should().Equal(secondList);
    }

    [Theory]
    [AutoMockData]
    public void CollectionStringSplitConverter_should_handle_string_list(List<string> firstStringList, List<string> secondStringList)
    {
        var serialisedSecondList = Serialise(secondStringList, value => value);

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
        // Match the complete serialised list exactly rather than searching for a single element: a
        // short digit substring such as "4" can also match inside another entity's values (e.g.
        // "147"), which made this test fail intermittently.
        var serialisedSecondList = Serialise(secondIntList, value => value.ToString(CultureInfo.InvariantCulture));

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
        // No truncation: the format now writes DateTime with the "O" round-trip specifier, so the
        // generated sub-second precision must survive intact. Truncating here would hide exactly
        // the defect this change fixes.
        var serialisedSecondList = Serialise(secondDateTimeList, value => value.ToString("O", CultureInfo.InvariantCulture));

        ValidateConverterEntities(e => e.DatesCollection,
                                  (e, v) => e.DatesCollection = v,
                                  firstDateTimeList,
                                  secondDateTimeList,
                                  t => (string)(object)t.DatesCollection == serialisedSecondList);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_datetime_list_under_non_invariant_culture()
    {
        // de-DE formats dates as "15.03.2024 13:45:30". Before the invariant-culture fix the write
        // path used the current culture while the read path parsed invariantly, so a round-trip
        // under a non-invariant culture corrupted data or threw FormatException.
        RunWithCulture("de-DE",
                       () =>
                       {
                           var dates = new List<DateTime>
                                       {
                                           new(2024, 3, 15, 13, 45, 30, DateTimeKind.Utc),
                                           new(2025, 12, 1, 8, 5, 59, DateTimeKind.Unspecified),
                                       };
                           var entity = CreateFullyPopulatedEntity(e => e.DatesCollection = dates);

                           DbContext.TestEntities.Add(entity);
                           DbContext.SaveChanges();
                           DbContext.ChangeTracker.Clear();

                           var reloaded = DbContext.TestEntities.Single(t => t.Id == entity.Id);

                           reloaded.DatesCollection.Should().Equal(dates);

                           // DateTime equality compares ticks only, so Kind has to be asserted
                           // separately or its loss passes unnoticed.
                           reloaded.DatesCollection.Select(d => d.Kind).Should().Equal(DateTimeKind.Utc, DateTimeKind.Unspecified);
                       });
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_decimal_list_under_non_invariant_culture()
    {
        // de-DE uses a comma as the decimal separator ("1234,56"). Before the invariant-culture fix
        // the write path produced culture-specific strings that the invariant read path could not
        // parse back, so a round-trip under a non-invariant culture threw FormatException.
        RunWithCulture("de-DE",
                       () =>
                       {
                           var decimals = new List<decimal> { 1234.56m, 0.5m, 42m };
                           var entity = CreateFullyPopulatedEntity(e => e.DecimalCollection = decimals);

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
        // The writer escapes each element (a comma becomes %2C); the reader must therefore split the
        // payload BEFORE unescaping the segments, or an element containing the separator is torn
        // apart on read.
        var strings = new List<string> { "alpha,beta", "gamma", "de,l,ta" };

        RoundTripThroughDatabase(e => e.StringCollection = strings).StringCollection.Should().Equal(strings);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_string_elements_containing_the_format_sentinels()
    {
        // Hostile data: elements made entirely of the header and tag characters. The header is
        // written once, before any separator, and every tag is read positionally, so element data
        // that spells "!1", "v" or "n" cannot be mistaken for structure — the escaping turns "!"
        // into "%21", and "v"/"n" only mean something as the first character of a segment.
        var strings = new List<string> { "!1", "v", "n", "!1,v,n", "vvv", string.Empty };

        RoundTripThroughDatabase(e => e.StringCollection = strings).StringCollection.Should().Equal(strings);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_an_empty_value_typed_collection()
    {
        // An empty collection is stored as the bare header; the reader must map it back to an empty
        // collection instead of trying to decode a segment.
        RoundTripThroughDatabase(e => e.IntCollection = []).IntCollection.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void CollectionStringSplitConverter_should_round_trip_a_single_falsy_but_present_int(int value)
    {
        // Regression guard for the cardinality defect fixed in #119 and preserved here: a
        // one-element collection holding default(TValue) must not collapse to an empty collection.
        RoundTripThroughDatabase(e => e.IntCollection = [value]).IntCollection.Should().Equal(value);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_a_single_falsy_but_present_decimal()
    {
        RoundTripThroughDatabase(e => e.DecimalCollection = [0m]).DecimalCollection.Should().Equal(0m);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_a_single_falsy_but_present_bool()
    {
        var converter = new CollectionStringSplitConverter<bool>();

        RoundTrip(converter, [false]).Should().Equal(false);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_a_single_default_datetime()
    {
        RoundTripThroughDatabase(e => e.DatesCollection = [default]).DatesCollection.Should().Equal(default(DateTime));
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_null_string_elements()
    {
        // A null element is written as the "n" tag, which no value segment can produce.
        var strings = new List<string> { "alpha", null!, "beta" };

        RoundTripThroughDatabase(e => e.StringCollection = strings).StringCollection.Should().Equal(strings);
    }

    [Theory]
    [MemberData(nameof(EmptyAndNullStringMatrix))]
    public void CollectionStringSplitConverter_should_distinguish_empty_string_elements_from_null_elements(string?[] elements, string expectedPayload)
    {
        // The headline defect from #121: an empty string element used to be written as an empty
        // segment, which was also the encoding for null, so ["a", ""] read back as ["a", null] and
        // both [""] and [null] collapsed to []. The mandatory v/n tag makes all of these distinct.
        var converter = new CollectionStringSplitConverter<string>();
        var input = elements.ToList();

        var encoded = (string)converter.ConvertToProvider(input)!;

        encoded.Should().Be(expectedPayload);
        RoundTrip(converter, input!).Should().Equal(input!);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_distinguish_a_single_empty_string_from_a_single_null_and_from_an_empty_collection()
    {
        // The residual cardinality hole from #119, now closed: three collections that previously
        // shared the empty payload are three distinct payloads.
        var converter = new CollectionStringSplitConverter<string>();

        var empty = (string)converter.ConvertToProvider(new List<string>())!;
        var singleEmptyString = (string)converter.ConvertToProvider(new List<string> { string.Empty })!;
        var singleNull = (string)converter.ConvertToProvider(new List<string> { null! })!;

        empty.Should().Be("!1");
        singleEmptyString.Should().Be("!1,v");
        singleNull.Should().Be("!1,n");
        new[] { empty, singleEmptyString, singleNull }.Should().OnlyHaveUniqueItems();

        ((ICollection<string>)converter.ConvertFromProvider(empty)!).Should().BeEmpty();
        ((ICollection<string>)converter.ConvertFromProvider(singleEmptyString)!).Should().Equal(string.Empty);
        ((ICollection<string>)converter.ConvertFromProvider(singleNull)!).Should().ContainSingle().Which.Should().BeNull();
    }

    [Fact]
    public void CollectionStringSplitConverter_should_preserve_datetime_sub_second_precision_to_the_tick()
    {
        // Defect 2 from #121. The general ("G") invariant format has no fractional-seconds field, so
        // 10:30:45.1230000 used to be stored as "10:30:45" and reloaded as 10:30:45.0000000 —
        // silent corruption with no exception. The "O" round-trip format carries all seven digits.
        var value = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Unspecified).AddTicks(1_230_000);
        var converter = new CollectionStringSplitConverter<DateTime>();

        var reloaded = RoundTrip(converter, [value]).Single();

        reloaded.Ticks.Should().Be(value.Ticks);
        reloaded.Millisecond.Should().Be(123);
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void CollectionStringSplitConverter_should_preserve_datetime_kind(DateTimeKind kind)
    {
        // Defect 3 from #121. Kind is asserted EXPLICITLY: DateTime equality compares ticks only, so
        // Utc and Local reading back as Unspecified was invisible to Should().Equal(...) — which is
        // precisely how the bug survived a round-trip test.
        var value = new DateTime(2024, 6, 30, 22, 15, 5, kind).AddTicks(4_567_891);
        var converter = new CollectionStringSplitConverter<DateTime>();

        var reloaded = RoundTrip(converter, [value]).Single();

        reloaded.Kind.Should().Be(kind);
        reloaded.Ticks.Should().Be(value.Ticks);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_guid_elements()
    {
        // Defect 4 from #121: Convert.ChangeType throws "Invalid cast from 'System.String' to
        // 'System.Guid'", so a Guid collection used to write correctly and then fail on every read.
        var guids = new List<Guid> { Guid.NewGuid(), Guid.Empty, Guid.NewGuid() };

        RoundTripThroughDatabase(e => e.GuidCollection = guids).GuidCollection.Should().Equal(guids);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_enum_elements()
    {
        var statuses = new List<ConverterTestStatus> { ConverterTestStatus.None, ConverterTestStatus.Retired, ConverterTestStatus.Active };

        RoundTripThroughDatabase(e => e.StatusCollection = statuses).StatusCollection.Should().Equal(statuses);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_nullable_int_elements()
    {
        // A Nullable<T> element is decoded through its underlying type, because the null case is
        // carried by the segment tag rather than by the value text.
        var converter = new CollectionStringSplitConverter<int?>();
        var values = new List<int?> { 1, null, 0, null };

        var encoded = (string)converter.ConvertToProvider(values)!;

        encoded.Should().Be("!1,v1,n,v0,n");
        RoundTrip(converter, values).Should().Equal(values);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_timespan_elements()
    {
        var converter = new CollectionStringSplitConverter<TimeSpan>();
        var values = new List<TimeSpan> { TimeSpan.Zero, new(1, 2, 3, 4, 5), TimeSpan.FromTicks(-1234567) };

        RoundTrip(converter, values).Should().Equal(values);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_datetimeoffset_elements_including_the_offset()
    {
        var converter = new CollectionStringSplitConverter<DateTimeOffset>();
        var values = new List<DateTimeOffset>
                     {
                         new DateTimeOffset(2024, 1, 15, 10, 30, 45, TimeSpan.FromHours(2)).AddTicks(1_230_000),
                         new(2024, 1, 15, 10, 30, 45, TimeSpan.Zero),
                     };

        var reloaded = RoundTrip(converter, values);

        reloaded.Should().Equal(values);

        // DateTimeOffset equality compares the instant, so the offset itself needs its own
        // assertion — the same trap as DateTimeKind.
        reloaded.Select(v => v.Offset).Should().Equal(TimeSpan.FromHours(2), TimeSpan.Zero);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_round_trip_dateonly_and_timeonly_elements()
    {
        var dates = new List<DateOnly> { new(2024, 2, 29), DateOnly.MinValue };
        var times = new List<TimeOnly> { new(23, 59, 59, 999), TimeOnly.MinValue };

        RoundTrip(new CollectionStringSplitConverter<DateOnly>(), dates).Should().Equal(dates);
        RoundTrip(new CollectionStringSplitConverter<TimeOnly>(), times).Should().Equal(times);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_reject_an_element_type_it_cannot_round_trip()
    {
        // A type outside the supported set now fails on write with a message naming the type,
        // instead of serialising into something that throws InvalidCastException on read.
        var converter = new CollectionStringSplitConverter<Uri>();

        var act = () => converter.ConvertToProvider(new List<Uri> { new("https://example.invalid") });

        act.Should().Throw<NotSupportedException>().WithMessage("*Uri*");
    }

    [Theory]
    [InlineData("1,0,2")]
    [InlineData("1,,2")]
    [InlineData("")]
    [InlineData("!2,v1")]
    [InlineData("alpha")]
    public void CollectionStringSplitConverter_should_reject_a_payload_without_the_current_format_header(string payload)
    {
        // Legacy payloads are rejected rather than read best-effort. Under the old rules an empty
        // segment meant BOTH null and the empty string, so a best-effort read would hand back data
        // that is quietly wrong. Nothing readable is lost: the pre-#119 read path threw
        // InvalidCastException for every payload, and the #119 format never reached a release.
        var converter = new CollectionStringSplitConverter<int>();

        var act = () => converter.ConvertFromProvider(payload);

        act.Should().Throw<FormatException>().WithMessage($"*{Header}*");
    }

    [Theory]
    [InlineData("!1x")]
    [InlineData("!1,x")]
    [InlineData("!1,")]
    [InlineData("!1,nx")]
    public void CollectionStringSplitConverter_should_reject_a_malformed_tagged_payload(string payload)
    {
        var converter = new CollectionStringSplitConverter<string>();

        var act = () => converter.ConvertFromProvider(payload);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void CollectionStringSplitConverter_should_write_an_empty_collection_as_the_bare_header()
    {
        var converter = new CollectionStringSplitConverter<int>();

        converter.ConvertToProvider(new List<int>()).Should().Be(Header);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_write_every_element_tagged_and_separator_introduced()
    {
        // Pins the wire format itself, so a change to it cannot pass unnoticed.
        var converter = new CollectionStringSplitConverter<int>();

        converter.ConvertToProvider(new List<int> { 1, 0, 2 }).Should().Be("!1,v1,v0,v2");
    }

    [Fact]
    public void CollectionStringSplitConverter_should_keep_elements_intact_when_the_separator_is_escapable()
    {
        // Escaping is what protects a separator occurring inside an element. The default "," is
        // escaped to %2C, so an element containing a comma survives.
        var converter = new CollectionStringSplitConverter<string>();

        var encoded = (string)converter.ConvertToProvider(new List<string> { "a,b", "c" })!;

        encoded.Should().Be("!1,va%2Cb,vc");
        ((ICollection<string>)converter.ConvertFromProvider(encoded)!).Should().Equal("a,b", "c");
    }

    [Theory]
    [InlineData("-")]
    [InlineData(".")]
    [InlineData("_")]
    [InlineData("~")]
    [InlineData("x")]
    [InlineData("7")]
    [InlineData("a-b")]
    [InlineData("%")]
    [InlineData("%2C")]
    [InlineData("%20")]
    [InlineData("~._-")]
    public void CollectionStringSplitConverter_constructor_should_reject_a_separator_that_can_occur_in_escaped_data(string separator)
    {
        // Escaped element data is drawn from exactly two sources: the RFC 3986 unreserved
        // characters (A-Z a-z 0-9 - . _ ~) emitted literally, and percent-triplets, which introduce
        // '%'. A separator built only from those can appear inside an element and tear it apart:
        //   "-"   -> the element "a-b" is written as "a-b"
        //   "%"   -> an element containing a space is written as "a%20b"
        //   "%2C" -> an element containing a comma is written as "a%2Cb" — the separator's spelling
        // Testing only whether escaping *changes* the separator is not enough, because "%2C"
        // escapes to "%252C" and would have passed such a check.
        var act = () => new CollectionStringSplitConverter<string>(separator);

        act.Should().Throw<ArgumentException>().WithParameterName(nameof(separator));
    }

    [Theory]
    [InlineData(",", "a,b")]
    [InlineData(";", "a;b")]
    [InlineData("|", "a|b")]
    [InlineData("!", "a!b")]
    public void CollectionStringSplitConverter_should_not_tear_an_element_that_contains_the_separator(string separator, string element)
    {
        // The property the guard exists to protect: an element containing the separator survives,
        // because the separator is escaped inside the element but not between elements. "!" is
        // included because it is also the header sentinel — the header is written before any
        // separator, so the two never interfere.
        var converter = new CollectionStringSplitConverter<string>(separator);

        var encoded = (string)converter.ConvertToProvider(new List<string> { element, "tail" })!;

        encoded.Should().Contain(separator);
        ((ICollection<string>)converter.ConvertFromProvider(encoded)!).Should().Equal(element, "tail");
    }

    [Theory]
    [InlineData(",")]
    [InlineData(";")]
    [InlineData("|")]
    [InlineData("::")]
    [InlineData("!")]
    public void CollectionStringSplitConverter_constructor_should_accept_a_separator_containing_an_escapable_character(string separator)
    {
        // The mirror of the guard: any separator escaping changes is safe, because an occurrence
        // inside an element is escaped and therefore cannot be mistaken for a delimiter.
        var converter = new CollectionStringSplitConverter<string>(separator);
        var values = new List<string> { $"a{separator}b", "c", string.Empty, null! };

        RoundTrip(converter, values).Should().Equal(values);
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
        // short-circuiting them. Null maps to null in both directions, which also makes a null
        // collection distinguishable from an empty one.
        var converter = new CollectionStringSplitConverter<string>();

        converter.ConvertToProvider(null).Should().BeNull();
        converter.ConvertFromProvider(null).Should().BeNull();
    }

    [Fact]
    public void CollectionStringSplitConverter_should_distinguish_a_null_collection_from_an_empty_one()
    {
        var converter = new CollectionStringSplitConverter<int>();

        converter.ConvertToProvider(null).Should().BeNull();
        converter.ConvertToProvider(new List<int>()).Should().Be(Header);

        converter.ConvertFromProvider(null).Should().BeNull();
        ((ICollection<int>)converter.ConvertFromProvider(Header)!).Should().BeEmpty();
    }

    [Fact]
    public void CollectionStringSplitConverter_should_save_and_reload_a_null_collection_on_an_optional_property()
    {
        // The end-to-end form of the null guard. It uses the nullable property because the
        // converter writes SQL NULL for a null collection, which a NOT NULL column correctly
        // rejects.
        var reloaded = RoundTripThroughDatabase(e => e.OptionalStringCollection = null);

        reloaded.OptionalStringCollection.Should().BeNull();
        reloaded.IntCollection.Should().Equal(1, 2);
    }

    [Fact]
    public void CollectionStringSplitConverter_should_distinguish_null_from_empty_on_an_optional_property_end_to_end()
    {
        // A null collection and an empty one must remain distinguishable after a database
        // round-trip, not merely at the converter level.
        var withNull = CreateFullyPopulatedEntity(e => e.OptionalStringCollection = null);
        var withEmpty = CreateFullyPopulatedEntity(e => e.OptionalStringCollection = []);

        DbContext.TestEntities.AddRange(withNull, withEmpty);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        DbContext.TestEntities.Single(t => t.Id == withNull.Id).OptionalStringCollection.Should().BeNull();
        DbContext.TestEntities.Single(t => t.Id == withEmpty.Id).OptionalStringCollection.Should().NotBeNull().And.BeEmpty();
    }

    /// <summary>
    ///     Builds the payload the converter is expected to produce, mirroring the write format:
    ///     the header, then one separator-introduced, tagged segment per element.
    /// </summary>
    /// <typeparam name="TValue">The element type.</typeparam>
    /// <param name="values">The elements.</param>
    /// <param name="encode">Produces an element's textual form, before escaping.</param>
    /// <returns>The expected payload.</returns>
    private static string Serialise<TValue>(IEnumerable<TValue> values, Func<TValue, string?> encode) =>
        Header + string.Concat(values.Select(value => "," + (encode(value) is { } text ? "v" + Uri.EscapeDataString(text) : "n")));

    private static ICollection<TValue> RoundTrip<TValue>(CollectionStringSplitConverter<TValue> converter, ICollection<TValue> values) =>
        (ICollection<TValue>)converter.ConvertFromProvider(converter.ConvertToProvider(values))!;

    private static ConverterTestEntity CreateFullyPopulatedEntity(Action<ConverterTestEntity>? configure = null)
    {
        // Every collection is populated so that a test which exercises one property is not also
        // silently exercising the empty-collection path on the others.
        var entity = new ConverterTestEntity
                     {
                         StringCollection = ["alpha", "beta"],
                         IntCollection = [1, 2],
                         DatesCollection = [new(2024, 1, 2, 3, 4, 5, DateTimeKind.Unspecified)],
                         DecimalCollection = [1.5m],
                         GuidCollection = [Guid.Empty],
                         StatusCollection = [ConverterTestStatus.Active],
                     };

        configure?.Invoke(entity);

        return entity;
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

    /// <summary>
    ///     Saves a fully populated entity with the given override applied, then reloads it from the
    ///     database with the change tracker cleared so the converter's read path really runs.
    /// </summary>
    /// <param name="configure">Applies the property under test.</param>
    /// <returns>The reloaded entity.</returns>
    private ConverterTestEntity RoundTripThroughDatabase(Action<ConverterTestEntity> configure)
    {
        var entity = CreateFullyPopulatedEntity(configure);

        DbContext.TestEntities.Add(entity);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        return DbContext.TestEntities.Single(t => t.Id == entity.Id);
    }

    private void ValidateConverterEntities<TValue>(Func<ConverterTestEntity, ICollection<TValue>> entityPropertyFunc,
                                                   Action<ConverterTestEntity, List<TValue>> entitySetPropertyFunc,
                                                   List<TValue> firstList,
                                                   List<TValue> secondList,
                                                   Expression<Func<ConverterTestEntity, bool>> findEntityExpression)
    {
        var converterTestEntity1 = CreateFullyPopulatedEntity(e => entitySetPropertyFunc(e, firstList));
        var converterTestEntity2 = CreateFullyPopulatedEntity(e => entitySetPropertyFunc(e, secondList));

        DbContext.TestEntities.Add(converterTestEntity1);
        DbContext.TestEntities.Add(converterTestEntity2);
        DbContext.SaveChanges();

        // Without this the query is satisfied from the change tracker's identity map, so the
        // converter's read path is never invoked and the assertions below only compare an entity
        // with itself. Clearing forces a real materialisation from the database.
        DbContext.ChangeTracker.Clear();

        var queriedEntity = DbContext.TestEntities.FirstOrDefault(findEntityExpression);

        queriedEntity.Should().BeEquivalentTo(converterTestEntity2);
        entityPropertyFunc(queriedEntity!).Should().Equal(secondList);
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

        // Guid and an enum are neither convertible by Convert.ChangeType nor round-trippable through
        // Convert.ToString, so before the format revision these mappings wrote successfully and then
        // threw InvalidCastException on every read.
        modelBuilder.Entity<ConverterTestEntity>().Property(e => e.GuidCollection).HasConversion(new CollectionStringSplitConverter<Guid>());
        modelBuilder.Entity<ConverterTestEntity>()
                    .Property(e => e.StatusCollection)
                    .HasConversion(new CollectionStringSplitConverter<ConverterTestStatus>());

        // CS8620: the property is ICollection<string>? while the converter is declared over the
        // non-nullable ICollection<string>. The variance is safe here because the converter maps
        // null to null in both directions rather than throwing.
#pragma warning disable CS8620
        modelBuilder.Entity<ConverterTestEntity>().Property(e => e.OptionalStringCollection).HasConversion(new CollectionStringSplitConverter<string>());
#pragma warning restore CS8620
        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>
///     An enum element type, exercising a <typeparamref name="TValue" /> that
///     <see cref="Convert.ChangeType(object, Type, IFormatProvider)" /> cannot read back.
/// </summary>
public enum ConverterTestStatus
{
    /// <summary>The default member, so a single-element collection holding it is also a falsy-but-present case.</summary>
    None = 0,

    /// <summary>An ordinary member.</summary>
    Active = 1,

    /// <summary>A second ordinary member.</summary>
    Retired = 2,
}

public class ConverterTestEntity : IHasId<int>
{
    [Key]
    public int Id { get; set; }

    public virtual ICollection<string> StringCollection { get; set; } = new List<string>();

    public virtual ICollection<int> IntCollection { get; set; } = new List<int>();

    public virtual ICollection<DateTime> DatesCollection { get; set; } = new List<DateTime>();

    public virtual ICollection<decimal> DecimalCollection { get; set; } = [];

    public virtual ICollection<Guid> GuidCollection { get; set; } = [];

    public virtual ICollection<ConverterTestStatus> StatusCollection { get; set; } = [];

    /// <summary>
    ///     Declared nullable on purpose, so EF Core maps it to a nullable column and the converter's
    ///     null handling can be exercised end to end. The non-nullable properties above map to
    ///     NOT NULL columns, where a null collection is correctly rejected by the database.
    /// </summary>
    public virtual ICollection<string>? OptionalStringCollection { get; set; }
}
