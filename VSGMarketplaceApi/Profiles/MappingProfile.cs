using AutoMapper;
using VSGMarketplaceApi.DTOs;
using VSGMarketplaceApi.Models;

namespace VSGMarketplaceApi.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ItemAddDTO, Item>();

            CreateMap<Item, ItemViewModel>();
            CreateMap<Item, InventoryItemViewModel>();
            CreateMap<Item, MarketplaceItemViewModel>();

            CreateMap<Order, MyOrdersViewModel>();
            CreateMap<Order, PendingOrderViewModel>();

            CreateMap<NewOrderInputModel, Order>();
        }

    }
}
