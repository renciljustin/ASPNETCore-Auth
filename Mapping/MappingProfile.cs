using AuthDemo.Data.Dtos;
using AuthDemo.Data.Models;
using AutoMapper;

namespace AuthDemo.Mapping
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