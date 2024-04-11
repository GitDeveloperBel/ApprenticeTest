﻿using CateringDataProcessingPlatform.DL.Models;
using Shared.Communication.Models;
using Shared.Patterns.ResultPattern;

namespace Catering.Shared.DL.Factories.OrderFactory;

public interface IOrderFactory
{

    public Result<Order> Build(OrderPlaceCommand request, OrderValidationData validationData);
}