using Xunit;
using YPermitin.DBTools.Core.Helpers;

namespace YPermitin.DBTools.Core.Tests.Helpers
{
    public class EnumHelper_Tests
    {
        [Fact]
        public void GetEnumValueByName_Test()
        {
            var enumValue = EnumHelper.GetEnumValueByName<TestEnum>("Value2");

            Assert.Equal(TestEnum.Value2, enumValue);
        }

        [Fact]
        public void GetWrongValueByName_Test()
        {
            var enumValue = EnumHelper.GetEnumValueByName<TestEnum>("Value999");
            var firstEnumIndex = 0;
            var enumEmpty = (TestEnum)firstEnumIndex;

            Assert.Equal(enumEmpty, enumValue);
        }

        public enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }
    }
}
