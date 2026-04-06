using System.Data;
using FluentAssertions;
using Ploch.Data.Utilities;
using Xunit;

namespace Ploch.Data.Utilities.Tests;

public class DataColumnExtensionsTests
{
    [Fact]
    public void CopyProperties_should_copy_AllowDBNull()
    {
        var source = new DataColumn("Source", typeof(string)) { AllowDBNull = false };
        var target = new DataColumn("Target", typeof(string));

        source.CopyProperties(target);

        target.AllowDBNull.Should().Be(source.AllowDBNull);
    }

    [Fact]
    public void CopyProperties_should_copy_AutoIncrement()
    {
        var source = new DataColumn("Source", typeof(int)) { AutoIncrement = true };
        var target = new DataColumn("Target", typeof(int));

        source.CopyProperties(target);

        target.AutoIncrement.Should().Be(source.AutoIncrement);
    }

    [Fact]
    public void CopyProperties_should_copy_Caption()
    {
        var source = new DataColumn("Source", typeof(string)) { Caption = "Test Caption" };
        var target = new DataColumn("Target", typeof(string));

        source.CopyProperties(target);

        target.Caption.Should().Be("Test Caption");
    }

    [Fact]
    public void CopyProperties_should_copy_AutoIncrementSeed()
    {
        var source = new DataColumn("Source", typeof(int)) { AutoIncrementSeed = 100 };
        var target = new DataColumn("Target", typeof(int));

        source.CopyProperties(target);

        target.AutoIncrementSeed.Should().Be(100);
    }

    [Fact]
    public void CopyProperties_should_copy_AutoIncrementStep()
    {
        var source = new DataColumn("Source", typeof(int)) { AutoIncrementStep = 5 };
        var target = new DataColumn("Target", typeof(int));

        source.CopyProperties(target);

        target.AutoIncrementStep.Should().Be(5);
    }

    [Fact]
    public void CopyProperties_should_copy_all_properties_at_once()
    {
        var source = new DataColumn("Source", typeof(int))
        {
            AllowDBNull = false,
            AutoIncrement = true,
            Caption = "My Column",
            AutoIncrementSeed = 10,
            AutoIncrementStep = 2,
        };
        var target = new DataColumn("Target", typeof(int));

        source.CopyProperties(target);

        target.AllowDBNull.Should().Be(false);
        target.AutoIncrement.Should().Be(true);
        target.Caption.Should().Be("My Column");
        target.AutoIncrementSeed.Should().Be(10);
        target.AutoIncrementStep.Should().Be(2);
    }

    [Fact]
    public void CopyProperties_should_not_copy_column_name()
    {
        var source = new DataColumn("Source", typeof(string));
        var target = new DataColumn("Target", typeof(string));

        source.CopyProperties(target);

        target.ColumnName.Should().Be("Target");
    }
}
