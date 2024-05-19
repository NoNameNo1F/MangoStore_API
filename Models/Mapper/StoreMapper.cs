using AutoMapper;
using MangoStore_API.Models.Dtos.MenuItemDto;

namespace MangoStore_API.Models.Mapper
{
    public class StoreMapper : Profile
    {
        public StoreMapper()
        {
            CreateMap<MenuItem, MenuItemDto>().ReverseMap();
        }
    }
}
