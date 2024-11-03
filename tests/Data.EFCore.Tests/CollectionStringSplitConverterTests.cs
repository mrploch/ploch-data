using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Objectivity.AutoFixture.XUnit2.AutoMoq.Attributes;
using Ploch.Data.EFCore.IntegrationTesting;
using Ploch.Data.Model;

namespace Ploch.Data.EFCore.Tests;

public class CollectionStringSplitConverterTests : DataIntegrationTest<ConverterTestDbContext>
{
    public static IEnumerable<object[]> Data =>
        new List<object[]> { new object[] { 1, 2, 3 }, new object[] { -4, -6, -10 }, new object[] { -2, 2, 0 }, new object[] { int.MinValue, -1, int.MaxValue } };

    [Theory]
    [AutoMockData]
    public void CollectionStringSplitConverter_should_convert_to_and_from_string_list(List<string> firstList, List<string> secondList)
    {
        DbContext.TestEntities.Add(new ConverterTestEntity { StringCollection = firstList });
        DbContext.TestEntities.Add(new ConverterTestEntity { StringCollection = secondList });
        DbContext.SaveChanges();

        var entity = DbContext.TestEntities.Skip(1).First();
        var queriedEntity = DbContext.TestEntities.FirstOrDefault(t => ((string)(object)t.StringCollection).Contains(secondList[1]));

        entity.Should().BeEquivalentTo(queriedEntity);
        entity.StringCollection.Should().HaveCount(secondList.Count);
        entity.StringCollection.Should().Contain(secondList);
    }

    [Theory]
    [AutoMockData]
    public void CollectionStringSplitConverter_should_handle_string_list(List<string> firstStringList, List<string> secondStringList)
    {
        ValidateConverterEntities(e => e.StringCollection,
                                  (e, v) => e.StringCollection = v,
                                  firstStringList,
                                  secondStringList,
                                  t => ((string)(object)t.StringCollection).Contains(secondStringList[1]));
    }

    [Theory]
    [AutoMockData]
    public void CollectionStringSplitConverter_should_handle_int_list(List<int> firstIntList, List<int> secondIntList)
    {
        ValidateConverterEntities(e => e.IntCollection,
                                  (e, v) => e.IntCollection = v,
                                  firstIntList,
                                  secondIntList,
                                  t => ((string)(object)t.IntCollection).Contains(secondIntList[1].ToString()));
    }

    [Theory]
    [AutoMockData]
    public void CollectionStringSplitConverter_should_handle_datetime_list(List<DateTime> firstDateTimeList, List<DateTime> secondDateTimeList)
    {
        ValidateConverterEntities(e => e.DatesCollection,
                                  (e, v) => e.DatesCollection = v,
                                  firstDateTimeList,
                                  secondDateTimeList,
                                  t => ((string)(object)t.DatesCollection).Contains(Uri.EscapeDataString(secondDateTimeList[1].ToString())));
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ConverterTestEntity>()
               .Property(e => e.StringCollection)
               .HasConversion(new CollectionStringSplitConverter<string>());
        builder.Entity<ConverterTestEntity>()
               .Property(e => e.IntCollection)
               .HasConversion(new CollectionStringSplitConverter<int>());
        builder.Entity<ConverterTestEntity>()
               .Property(e => e.DatesCollection)
               .HasConversion(new CollectionStringSplitConverter<DateTime>());
        base.OnModelCreating(builder);
    }
}

public class ConverterTestEntity : IHasId<int>
{
    [Key]
    public int Id { get; set; }

    public virtual ICollection<string> StringCollection { get; set; } = new List<string>();

    public virtual ICollection<int> IntCollection { get; set; } = new List<int>();

    public virtual ICollection<DateTime> DatesCollection { get; set; } = new List<DateTime>();
}