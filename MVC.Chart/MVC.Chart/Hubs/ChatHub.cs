using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;

namespace MVC.Chart.SignalR.Hubs
{
    public class ChatHub : Hub
    {

        //static ChatHub()
        //{
        //    Configuration.IConfigurationManager.DisconnectTimeout
        //}

        public void Hello()
        {
            Clients.All.hello();
        }

        static List<User> Users = new List<User>();

        public void Broadcast( string message )
        {
            var identity = Context.User.Identity;
            if (!identity.IsAuthenticated)
                return;
            Clients.All.addMessage(identity.Name, message);
        }

        public void Send( IEnumerable<string> users, string message)
        {
            var identity = Context.User.Identity;
            if (!identity.IsAuthenticated)
                return;
            var userList = users.ToList();
            if( userList.Any() )
                Clients.Clients(userList).addMessage(identity.Name, message);
            //Clients.All.addMessage(identity.Name, message);
        }

        // Подключение нового пользователя
        public void Connect()
        {
            var identity = Context.User.Identity;
            if (!identity.IsAuthenticated)
                return;

            var id = Context.ConnectionId;

            string userName = identity.Name;

            if (!Users.Any(x => x.ConnectionId == id))
            {
                var previousConnections = Users
                    .Where(u => u.Name.Equals(userName, StringComparison.InvariantCulture))
                    .Select(u => u.ConnectionId)
                    ;
                Users.Add(new User { ConnectionId = id, Name = userName });

                // Посылаем сообщение текущему пользователю
                Clients.Caller.onConnected(id, userName, Users);

                // Посылаем сообщение всем пользователям, кроме текущего
                Clients.AllExcept(id).onNewUserConnected(id, userName);
            }
        }

        // Отключение пользователя
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var context = Context;
            var id = Context.ConnectionId;
            return Disconnect(stopCalled, id);
        }

        private System.Threading.Tasks.Task Disconnect(bool stopCalled, string id)
        {
            var item = Users.FirstOrDefault(x => x.ConnectionId == id);
            if (item != null)
            {
                Users.Remove(item);
                Clients.All.onUserDisconnected(id, item.Name);
            }

            return base.OnDisconnected(stopCalled);
        }

        class User
        {
            public string ConnectionId;
            public string Name;
        }
    }
}