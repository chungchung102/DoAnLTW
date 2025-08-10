using CosmeticsShop.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Configuration;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.EventArgs;

namespace CosmeticsShop.Hubs
{
    public class ChatHubs : Hub
    {
        // Static để khởi tạo 1 lần duy nhất
        private static SqlTableDependency<Message> tableDependency;

        static ChatHubs()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ShoppingConnectionString"].ConnectionString;

                tableDependency = new SqlTableDependency<Message>(
                    connectionString,
                    tableName: "Message",
                    schemaName: "dbo",
                    executeUserPermissionCheck: false,
                    includeOldValues: true
                );

                tableDependency.OnChanged += TableDependency_Changed;
                tableDependency.OnError += TableDependency_OnError;
                tableDependency.Start();
            }
            catch (Exception ex)
            {
                // Ghi log hoặc thông báo lỗi rõ ràng
                Console.WriteLine("Lỗi khởi tạo TableDependency: " + ex.Message);
            }
        }

        private static void TableDependency_Changed(object sender, RecordChangedEventArgs<Message> e)
        {
            Show();
            ShowMessage();
        }

        private static void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            // Log chi tiết lỗi ra console hoặc file
            Console.WriteLine("Lỗi từ SqlTableDependency: " + e.Error.Message);
        }

        public static void Show()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ChatHubs>();
            context.Clients.All.displayMessage();
        }

        public static void ShowMessage()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ChatHubs>();
            context.Clients.All.displayMessageChating();
        }
    }
}
