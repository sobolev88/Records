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
            typeof(OneRequiredIntFieldRecord).Should().NotHaveDefaultConstructor();
        }


        [Test]
        public void OneRequiredValueFieldTest()
        {
            var record = new OneRequiredIntFieldRecord(5);
            record.Id.Should().Be(5);
        }

        [Test]
        public void TwoRequiredFieldsTest()
        {
            var record = new TwoRequiredFieldsRecord(5, "5");
            record.Id.Should().Be(5);
            record.Name.Should().Be("5");
        }

        [Test]
        public void OneNotRequiredIntFieldTest_WhenHasValue()
        {
            var record = new OneNotRequiredIntFieldRecord(5);
            record.Id.Should().Be(5);
        }

        [Test]
        public void OneNotRequiredIntFieldTest_WhenHasNoValue()
        {
            var record = new OneNotRequiredIntFieldRecord();
            record.Id.Should().BeNull();
        }
    }

    [Record]
    internal partial class EmptyRecord
    {
    }

    [Record]
    internal partial class OneRequiredIntFieldRecord
    {
        public int Id { get; }
    }

    [Record]
    internal partial class OneNotRequiredIntFieldRecord
    {
        public int? Id { get; }
    }

    [Record]
    internal partial class TwoRequiredFieldsRecord
    {
        public int Id { get; }

        public string Name { get; }
    }
}