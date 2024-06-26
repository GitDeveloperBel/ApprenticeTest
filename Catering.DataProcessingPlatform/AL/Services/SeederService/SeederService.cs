﻿using Catering.Shared.DL.Communication.Models;
using Catering.Shared.DL.Factories.DishFactory;
using Catering.Shared.DL.Factories.MenuFactory;
using Catering.Shared.IPL.UnitOfWork;
using Catering.DataProcessingPlatform.AL.Services.Contracts;
using Serilog;
using Shared.Patterns.CQRS.Queries;
using Shared.Service;
using System.Linq.Expressions;
using Catering.Shared.DL.Models;

namespace Catering.DataProcessingPlatform.AL.Services.SeederService;

internal sealed partial class SeederService : BaseService, ISeederService
{
    private readonly IUnitOfWork _unitOfWork;
    public SeederService(IUnitOfWork unitOfWork, ILogger logger) : base(logger)
    {
        _unitOfWork = unitOfWork;       
    }

    public void Seed()
    {
        var dishes = _unitOfWork.DishRepository.AllAsync(new DishCheckQuery()).Result.ToArray();
        if (!dishes.Any())
        {
            _logger.Warning("{Identifier}: Found no dishes. Seeding", _identifier);
            DishFactory dishFactory = new();
            DishValidationData dvd = new([]);
            var dish1 = dishFactory.Build(new DishCreationRequest { Name = "Æg" }, dvd).Data;
            var dish2 = dishFactory.Build(new DishCreationRequest { Name = "Kød" }, dvd).Data;
            var dish3 = dishFactory.Build(new DishCreationRequest { Name = "Rygbrød" }, dvd).Data;
            var dish4 = dishFactory.Build(new DishCreationRequest { Name = "Argurk" }, dvd).Data;
            _unitOfWork.DishRepository.Create(dish1);
            _unitOfWork.DishRepository.Create(dish2);
            _unitOfWork.DishRepository.Create(dish3);
            _unitOfWork.DishRepository.Create(dish4);
            _unitOfWork.Commit();
            dishes = [new(dish1.Id, dish1.Name), new(dish2.Id, dish2.Name), new(dish3.Id, dish3.Name), new(dish4.Id, dish4.Name)];
        }
        _logger.Debug("{Identifier}: The following diches are present in the context: {Dishes}", _identifier, dishes.Select(x => x.Id).ToArray());
        var menues = _unitOfWork.MenuRepository.AllAsync(new MenuCheckQuery()).Result;
        if (!menues.Any())
        {
            _logger.Warning("{Identifier}: Found no menues. Seeding", _identifier);
            MenuFactory menuFactory = new();
            MenuValidationData mvd = new([], dishes.Select(x => new MenuDishData(x.Id, x.Name)));
            var menu1 = menuFactory.Build(new MenuCreationRequest { Name = "En del æg", Description = "Hvis man er til æg", Parts =
                [
                new MenuPartCreationRequest { Id = dishes[0].Id, Amount =  50, Price = 10} 
                ] 
            }, mvd).Data;
            var menu2 = menuFactory.Build(new MenuCreationRequest { Name = "Frokost", Description = "Frokost menu", Parts = 
                [
                new MenuPartCreationRequest { Id = dishes[0].Id, Amount = 40, Price = 12},
                new MenuPartCreationRequest { Id = dishes[1].Id, Amount = 1, Price = 100},
                new MenuPartCreationRequest { Id = dishes[2].Id, Amount = 27, Price = 20},
                new MenuPartCreationRequest { Id = dishes[3].Id, Amount = 3, Price = 0.5f},
                ],
            }, mvd).Data;
            _unitOfWork.MenuRepository.Create(menu1);
            _unitOfWork.MenuRepository.Create(menu2);
            _unitOfWork.Commit();
            menues = [new(menu1.Id), new(menu2.Id)];

            var dish1 = _unitOfWork.DishRepository.GetSingleAsync(x => x.Id == dishes[0].Id).Result;
            var dish2 = _unitOfWork.DishRepository.GetSingleAsync(x => x.Id == dishes[1].Id).Result;
            var dish3 = _unitOfWork.DishRepository.GetSingleAsync(x => x.Id == dishes[2].Id).Result;
            var dish4 = _unitOfWork.DishRepository.GetSingleAsync(x => x.Id == dishes[3].Id).Result;
            dish1.AddMenu(menu1.Id);
            dish1.AddMenu(menu2.Id);
            dish2.AddMenu(menu2.Id);
            dish3.AddMenu(menu2.Id);
            dish4.AddMenu(menu2.Id);
            _unitOfWork.DishRepository.Update(dish1);
            _unitOfWork.DishRepository.Update(dish2);
            _unitOfWork.DishRepository.Update(dish3);
            _unitOfWork.DishRepository.Update(dish4);
            _unitOfWork.Commit();
        } 
        _logger.Debug("{Identifier}: The following menues are present in the context: {Menues}", _identifier, menues.Select(x => x.Id));
    }




    private class DishCheck : BaseReadModel
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public DishCheck(Guid id, string name)
        {
            Id = id;            
            Name = name;
        }
    }

    private class MenuCheck : BaseReadModel
    {
        public Guid Id { get; private set; }

        public MenuCheck(Guid id)
        {
            Id = id;            
        }
    }

    private class DishCheckQuery : BaseQuery<Dish, DishCheck>
    {
        public override Expression<Func<Dish, DishCheck>> Map()
        {
            return e => new(e.Id, e.Name);
        }
    }

    private class MenuCheckQuery : BaseQuery<Menu, MenuCheck>
    {
        public override Expression<Func<Menu, MenuCheck>> Map()
        {
            return e => new(e.Id);
        }
    }
}

