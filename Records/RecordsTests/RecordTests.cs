using FluentAssertions;
using NUnit.Framework;
using Records;

namespace RecordsTests
{
    public class RecordTests
    {
        [Test]
        public void EmptyTest()
        {
            new EmptyRecord();
        }

        [Test]
        public void Record_MustNotHave_DefaultConstructor()
        {
            typeof(IntRecord).Should().NotHaveDefaultConstructor();
        }


        [Test]
        public void IntTest()
        {
            var record = new IntRecord(5);
            record.Id.Should().Be(5);
        }

        [Test]
        public void StringTest()
        {
            var record = new StringRecord("5");
            record.Id.Should().Be("5");
        }

        [Test]
        public void IntAndStringTest()
        {
            var record = new IntAndStringRecord(5, "5");
            record.Id.Should().Be(5);
            record.Name.Should().Be("5");
        }

        [Test]
        public void NullableIntTest_WhenHasValue()
        {
            var record = new NullableIntRecord(5);
            record.Id.Should().Be(5);
        }

        [Test]
        public void NullableIntTest_WhenNoValue()
        {
            var record = new NullableIntRecord();
            record.Id.Should().BeNull();
        }

        [Test]
        public void NullableStringTest_WhenHasValue()
        {
            var record = new NullableStringRecord("5");
            record.Id.Should().Be("5");
        }

        [Test]
        public void NullableStringTest_WhenNoValue()
        {
            var record = new NullableStringRecord();
            record.Id.Should().BeNull();
        }

        [Test]
        public void NullableIntAndStringTest_WhenAllValues()
        {
            var record = new NullableIntAndStringRecord("5", 5);
            record.Id.Should().Be(5);
            record.Name.Should().Be("5");
        }

        [Test]
        public void NullableIntAndStringTest_WhenOnlyRequiredValues()
        {
            var record = new NullableIntAndStringRecord("5");
            record.Id.Should().BeNull();
            record.Name.Should().Be("5");
        }
    }

    [Record]
    internal partial class EmptyRecord
    {
    }

    [Record]
    internal partial class IntRecord
    {
        public int Id { get; }
    }

    [Record]
    internal partial class StringRecord
    {
        public string Id { get; }
    }

    [Record]
    internal partial class NullableIntRecord
    {
        public int? Id { get; }
    }

    [Record]
    internal partial class NullableStringRecord
    {
        public string? Id { get; }
    }

    [Record]
    internal partial class IntAndStringRecord
    {
        public int Id { get; }

        public string Name { get; }
    }

    [Record]
    internal partial class NullableIntAndStringRecord
    {
        public int? Id { get; }

        public string Name { get; }
    }
}