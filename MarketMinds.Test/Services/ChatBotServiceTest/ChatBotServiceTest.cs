using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Helper;

namespace MarketMinds.Tests.Services.ChatBotServiceTest
{
    [TestFixture]
    public class ChatBotServiceTest
    {
        private IChatbotService _chatBotService;
        private ChatBotRepositoryMock _chatBotRepositoryMock;

        [SetUp]
        public void Setup()
        {
            _chatBotRepositoryMock = new ChatBotRepositoryMock();
            _chatBotService = new ChatbotService(_chatBotRepositoryMock, AppConfig.Configuration);
        }

        [Test]
        public void TestInitializeChat_LoadsRootNode()
        {
            // Act
            var result = _chatBotService.InitializeChat();

            // Assert
            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public void TestInitializeChat_SetsIsActiveToTrue()
        {
            // Act
            _chatBotService.InitializeChat();

            // Assert
            Assert.That(_chatBotService.IsInteractionActive(), Is.True);
        }

        [Test]
        public void TestInitializeChat_CallsRepositoryLoad()
        {
            // Act
            _chatBotService.InitializeChat();

            // Assert
            Assert.That(_chatBotRepositoryMock.GetLoadCount(), Is.EqualTo(1));
        }

        [Test]
        public void TestSelectOption_ValidOption_ChangesCurrentNode()
        {
            // Arrange
            _chatBotService.InitializeChat();
            var options = _chatBotService.GetCurrentOptions();
            var optionToSelect = options.First();

            // Act
            var result = _chatBotService.SelectOption(optionToSelect);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestSelectOption_InvalidOption_ReturnsFalse()
        {
            // Arrange
            _chatBotService.InitializeChat();
            var invalidNode = new Node
            {
                Id = 999,
                ButtonLabel = "Invalid",
                LabelText = "Invalid Node",
                Response = "Invalid Node Response",
                Children = new List<Node>()
            };

            // Act
            var result = _chatBotService.SelectOption(invalidNode);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TestSelectOption_LastNode_SetsIsActiveToFalse()
        {
            // Arrange
            _chatBotService.InitializeChat();
            var options = _chatBotService.GetCurrentOptions();
            var lastOption = options.Last();
            lastOption.Children.Clear(); // Make it a leaf node

            // Act
            _chatBotService.SelectOption(lastOption);

            // Assert
            Assert.That(_chatBotService.IsInteractionActive(), Is.False);
        }

        [Test]
        public void TestGetCurrentOptions_ReturnsAllChildren()
        {
            // Arrange
            _chatBotService.InitializeChat();

            // Act
            var options = _chatBotService.GetCurrentOptions();

            // Assert
            Assert.That(options.Count(), Is.EqualTo(3));
        }

        [Test]
        public void TestGetCurrentResponse_ReturnsNodeResponse()
        {
            // Arrange
            _chatBotService.InitializeChat();

            // Act
            var response = _chatBotService.GetCurrentResponse();

            // Assert
            Assert.That(response, Is.EqualTo("Welcome to the chat service"));
        }

        [Test]
        public void TestIsInteractionActive_WhenNotInitialized_ReturnsFalse()
        {
            // Act & Assert
            Assert.That(_chatBotService.IsInteractionActive(), Is.False);
        }
    }
}
