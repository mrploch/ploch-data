using System;
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
        using var source = new DataColumn("Source", typeof(string)) { AllowDBNull = false };
        using var target = new DataColumn("Target", typeof(string));

        source.CopyProperties(target);

        target.AllowDBNull.Should().Be(source.AllowDBNull);
    }

    [Fact]
    public void CopyProperties_should_copy_AutoIncrement()
    {
        using var source = new DataColumn("Source", typeof(int)) { AutoIncrement = true };
        using var target = new DataColumn("Target", typeof(int));

        source.CopyProperties(target);

        target.AutoIncrement.Should().Be(source.AutoIncrement);
    }

    [Fact]
    public void CopyProperties_should_copy_Caption()
    {
        using var source = new DataColumn("Source", typeof(string)) { Caption = "Test Caption" };
        using var target = new DataColumn("Target", typeof(string));

        source.CopyProperties(target);

        target.Caption.Should().Be("Test Caption");
    }

    [Fact]
    public void CopyProperties_should_copy_AutoIncrementSeed()
    {
        using var source = new DataColumn("Source", typeof(int)) { AutoIncrementSeed = 100 };
        using var target = new DataColumn("Target", typeof(int));

        source.CopyProperties(target);

        target.AutoIncrementSeed.Should().Be(100);
    }

    [Fact]
    public void CopyProperties_should_copy_AutoIncrementStep()
    {
        using var source = new DataColumn("Source", typeof(int)) { AutoIncrementStep = 5 };
        using var target = new DataColumn("Target", typeof(int));

        source.CopyProperties(target);

        target.AutoIncrementStep.Should().Be(5);
    }

    [Fact]
    public void CopyProperties_should_copy_all_properties_at_once()
    {
        using var source = new DataColumn("Source", typeof(int))
        {
            AllowDBNull = false,
            AutoIncrement = true,
            Caption = "My Column",
            AutoIncrementSeed = 10,
            AutoIncrementStep = 2,
        };
        using var target = new DataColumn("Target", typeof(int));

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
        using var source = new DataColumn("Source", typeof(string));
        using var target = new DataColumn("Target", typeof(string));

        source.CopyProperties(target);

        target.ColumnName.Should().Be("Target");
    }

    [Fact]
    public void CopyProperties_should_throw_ArgumentNullException_when_source_is_null()
    {
        using var target = new DataColumn("Target", typeof(string));

        var act = () => ((DataColumn)null!).CopyProperties(target);

        act.Should().Throw<ArgumentNullException>().WithParameterName("sourceColumn");
    }

    [Fact]
    public void CopyProperties_should_throw_ArgumentNullException_when_target_is_null()
    {
        using var source = new DataColumn("Source", typeof(string));

        var act = () => source.CopyProperties(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("targetColumn");
    }
}
