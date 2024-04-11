﻿using Catering.Models.DL.Models;
using CateringDataProcessingPlatform.DL.Models;
using Shared.Communication.Models;
using Shared.Patterns.ResultPattern;
using Shared.Patterns.ResultPattern.BadRequest;

namespace Catering.Shared.DL.Factories.CustomerFactory;

public sealed class CustomerFactory : ICustomerFactory
{
    public Result<Customer> Build(UserCreationCommand command, CustomerValidationData validationData)
    {
        BinaryFlag flag = new();
        if(command is null)
        {
            flag += CustomerFactoryErrors.RequestIsNull;
            return new BadRequestResult<Customer>(flag);
        }
        if (command.UserId == default)
            flag += CustomerFactoryErrors.CustomerIdInvalid;
        if (string.IsNullOrWhiteSpace(command.Street))
            flag += CustomerFactoryErrors.StreetIsInvalid;
        if (string.IsNullOrWhiteSpace(command.City))
            flag += CustomerFactoryErrors.CityIsInvalid;
        if (validationData.IdsInUse.Any(x => x.Id == command.UserId))
            flag += CustomerFactoryErrors.CustomerIdUse;
        if (!flag)
        {
            return new BadRequestResult<Customer>(flag);
        }
        CustomerLocation location = new(command.Street, command.City);
        Customer customer = new(command.UserId, location);
        return new SuccessResult<Customer>(customer);
    }
}