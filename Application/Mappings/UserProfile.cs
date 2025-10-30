using AutoMapper;
using BaseSourceImpl.Application.DTOs.User;
using BaseSourceImpl.Domains.Entities.RefreshToken;
using BaseSourceImpl.Domains.Entities.User;

namespace BaseSourceImpl.Application.Mappings
{
    /// <summary>
    /// UserProfile - AutoMapper profile cho User
    /// Map giữa Entity, DTO, và ViewModel
    /// </summary>
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Entity <-> DTO (Business Logic Layer)
            CreateMap<UserEntity, UserDto>()
               .ForMember(dest => dest.RoleIdList, opt => opt.MapFrom(src => src.RoleIdList ?? new List<int>()))
               .ReverseMap()
               .ForMember(dest => dest.RoleIdList, opt => opt.MapFrom(src => src.RoleIdList ?? new List<int>()));

            // Entity -> ViewModel (Client Response)
            CreateMap<UserEntity, UserViewModel>()
                .ForMember(dest => dest.RoleIdList, opt => opt.MapFrom(src => src.RoleIdList ?? new List<int>()));

            // DTO -> ViewModel (Alternative path)
            CreateMap<UserDto, UserViewModel>();
        }
    }
}
