using AutoMapper;
using Data.Models;
using Data.ViewModels;

namespace Helpers.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ItemAddModel, Item>();
            CreateMap<ItemAddModelWithFormFile, Item>();
            CreateMap<ItemAddModelWithFormFile, ItemAddModel>();

            CreateMap<Item, InventoryItemViewModel>();
            CreateMap<Item, MarketplaceItemViewModel>();
            CreateMap<Item, MarketplaceByIdItemViewModel>();

            CreateMap<Order, MyOrdersViewModel>();
            CreateMap<Order, PendingOrderViewModel>();

            CreateMap<NewOrderAddModel, Order>();
        }

    }
}
