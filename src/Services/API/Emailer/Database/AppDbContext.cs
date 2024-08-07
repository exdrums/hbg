using API.Emailer.Models;
using Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace API.Emailer.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Template> Templates { get; set; }
    public DbSet<Distribution> Distributions { get; set; }
    public DbSet<Sender> Senders { get; set; }
    public DbSet<Receiver> Receivers { get; set; }
    public DbSet<Email> Emails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use it ONLY to create Migrations
        // optionsBuilder.UseNpgsql("Server=localhost;port=3306; Database=hbgprojectsdb; Uid=hbg-dbuser; Pwd=hbg-password-database");

    }

    public async Task<List<Template>> GetTemplateList(string userId) =>
        await Templates
            .Where(t => t.UserID == userId)
            .ToListAsync();


    public async Task<Template> GetTemplate(string userId, long templateId) =>
        await Templates
            .FirstOrDefaultAsync(t => t.UserID == userId && t.TemplateID == templateId)
            ?? throw new NotFoundException("Template not found.");

    public async Task<List<Distribution>> GetDistribiutionList(string userId) =>
        await Distributions
            .Where(d => d.Template.UserID == userId)
            .Include(d => d.Sender)
            .Include(d => d.Template)
            .Include(d => d.Emails)
            .ToListAsync();

    public async Task<Distribution> GetDistribution(string userId, long distributionId) =>
        await Distributions
            .FirstOrDefaultAsync(d => d.Template.UserID == userId && d.DistributionID == distributionId)
            ?? throw new NotFoundException("Distribution not found");

    public async Task<List<Sender>> GetSendersList(string userId) =>
        await Senders
            .Where(t => t.UserID == userId)
            .ToListAsync();

    public async Task<Sender> GetSender(string userId, long senderId) =>
        await Senders
            .FirstOrDefaultAsync(t => t.UserID == userId && t.SenderID == senderId)
            ?? throw new NotFoundException("Sender not found.");

    public async Task<List<Receiver>> GetReceiversList(string userId) =>
        await Receivers
            .Where(t => t.UserID == userId)
            .ToListAsync();

    public async Task<Receiver> GetReceiver(string userId, long receiverId) =>
        await Receivers
            .FirstOrDefaultAsync(t => t.UserID == userId && t.ReceiverID == receiverId)
            ?? throw new NotFoundException("Receiver not found.");
}
