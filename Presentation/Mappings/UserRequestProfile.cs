using AutoMapper;
using BaseSourceImpl.Application.DTOs.User;
using BaseSourceImpl.Presentation.Models.Requests;

namespace BaseSourceImpl.Presentation.Mappings
{
    /// <summary>
    /// UserRequestProfile - AutoMapper profile cho Request Models
    /// Map gi?a Request Models (Presentation) và DTOs (Application)
    /// </summary>
    public class UserRequestProfile : Profile
    {
        public UserRequestProfile()
        {
            // CreateUserRequest -> UserDto
            CreateMap<CreateUserRequest, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.RoleIdList, opt => opt.MapFrom(src => src.RoleIdList ?? new List<int>()));

            // UpdateUserRequest -> UserDto
            CreateMap<UpdateUserRequest, UserDto>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.RoleIdList, opt => opt.MapFrom(src => src.RoleIdList ?? new List<int>()));
        }
    }
}
