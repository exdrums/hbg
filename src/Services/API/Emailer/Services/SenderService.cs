using MimeKit;
using MailKit.Net.Smtp;
using API.Emailer.Models;
using System.Collections.Concurrent;
using Org.BouncyCastle.Asn1.Esf;
using API.Emailer.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using API.Emailer.WebSocket;
using AutoMapper;

namespace Emailer.Services;

public class SenderService
{
    private readonly ConcurrentDictionary<long, Distribution> processingEmails = new ConcurrentDictionary<long, Distribution>();
    private IServiceProvider services { get; set; }
    private readonly IHubContext<EmailerHub, IEmailerHubClientActions> hub;
    private readonly IMapper mapper;

    public SenderService(IServiceProvider services, IHubContext<EmailerHub, IEmailerHubClientActions> hub, IMapper mapper)
    {
            this.mapper = mapper;
        this.hub = hub;
        this.services = services;
    }

    public async void ProcessDistribution(Distribution dist)
    {
        processingEmails.TryAdd(dist.DistributionID, dist);

        using (var client = new SmtpClient())
        {
            client.Connect(dist.Sender.ServerAddress, 587, false);
            client.Authenticate(dist.Sender.Address, dist.Sender.Passcode);
            try 
            {
                await SendEmails(dist, client);

            }
            catch (Exception ex) 
            {
                foreach(var email in dist.Emails.Where(e => e.Status == EmailStatus.Pending)) 
                {
                    email.Status = EmailStatus.None;
                }
            }
            client.Disconnect(true);
        }
        
        await this.CommitChanges(dist);
    }

    /// <summary>
    /// Process sending all emails for selected distribution
    /// </summary>
    /// <param name="dist"></paramL>
    /// <param name="smtpClient"></param>
    /// <returns></returns>
    private async Task SendEmails(Distribution dist, SmtpClient smtpClient)
    {
        foreach(var email in dist.Emails) {
            email.Status = EmailStatus.Pending;
        }

        foreach (var email in dist.Emails)
        {
            var body = dist.Template.Content;
            try 
            {
                await SendEmail(email, smtpClient, body);
                NotifyClients(dist);
            }
            catch(Exception ex) 
            {
                email.Status = EmailStatus.Error;
                throw;
            }
            email.Status = EmailStatus.Sent;
        }
    }

    /// <summary>
    /// Process sending email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    private async Task SendEmail(Email email, SmtpClient smtpClient, string body) 
    {
        await Task.Delay(2000);
        email.Status = EmailStatus.Sent;
        return;
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(email.Distribution.SenderName, email.Distribution.Sender.Address));
        message.To.Add(new MailboxAddress(email.Receiver.Name, email.Receiver.Address));

        message.Subject = email.Distribution.Subject;

        message.Body = new TextPart("html")
        {
            Text = body,
        };

        await smtpClient.SendAsync(message);
    }

    /// <summary>
    /// Notify that Distribution was changed
    /// </summary>
    /// <param name="dist"></param>
    private async void NotifyClients(Distribution dist)
    {
        var dto = mapper.Map<DistributionUpdatedHubDto>(dist);
        await hub.Clients.Group(dist.DistributionID.ToString()).DistributionUpdated(dto);
    }

    private async Task CommitChanges(Distribution dist)
    {
        using (var scope = this.services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var existingDist = dbContext.Distributions.FirstOrDefault(x => x.DistributionID == dist.DistributionID);
            if (existingDist == null) return;

            foreach(var email in dist.Emails)
            {
                dbContext.Entry(email).State = EntityState.Modified;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
