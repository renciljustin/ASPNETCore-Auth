using API.Persistence.Dtos;
using API.Core.Models;
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