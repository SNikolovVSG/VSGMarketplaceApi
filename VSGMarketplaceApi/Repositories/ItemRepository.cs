using AutoMapper;
using Dapper;
using Microsoft.Data.SqlClient;
using VSGMarketplaceApi.DTOs;
using VSGMarketplaceApi.Models;
using VSGMarketplaceApi.Repositories.Interfaces;

namespace VSGMarketplaceApi.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;

        public ItemRepository(IConfiguration configuration, IMapper mapper)
        {
            this.configuration = configuration;
            this.mapper = mapper;
        }

        public async Task<int> AddAsync(ItemAddModel item)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            int changesByAddingItem = await connection.ExecuteAsync("insert into items (name, price, category, quantity, quantityForSale, description) values (@Name, @Price, @Category, @Quantity, @QuantityForSale, @Description)", item);
            return changesByAddingItem;
        }

        public async Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync()
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var items = await connection.QueryAsync<InventoryItemViewModel>("select * from Items");

            return items;
        }

        public async Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int code)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var item = await connection.QueryFirstAsync<MarketplaceByIdItemViewModel>("select * from Items where code = @Code", new { Code = code });
            if (item == null || item.QuantityForSale <= 0)
            {
                return null;
            }
            return item;
        }

        public async Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync()
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var items = await connection.QueryAsync<MarketplaceItemViewModel>("select * from Items where quantityForSale > 0");
            return items;
        }

        public async Task<int> UpdateAsync(ItemAddModel item, int code)
        {
            var editItem = mapper.Map<Item>(item);
            editItem.Code = code;

            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            int result = 0;
            try
            {
                result = await connection.ExecuteAsync
                    ("update items set " +
                    "name = @Name, " +
                    "price = @Price, " +
                    "category = @Category, " +
                    "quantity = @Quantity, " +
                    "quantityForSale = @QuantityForSale, " +
                    "description = @Description " +
                    "where code = @code", editItem);
            }
            catch (Exception)
            {
                return 0;
            }

            return result;
        }
    }
}

