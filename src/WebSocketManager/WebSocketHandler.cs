using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebSocketManager.Common;

namespace WebSocketManager
{
    public abstract class WebSocketHandler
    {
        protected WebSocketConnectionManager WebSocketConnectionManager { get; set; }

        private JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public string SocketId { get; set; }

        public WebSocketHandler(WebSocketConnectionManager webSocketConnectionManager)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        public virtual async Task OnConnected(WebSocket socket)
        {
            WebSocketConnectionManager.AddSocket(socket);
            SocketId = WebSocketConnectionManager.GetId(socket);

            await SendMessageAsync(socket, new Message()
            {
                MessageType = MessageType.ConnectionEvent,
                Data = SocketId
            });
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await WebSocketConnectionManager.RemoveSocket(WebSocketConnectionManager.GetId(socket));
        }

        public async Task SendMessageAsync(WebSocket socket, Message message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            var serializedMessage = JsonConvert.SerializeObject(message, _jsonSerializerSettings);
            await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(serializedMessage),
                                                                  offset: 0,
                                                                  count: serializedMessage.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);
        }

        public async Task SendMessageAsync(string socketId, Message message)
        {
            await SendMessageAsync(WebSocketConnectionManager.GetSocketById(socketId), message);
        }

        public async Task SendMessageToAllAsync(Message message)
        {
            foreach (var pair in WebSocketConnectionManager.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open)
                    await SendMessageAsync(pair.Value, message);
            }
        }

        public async Task InvokeClientMethodAsync(string socketId, string operationId, string controller, string action, object[] arguments)
        {
            var message = new Message()
            {
                MessageType = MessageType.ClientMethodInvocation,
                Data = JsonConvert.SerializeObject(new InvocationDescriptor()
                {
                    OperationId = operationId,
                    Controller = controller,
                    Action = action,
                    Arguments = arguments
                }, _jsonSerializerSettings)
            };

            await SendMessageAsync(socketId, message);
        }

        public async Task InvokeClientMethodToAllAsync(string operationId, string controller, string action, params object[] arguments)
        {
            foreach (var pair in WebSocketConnectionManager.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open)
                    await InvokeClientMethodAsync(pair.Key, operationId, controller, action, arguments);
            }
        }

        public virtual async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var serializedInvocationDescriptor = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var invocationDescriptor = JsonConvert.DeserializeObject<InvocationDescriptor>(serializedInvocationDescriptor);

            // Make sure that only public methods can be invoked by clients.
            var method = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m =>
                                            m.Name.Equals(invocationDescriptor.Action, StringComparison.OrdinalIgnoreCase));

            // Check if the Access attribute has been set for the method and if so get it.
            var clientAccess = method?.GetCustomAttribute<ClientAccessAttribute>();

            // If the method does not exist or client access has been denied to it then return an error.
            if (method == null || (clientAccess != null && clientAccess.AccessibilityOptions == ClientAccessibilityOptions.DenyAll))
            {
                await SendMessageAsync(socket, new Message()
                {
                    MessageType = MessageType.Text,
                    Data = $"Cannot find method {invocationDescriptor.Action}"
                });
                return;
            }

            try
            {
                method.Invoke(this, invocationDescriptor.Arguments);
            }
            catch (TargetParameterCountException e)
            {
                await SendMessageAsync(socket, new Message()
                {
                    MessageType = MessageType.Text,
                    Data = $"The {invocationDescriptor.Action} method does not take {invocationDescriptor.Arguments.Length} parameters!"
                });
            }

            catch (ArgumentException e)
            {
                await SendMessageAsync(socket, new Message()
                {
                    MessageType = MessageType.Text,
                    Data = $"The {invocationDescriptor.Action} method takes different arguments!"
                });
            }
        }
    }
}