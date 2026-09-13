using Microsoft.AspNetCore.SignalR;

namespace TaskAPI.Hubs
{
    // Bu sýnýf tarayýcýlar ile API arasýndaki canlý köprüdür
    public class NotificationHub : Hub
    {
        // Ýhtiyaç halinde buraya istemciden sunucuya mesaj gönderme metotlarý eklenebilir.
        // Biz þimdilik sadece sunucudan istemciye mesaj iteceðiz (Push).
    }
}