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
        public void OneRequiredValueFieldTest()
        {
            var record = new OneRequiredIntFieldRecord(5);
            record.Id.Should().Be(5);
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
}