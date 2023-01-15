using EGD.Command.Models;
using EGD.Command.Models.Enums;
using EGD.Command.Models.Extensions;
using Newtonsoft.Json;
using System.Text;

namespace EGD.Command.Test
{
    public class ConvertToMessageExtensionsTest
    {
        [Fact]
        public void ConvertToMessage_HappyPath_ReturnsMessageWithExpectedUserPropsAndCommandObject()
        {
            var expectedStatus = CommandStatus.New;
            var expectedAction = "action";
            var request = "this very specific request";

            var message = request.ConvertToMessage(expectedStatus, expectedAction);

            var userProps = message.UserProperties;
            Assert.Equal(expectedAction, userProps["action"]);
            Assert.Equal(expectedStatus.ToString(), userProps["status"]);
            Assert.NotEqual(Guid.Empty.ToString(), message.CorrelationId);

            var command = JsonConvert.DeserializeObject<CommandProcessEvent<string>>(Encoding.UTF8.GetString(message.Body));
            Assert.Equal(request, command.ProcessData);
            Assert.Equal(expectedAction, command.Action);
            Assert.Equal(expectedStatus, command.Status);
            Assert.Null(command.EntityReferences);
            Assert.NotEqual(Guid.Empty, command.CorrelationId);
        }

        [Fact]
        public void ConvertToMessage_CorrelationIdIsProvided_ReturnsMessageWithMatchingCorrelationId()
        {
            var expectedCorrelationId = Guid.NewGuid();

            var message = expectedCorrelationId.ConvertToMessage(CommandStatus.New, "action", expectedCorrelationId);

            var command = JsonConvert.DeserializeObject<CommandProcessEvent<string>>(Encoding.UTF8.GetString(message.Body));
            Assert.Equal(expectedCorrelationId, command.CorrelationId);
            Assert.Equal(expectedCorrelationId.ToString(), message.CorrelationId);
        }

        [Fact]
        public void ConvertToMessage_HasUserProperties_ReturnsMessageWithContaininAdditionalUserPropeties()
        {
            var key = "key";
            var userProps = new Dictionary<string, object>
            {
                { key, "value" }
            };

            var request = "request";
            var message = request.ConvertToMessage(CommandStatus.New, "action", userProperties: userProps);

            Assert.Equal(3, message.UserProperties.Count);
            Assert.Equal(userProps[key], message.UserProperties[key]);
        }

        [Fact]
        public void ConvertToMessage_UserDefinedAction_ReturnsMessageWithExpectedAction()
        {
            var key = "action";
            var expectedAction = "action";
            var userProps = new Dictionary<string, object>
            {
                { key, "custom action" }
            };

            var request = "request";
            var message = request.ConvertToMessage(CommandStatus.New, expectedAction, userProperties: userProps);

            Assert.Equal(expectedAction, message.UserProperties[key]);
        }

        [Fact]
        public void ConvertToMessage_HasEntityReferences_ReturnsMessageWithEntityReferences()
        {
            var references = new List<EntityReference>
            {
                new EntityReference
                {
                    Entity = "test entity"
                }
            };

            var request = "request";
            var message = request.ConvertToMessage(CommandStatus.New, "action", entityReferences: references);

            var command = JsonConvert.DeserializeObject<CommandProcessEvent<string>>(Encoding.UTF8.GetString(message.Body));
            Assert.Equal(references.First().Entity, command.EntityReferences.First().Entity);
        }
    }
}