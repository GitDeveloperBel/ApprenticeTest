﻿using Shared.Patterns.ResultPattern;
using System.Text.RegularExpressions;
using UserPlatform.Shared.Communication.Models;
using UserPlatform.Shared.DL.Models;
using UserPlatform.Shared.Helpers;

namespace UserPlatform.Shared.DL.Factories.UserFactory;

public class UserFactory : IUserFactory
{
    private Regex _passwordSmallLetter;
    private Regex _passwordCapitalLetter;
    private Regex _passwordDigit;
    private Regex _passwordSpecial;
    private byte _passwordMinLength;
    private byte _passwordMaxLength;
    private IPasswordHasher _passwordHasher;

    public UserFactory(IPasswordHasher passwordHasher)
    {
        _passwordSmallLetter = new("[a-z]+");
        _passwordCapitalLetter = new("[A-Z]+");
        _passwordDigit = new("[0-9]+");
        _passwordSpecial = new("[!|\\+|\\-|#|\\.|,|\\^|*]+");
        _passwordMinLength = 8;
        _passwordMaxLength = 128;
        _passwordHasher = passwordHasher;
    }

    public Result<User> Build(UserCreationRequest request, UserValidationData validationData) // TODO: unit test
    {
        BinaryFlag flag = new();
        if (request is null)
        {
            flag += UserFactoryErrors.RequestIsNull;
            return new BadRequestResult<User>(flag);
        }
        if (string.IsNullOrWhiteSpace(request.CompanyName)) // could benefit from specification pattern, perhaps
            flag += UserFactoryErrors.CompanyNameNotSat;
        if (request.CompanyName is not null && validationData.Users.Any(x => string.Equals(x.CompanyName.ToLower(), request.CompanyName.ToLower())))
            flag += UserFactoryErrors.CompanyNameInUse;
        if (string.IsNullOrWhiteSpace(request.City) || string.IsNullOrWhiteSpace(request.Street))
            flag += UserFactoryErrors.LocationInvalid;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (!_passwordCapitalLetter.IsMatch(request.Password))
                flag += UserFactoryErrors.PasswordMissingCapital;
            if (!_passwordSmallLetter.IsMatch(request.Password))
                flag += UserFactoryErrors.PasswordMissingSmall;
            if (!_passwordDigit.IsMatch(request.Password))
                flag += UserFactoryErrors.PasswordMissingDigit;
            if (!_passwordSpecial.IsMatch(request.Password))
                flag += UserFactoryErrors.PasswordMissingSpecial;
            if (request.Password.Length < _passwordMinLength)
                flag += UserFactoryErrors.PasswordToShort;
            if (request.Password.Length > _passwordMaxLength)
                flag += UserFactoryErrors.PasswordToLong;
        }
        else
            flag += UserFactoryErrors.MissingPassword;
        if (!string.Equals(request.Password, request.PasswordReentered))
            flag += UserFactoryErrors.NotSamePassword;
        if (string.IsNullOrWhiteSpace(request.Phone) && string.IsNullOrWhiteSpace(request.Email))
            flag += UserFactoryErrors.NoContactInformationSat;
        if (!flag)
            return new BadRequestResult<User>(flag);
        UserLocation location = new(request.City!, request.Street!);
        UserContact contact = new(request.Email, request.Phone);
        User user = new(request.CompanyName!, contact, location);
        var hashedPassword = _passwordHasher.Hash(user, request.Password!);
        user.SetPassword(hashedPassword);
        return new SuccessResult<User>(user);
    }




}
