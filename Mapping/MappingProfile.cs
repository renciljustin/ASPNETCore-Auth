using API.Data.Dtos;
using API.Data.Models;
using AutoMapper;

namespace API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDetailDto>();
            CreateMap<User, UserListDto>();

            CreateMap<UserRegisterDto, User>();
        }
    }
}