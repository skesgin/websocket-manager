using System.Net.WebSockets;
using System.Threading.Tasks;
using WebSocketManager;
using WebSocketManager.Common;
using StackExchange.Redis;
using System.Linq;
using WebSocketsSample.Data;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Internal;
using System.Reflection;
using System;
using System.Text;

namespace WebSocketsSample
{
    public class DataHandler : WebSocketHandler
    {
        private static ConnectionMultiplexer _redis;
        private ApplicationDbContext _dbContext;

        public DataHandler(WebSocketConnectionManager webSocketConnectionManager, ApplicationDbContext dbContext) : base(webSocketConnectionManager)
        {
            _dbContext = dbContext;
            Task.Run(async () => { await ConfigureMessaging(); });
        }


        public IQueryable GetData()
        {
            return _dbContext.Products;
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var serializedInvocationDescriptor = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var invocationDescriptor = JsonConvert.DeserializeObject<InvocationDescriptor>(serializedInvocationDescriptor);

            Type controller = Type.GetType(invocationDescriptor.Controller);
            ConstructorInfo controllerConstructor = controller.GetConstructor(Type.EmptyTypes);
            object controllerInstance = controllerConstructor.Invoke(new object[] { });


            // Make sure that only public methods can be invoked by clients.
            var method = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m =>
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
                //method.Invoke(this, invocationDescriptor.Arguments);
                await InvokeClientMethodAsync(SocketId, invocationDescriptor.OperationId, invocationDescriptor.Controller, invocationDescriptor.Action, new object[] { method.Invoke(controllerInstance, invocationDescriptor.Arguments) });

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


        private async Task ConfigureMessaging()
        {
            _redis = await ConnectionMultiplexer.ConnectAsync("localhost");
            _redis.PreserveAsyncOrder = false;

            ISubscriber sub = _redis.GetSubscriber();

            await sub.SubscribeAsync("messages", async (channel, msg) =>
            {
                await InvokeClientMethodToAllAsync(SocketId, "0", this.GetType().Name, "SendMessage", msg.ToString());
            });
        }

        public async Task SendMessage(string message)
        {
            var dataRequest = JsonConvert.DeserializeObject<DataOperationRequest>(message);
            //var invoker = new ControllerActionInvoker();

            Type magicType = Type.GetType(dataRequest.Controller);
            ConstructorInfo magicConstructor = magicType.GetConstructor(Type.EmptyTypes);
            object magicClassObject = magicConstructor.Invoke(new object[] { });

            // Get the ItsMagic method and invoke with a parameter value of 100

            MethodInfo magicMethod = magicType.GetMethod(dataRequest.Action);
            object magicValue = magicMethod.Invoke(magicClassObject, dataRequest.Params);

            await InvokeClientMethodAsync(SocketId, "0", this.GetType().Name, "SendMessage", new object[] { magicValue });
            //await _redis.GetSubscriber().PublishAsync("messages", socketId + "||" + message);// new string[] { socketId, message });
        }

        [ClientAccess(ClientAccessibilityOptions.DenyAll)]
        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);

            var socketId = WebSocketConnectionManager.GetId(socket);

            var message = new Message()
            {
                MessageType = MessageType.Text,
                Data = $"{socketId} is now connected"
            };

            await SendMessageToAllAsync(message);
        }

        [ClientAccess(ClientAccessibilityOptions.DenyAll)]
        public override async Task OnDisconnected(WebSocket socket)
        {
            var socketId = WebSocketConnectionManager.GetId(socket);
            
            await base.OnDisconnected(socket);

            var message = new Message()
            {
                MessageType = MessageType.Text,
                Data = $"{socketId} disconnected"
            };
            await SendMessageToAllAsync(message);
        }
    }
}
