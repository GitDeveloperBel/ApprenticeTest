﻿using Shared.DDD;

namespace Catering.Models.DL.Models;

public sealed record CustomerLocation : ValueObject
{
    private string _street;
    private string _city;

    public string Street { get => _street; private set => _street = value; }
    public string City { get => _city; private set => _city = value; }

    private CustomerLocation()
    {
        
    }

    internal CustomerLocation(string street, string city)
    {
        _street = street;
        _city = city;
    }
}
