using System.Text.Json;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using NUnit.Framework;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class UserJsonConverterTest
    {
        private JsonSerializerOptions _options;

        [SetUp]
        public void Setup()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new UserJsonConverter());
        }

        [Test]
        public void Deserialize_ValidUserJson_AllFieldsSet()
        {
            string json = "{" +
                "\"id\":1," +
                "\"username\":\"testuser\"," +
                "\"email\":\"test@example.com\"," +
                "\"usertype\":2," +
                "\"balance\":100.5," +
                "\"rating\":4.5" +
                "}";
            var user = JsonSerializer.Deserialize<User>(json, _options);
            Assert.That(user.Id, Is.EqualTo(1));
            Assert.That(user.Username, Is.EqualTo("testuser"));
            Assert.That(user.Email, Is.EqualTo("test@example.com"));
            Assert.That(user.UserType, Is.EqualTo(2));
            Assert.That(user.Balance, Is.EqualTo(100.5f).Within(0.001));
            Assert.That(user.Rating, Is.EqualTo(4.5f).Within(0.001));
        }

        [Test]
        public void Deserialize_UserJson_MissingOptionalFields_DoesNotThrow()
        {
            string json = "{" +
                "\"id\":2," +
                "\"username\":\"user2\"," +
                "\"email\":\"user2@example.com\"," +
                "\"usertype\":1," +
                "\"balance\":0.0" +
                "}";
            var user = JsonSerializer.Deserialize<User>(json, _options);
            Assert.That(user.Id, Is.EqualTo(2));
            Assert.That(user.Username, Is.EqualTo("user2"));
            Assert.That(user.Email, Is.EqualTo("user2@example.com"));
            Assert.That(user.UserType, Is.EqualTo(1));
            Assert.That(user.Balance, Is.EqualTo(0.0f).Within(0.001));
        }

        [Test]
        public void Deserialize_InvalidJson_ThrowsJsonException()
        {
            string json = "[1,2,3]";
            Assert.Throws<System.Text.Json.JsonException>(() =>
                JsonSerializer.Deserialize<User>(json, _options));
        }
    }
} 