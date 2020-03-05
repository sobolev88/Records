using NUnit.Framework;
using Records;

namespace RecordsTests
{
    public class RecordTests
    {
        [Test]
        public void Test1()
        {
            new TestRecord();
        }
    }

    [Record]
    internal class TestRecord
    {

    }
}