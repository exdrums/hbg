using API.Contacts.Application.Interfaces;
using API.Contacts.Application.Services;
using API.Contacts.Domain.Repositories;
using API.Contacts.Infrastructure.Repositories.InMemory;
using API.Contacts.Infrastructure.Services;
using API.Contacts.Infrastructure.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace API.Contacts.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Extension methods for IServiceCollection to register chat services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all chat-related services to the service collection
        /// </summary>
        public static IServiceCollection AddChatServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            services.AddSingleton<IConversationRepository, InMemoryConversationRepository>();
            services.AddSingleton<IMessageRepository, InMemoryMessageRepository>();
            services.AddSingleton<IAlertRepository, InMemoryAlertRepository>();

            // Register application services
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAlertService, AlertService>();
            services.AddScoped<IReadReceiptService, ReadReceiptService>();
            services.AddScoped<ITypingService, TypingService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IFileMessageHandler, FileMessageHandler>();
            services.AddScoped<IAiAssistantService, MockAiAssistantService>();
            services.AddScoped<IChatApplicationService, ChatApplicationService>();

            // Register SignalR services
            services.AddSingleton<IConnectionManager, InMemoryConnectionManager>();
            services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();

            // Register SignalR hub
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaximumReceiveMessageSize = 102400; // 100 KB
            });

            return services;
        }
    }
}
