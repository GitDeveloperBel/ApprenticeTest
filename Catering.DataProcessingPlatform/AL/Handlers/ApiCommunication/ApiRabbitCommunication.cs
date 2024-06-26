﻿using Catering.Shared.DL.Factories;
using Catering.DataProcessingPlatform.AL.Handlers.ApiCommunication.DataProcessing;
using Catering.DataProcessingPlatform.Extensions;
using Catering.DataProcessingPlatform.IPL;
using Catering.DataProcessingPlatform.IPL.Appsettings;
using Catering.DataProcessingPlatform.IPL.Appsettings.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using Shared.Communication;
using Shared.Communication.Models;
using Shared.Communication.Models.Dish;
using Shared.Communication.Models.Menu;
using Shared.Communication.Models.Order;
using Shared.Helpers.Time;
using Shared.Patterns.ResultPattern;
using Shared.Service;

namespace Catering.DataProcessingPlatform.AL.Handlers.ApiCommunication;

internal sealed class ApiRabbitCommunication : BaseService, IDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly ApiRabbitDataProcessing _processing;
    private IConnection _connection;
    private IModel _channel;

    public ApiRabbitCommunication(IConfigurationManager configurationManager, IContextFactory contextFactory, IFactoryCollection factoryCollection, RabbitData rabbitData, ITime time, ILogger logger) : base(logger)
    {
        _connectionFactory = new ConnectionFactory { HostName = rabbitData.Url, Port = rabbitData.Port };
        _processing = new(configurationManager, contextFactory, factoryCollection, time, logger);
        _channel = null!;
        _connection = null!;
    }

    public void Initialise()
    {
        _connection = _connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.BasicQos(0, 1, false);

        DeclareQueueWithConsumer(CommunicationQueueNames.DISH_CREATION, ReceivedForDishCreationRPC);
        DeclareQueueWithConsumer(CommunicationQueueNames.MENU_CREATION, ReceivedForMenuCreationRPC);
        DeclareQueueWithConsumer(CommunicationQueueNames.ORDER_QUERY_FUTURE, ReceivedForOrderFutureRPC);
        DeclareQueueWithConsumer(CommunicationQueueNames.DISH_QUERY, ReceivedForDishesRPC);
        DeclareQueueWithConsumer(CommunicationQueueNames.MENU_QUERY_EMPLOYEE, ReceivedForMenuesRPC);
        DeclareQueueWithConsumer(CommunicationQueueNames.ORDER_STATUS, ReceivedForOrderStatusRPC);
        DeclareQueueWithConsumer(CommunicationQueueNames.ORDER_QUERY_OVERVIEW, ReceivedForOrderOverviewRPC);

        _logger.Information("{Identifier}: Initialised", _identifier);
    }

    private void DeclareQueueWithConsumer(string name, EventHandler<BasicDeliverEventArgs> handler)
    {
        _channel.QueueDeclare(queue: name, durable: true, exclusive: false, autoDelete: false);
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += handler;
        _channel.BasicConsume(queue: name, autoAck: false, consumer: consumer);
    }

    private void ReceivedForMenuCreationRPC(object? sender, BasicDeliverEventArgs e)
    {
        var message = e.ToMessage();
        try
        {
            var requestProps = e.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = requestProps.CorrelationId;

            var command = message.ToCommand<MenuCreationCommand>();

            if(command is null)
            {
                _logger.Error("{Identifier}: {Message} could not be parsed to {MenuCreationCommandType}", _identifier, message, typeof(MenuCreationCommand));
                _channel.BasicAck(e.DeliveryTag, false);
                return;
            }
            var result = _processing.Process(command);
            var body = result.ToBody();
            SendReply(requestProps, replyProps, body);
            //_channel.BasicPublish(exchange: string.Empty, routingKey: requestProps.ReplyTo, basicProperties: replyProps, body: body);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "{Identifier}: Error processing {Message}", _identifier, message);
        }
        _channel.BasicAck(e.DeliveryTag, false);
    }

    private void ReceivedForDishCreationRPC(object? sender, BasicDeliverEventArgs e)
    {
        var message = e.ToMessage();
        try
        {
            var requestProps = e.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = requestProps.CorrelationId;

            var command = message.ToCommand<DishCreationCommand>();

            if (command is null)
            {
                _logger.Error("{Identifier}: {Message} could not be parsed to {DishCreationCommandType}", _identifier, message, typeof(DishCreationCommand));
                _channel.BasicAck(e.DeliveryTag, false);
                return;
            }
            var result = _processing.Process(command);
            var body = result.ToBody();
            SendReply(requestProps, replyProps, body);
            //_channel.BasicPublish(exchange: string.Empty, routingKey: requestProps.ReplyTo, basicProperties: replyProps, body: body);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "{Identifier}: Error processing {Message}", _identifier, message);
        }
        _channel.BasicAck(e.DeliveryTag, false);
    }

    private void ReceivedForOrderFutureRPC(object? sender, BasicDeliverEventArgs e)
    {
        var message = e.ToMessage();
        try
        {
            var requestProps = e.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = requestProps.CorrelationId;

            var command = message.ToCommand<GetFutureOrdersCommand>();

            if (command is null)
            {
                _logger.Error("{Identifier}: {Message} could not be parsed to {OrderFutureCommandType}", _identifier, message, typeof(GetFutureOrdersCommand));
                _channel.BasicAck(e.DeliveryTag, false);
                return;
            }
            var result = _processing.Process(command);
            var body = result.ToBody();
            SendReply(requestProps, replyProps, body);
            //_channel.BasicPublish(exchange: string.Empty, routingKey: requestProps.ReplyTo, basicProperties: replyProps, body: body);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "{Identifier}: Error processing {Message}", _identifier, message);
        }
        _channel.BasicAck(e.DeliveryTag, false);
    }

    private void ReceivedForMenuesRPC(object? sender, BasicDeliverEventArgs e)
    {
        var message = e.ToMessage();
        try
        {
            var requestProps = e.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = requestProps.CorrelationId;

            var command = message.ToCommand<MenuListCommand>();

            if (command is null)
            {
                _logger.Error("{Identifier}: {Message} could not be parsed to {MenuListCommandType}", _identifier, message, typeof(MenuListCommand));
                _channel.BasicAck(e.DeliveryTag, false);
                return;
            }
            var result = _processing.Process(command);
            var body = result.ToBody();
            SendReply(requestProps, replyProps, body);
            //_channel.BasicPublish(exchange: string.Empty, routingKey: requestProps.ReplyTo, basicProperties: replyProps, body: body);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "{Identifier}: Error processing {Message}", _identifier, message);
        }
        _channel.BasicAck(e.DeliveryTag, false);
    }

    private void ReceivedForDishesRPC(object? sender, BasicDeliverEventArgs e)
    {
        var message = e.ToMessage();
        try
        {
            var requestProps = e.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = requestProps.CorrelationId;

            var command = message.ToCommand<DishListCommand>();

            if (command is null)
            {
                _logger.Error("{Identifier}: {Message} could not be parsed to {DishListCommandType}", _identifier, message, typeof(DishListCommand));
                _channel.BasicAck(e.DeliveryTag, false);
                return;
            }
            var result = _processing.Process(command);
            var body = result.ToBody();
            SendReply(requestProps, replyProps, body);
            //_channel.BasicPublish(exchange: string.Empty, routingKey: requestProps.ReplyTo, basicProperties: replyProps, body: body);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "{Identifier}: Error processing {Message}", _identifier, message);
        }
        _channel.BasicAck(e.DeliveryTag, false);
    }

    private void ReceivedForOrderStatusRPC(object? sender, BasicDeliverEventArgs e)
    {
        var message = e.ToMessage();
        try
        {
            var requestProps = e.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = requestProps.CorrelationId;

            var command = message.ToCommand<SetOrderStatusCommand>();

            if(command is null)
            {
                _logger.Error("{Identifier}: {Message} could not be parsed to {SetOrderStatusCommandType}", _identifier, message, typeof(SetOrderStatusCommand));
                SendReplyProcessingFailed(requestProps, replyProps);
                _channel.BasicAck(e.DeliveryTag, false);
                return;
            }
            var result = _processing.Process(command);
            var body = result.ToBody();
            SendReply(requestProps, replyProps, body);
            //_channel.BasicPublish(exchange: string.Empty, routingKey: requestProps.ReplyTo, basicProperties: replyProps, body: body);

        }
        catch (Exception ex)
        {
            _logger.Error(ex, "{Identifier}: Error processing {Message}", _identifier, message);
        }
        _channel.BasicAck(e.DeliveryTag, false);
    }

    private void ReceivedForOrderOverviewRPC(object? sender, BasicDeliverEventArgs e)
    {
        var message = e.ToMessage();
        try
        {
            var requestProps = e.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = requestProps.CorrelationId;

            var command = message.ToCommand<GetOrderOverviewQueryCommand>();

            if(command is null)
            {
                _logger.Error("{Identifier}: {Message} could not be pared to {GetOrderOverviewQueryCommandType}", _identifier, message, typeof(GetOrderOverviewQueryCommand));
                SendReplyProcessingFailed(requestProps, replyProps);
                _channel.BasicAck(e.DeliveryTag, false);
                return;
            }
            var result = _processing.Process(command);
            var body = result.ToBody();
            SendReply(requestProps, replyProps, body);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "{Identifier}: Error processing {Message}", _identifier, message);
        }
        _channel.BasicAck(e.DeliveryTag, false);
    }

    private void SendReply(IBasicProperties requestProps, IBasicProperties replyProps, ReadOnlyMemory<byte> body)
    {
        _channel.BasicPublish(exchange: string.Empty, routingKey: requestProps.ReplyTo, basicProperties: replyProps, body: body);
    }

    private void SendReplyProcessingFailed(IBasicProperties requestProps, IBasicProperties replyProps)
    {
        var result = new UnhandledResult(new(1));
        var body = result.ToBody();
        SendReply(requestProps, replyProps, body);
    }

    public void Dispose()
    {
        _connection.Close();
    }
}
