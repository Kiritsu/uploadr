using NUnit.Framework;
using UploadR.Utilities;

namespace UploadR.Test
{
    //email source https://gist.github.com/cjaoude/fd9910626629b53c4d25
    public class RegexUtilitiesTests
    {
        [TestCaseSource(nameof(ValidEmailsSource))]
        public void ValidEmailTest(string email)
        {
            Assert.True(RegexUtilities.IsValidEmail(email));
        }

        [TestCaseSource(nameof(InvalidEmailsSource))]
        public void InvalidEmailTest(string email)
        {
            Assert.False(RegexUtilities.IsValidEmail(email));
        }
        
        [Test]
        public void LongValidEmailTest()
        {
            var longEmail = $"{new string('k', 10000000)}@email.com";
            Assert.False(RegexUtilities.IsValidEmail(longEmail));
        }
        
        [Test]
        public void LongInvalidEmailTest()
        {
            var longEmail = $"{new string('@', 10000000)}";
            Assert.False(RegexUtilities.IsValidEmail(longEmail));
        }
        
        private static string[] ValidEmailsSource()
        {
            return new[]
            {
                "email@example.com",
                "firstname.lastname@example.com",
                "email@subdomain.example.com",
                "firstname+lastname@example.com",
                "email@123.123.123.123",
                "email@[123.123.123.123]",
                "email@example.com",
                "1234567890@example.com",
                "email@example-one.com",
                "email@example.name",
                "email@example.museum",
                "email@example.co.jp",
                "firstname-lastname@example.com"

                // todo regex doesn't match these, but they are valid
                // "_______@example.com"
                // "much.”more\\ unusual”@example.com",
                // "very.unusual.”@”.unusual.com@example.com",
                // "very.”(),:;<>[]”.VERY.”very@\\ \"very”.unusual@strange.example.com"
            };
        }

        private static string[] InvalidEmailsSource()
        {
            return new[]
            {
                "",
                "plainaddress",
                "#@%^%#$@#$@#.com",
                "@example.com",
                "Joe Smith <email@example.com>",
                "email.example.com",
                "email@example@example.com",
                ".email@example.com",
                "email.@example.com",
                "email..email@example.com",
                "あいうえお@example.com",
                "email@example.com (Joe Smith)",
                "email@example",
                "email@-example.com",
                "email@example..com",
                "Abc..123@example.com"
                
                // todo this should fail
                // "email@111.222.333.44444"
            };
        }
    }
}