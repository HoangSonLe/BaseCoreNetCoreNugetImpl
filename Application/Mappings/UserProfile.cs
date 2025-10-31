using AutoMapper;
using BaseSourceImpl.Application.DTOs.User;
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
            CreateMap<UserEntity, UserDto>().ReverseMap();

            // Entity -> ViewModel (Client Response)
            CreateMap<UserEntity, UserViewModel>();

            // DTO -> ViewModel (Alternative path)
            CreateMap<UserDto, UserViewModel>();
        }
    }
}
