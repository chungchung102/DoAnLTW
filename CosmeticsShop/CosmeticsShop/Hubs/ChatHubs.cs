using Microsoft.AspNet.SignalR;
using CosmeticsShop.Models;
using System;

namespace CosmeticsShop.Hubs
{
    public class ChatHubs : Hub
    {
        private ShoppingEntities db = new ShoppingEntities();

        public void SendMessage(int fromUserId, int toUserId, string content)
        {
            var message = new Message
            {
                FromUserID = fromUserId,
                ToUserID = toUserId,
                Content = content,
                CreatedDate = DateTime.Now
            };

            db.Messages.Add(message);
            db.SaveChanges();

            var msgData = new
            {
                ID = message.ID,
                FromUserID = message.FromUserID,
                ToUserID = message.ToUserID,
                Content = message.Content,
                CreatedDate = message.CreatedDate.Value.ToString("HH:mm:ss dd/MM/yyyy"),
                FromUserName = message.User?.Name ?? "",
                FromUserAvatar = message.User?.Avatar ?? ""
            };

            // Gửi cho tất cả client
            Clients.All.receiveMessage(msgData);
        }
    }
}
