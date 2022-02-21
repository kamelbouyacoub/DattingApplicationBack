namespace DattingApplication.Entities
{
    public class Connection
    {
        public Connection()
        {

        }
        public Connection(string connectionId, string username)
        {
            this.ConnectionId = connectionId;
            this.UserName = username;
        }
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
    }
}