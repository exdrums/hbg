using API.Emailer.Models;

namespace API.Emailer.Database;

public static class Seeding
{
    /// <summary>
    /// It will not work because of UserID, it can be created only in runtime
    /// </summary>
    /// <param name="context"></param>
    /// <param name="settings"></param>
    public static void SeedEmailerDb(this AppDbContext context, EmailerAppSettings settings)
    {
        if (settings.EnableSeeding != true) return;

        if(!context.Senders.Any())
        {
            var defaultSender = new Sender()
            {
                UserID = "*", 
                Name = "Default sender",
                Address = settings.DEFAULT_SENDER_ADDRESS,
                ServerAddress = settings.DEFAULT_SENDER_SERVER,
                Login = settings.DEFAULT_SENDER_ADDRESS,
                Passcode = settings.DEFAULT_SENDER_PASSCODE
            };
            context.Senders.Add(defaultSender);
            context.SaveChanges();
        }
    }
}
