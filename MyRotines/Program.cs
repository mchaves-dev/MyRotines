using Microsoft.Extensions.DependencyInjection;
using MyRotines.Application;
using MyRotines.Core.Domain.Events;

var services = new ServiceCollection();
services.AddScoped<IEventPublisher, EventPublisher>();

var provider = services.BuildServiceProvider();


