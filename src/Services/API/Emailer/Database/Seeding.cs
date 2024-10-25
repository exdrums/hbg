using API.Emailer.Models;

namespace API.Emailer.Database;

public static class Seeding
{
    /// <summary>
    /// It will not work because of UserID, it can be created only in runtime
    /// </summary>
    /// <param name="context"></param>
    /// <param name="settings"></param>
    public static void SeedEmailerDb(this AppDbContext context, EmailerAppSettings settings, string userId)
    {
        if (settings.EnableSeeding != true) return;

        if(!context.Senders.Any())
        {
            var defaultSender = new Sender()
            {
                UserID = userId, 
                Name = "Default testing sender",
                Address = settings.DEFAULT_SENDER_ADDRESS,
                ServerAddress = settings.DEFAULT_SENDER_SERVER,
                Login = settings.DEFAULT_SENDER_ADDRESS,
                Passcode = settings.DEFAULT_SENDER_PASSCODE
            };
            context.Senders.Add(defaultSender);

            var rec1 = new Receiver() { UserID = userId, Name = "Receiver 1", Address = "exdrums@gmail.com" };
            var rec2 = new Receiver() { UserID = userId, Name = "Receiver 2", Address = "exdrums@gmail.com" };
            var rec3 = new Receiver() { UserID = userId, Name = "Receiver 3", Address = "exdrums@gmail.com" };
            var rec4 = new Receiver() { UserID = userId, Name = "Receiver 4", Address = "exdrums@gmail.com" };
            var rec5 = new Receiver() { UserID = userId, Name = "Receiver 5", Address = "exdrums@gmail.com" };

            context.Add(rec1);
            context.Add(rec2);
            context.Add(rec3);
            context.Add(rec4);
            context.Add(rec5);

            var defaultTemplate = new Template(){
                UserID = userId,
                Name = "Default testing template",
                Content = "<h1>TESTINGGG</h1>",
                Distributions = new List<Distribution>() {
                    new Distribution() 
                    {
                        Name = "Default testing distribution",
                        Subject = "Testing",
                        Sender = defaultSender,
                        Emails = new List<Email>() 
                        {
                            new Email() { Status = EmailStatus.None, Receiver = new Receiver() { UserID = userId, Name = "Receiver 6", Address = "exdrums@gmail.com" } },
                            new Email() { Status = EmailStatus.None, Receiver = new Receiver() { UserID = userId, Name = "Receiver 7", Address = "exdrums@gmail.com" } },
                            new Email() { Status = EmailStatus.None, Receiver = new Receiver() { UserID = userId, Name = "Receiver 8", Address = "exdrums@gmail.com" } },
                            new Email() { Status = EmailStatus.None, Receiver = new Receiver() { UserID = userId, Name = "Receiver 9", Address = "exdrums@gmail.com" } },
                            new Email() { Status = EmailStatus.None, Receiver = new Receiver() { UserID = userId, Name = "Receiver 10", Address = "exdrums@gmail.com" } }
                        }
                    }
                }

            };

            context.Add(defaultTemplate);

            context.SaveChanges();
        }
    }
}
