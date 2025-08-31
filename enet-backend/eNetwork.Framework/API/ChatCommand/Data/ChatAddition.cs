namespace eNetwork
{
    public class ChatAddition
    {
        public int ChatType { get; set; }
        public string ComponentText { get; set; }
        public ChatAddition(ChatType chatType, string component = "eNetwork Globals")
        {
            ChatType = (int)chatType;
            ComponentText = component;
        }
    }
}
