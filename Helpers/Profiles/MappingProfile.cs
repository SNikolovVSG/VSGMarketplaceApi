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
            CreateMap<ItemAddModelString, Item>();
            CreateMap<ItemAddModelString, ItemAddModel>();

            CreateMap<Item, ItemViewModel>();
            CreateMap<Item, InventoryItemViewModel>();
            CreateMap<Item, MarketplaceItemViewModel>();

            CreateMap<Order, MyOrdersViewModel>();
            CreateMap<Order, PendingOrderViewModel>();

            CreateMap<NewOrderAddModel, Order>();
        }

    }
}
