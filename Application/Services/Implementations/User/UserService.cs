using AutoMapper;
using BaseNetCore.Core.src.Main.Common.Exceptions;
using BaseNetCore.Core.src.Main.Common.Models;
using BaseNetCore.Core.src.Main.DAL.Repository;
using BaseSourceImpl.Application.DTOs.User;
using BaseSourceImpl.Application.Services.Interfaces;
using BaseSourceImpl.Common.ErrorCodes.User;
using BaseSourceImpl.Domains.Entities.User;
using BaseSourceImpl.Presentation.Models.Requests;

namespace BaseSourceImpl.Application.Services.Implementations.User
{
    /// <summary>
    /// UserService Implementation
    /// Business Logic Layer - S? d?ng AutoMapper
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IMapper mapper,IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ValueResponse<UserViewModel>> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.Repository<UserEntity>().GetByIdAsync(id);
            if (entity == null)
                throw new BaseApplicationException(UserErrorCodes.USER_NOT_FOUND,$"User with ID {id} not found");

            return new ValueResponse<UserViewModel>(_mapper.Map<UserViewModel>(entity));
        }


        public async Task<PageResponse<UserViewModel>> GetPageAsync(UserSearchModel searchModel)
        {
            var spec = UserSpecifications.PagingSpecification(searchModel.SearchText, searchModel.CurrentPage, searchModel.Size);
            var pageResponse = await _unitOfWork.Repository<UserEntity>().GetWithPagingAsync(spec);
            return new PageResponse<UserViewModel>(_mapper.Map<List<UserViewModel>>(pageResponse.Data), pageResponse.Success, pageResponse.Total, pageResponse.CurrentPage, pageResponse.PageSize);
        }

        public async Task<UserViewModel> CreateAsync(UserDto dto)
        {
            try
            {
                // Validate
                var userRepository = _unitOfWork.Repository<UserEntity>();
                if (await userRepository.AnyAsync(i => i.UserName == dto.UserName))
                    throw new UserDuplicateException($"Username '{dto.UserName}' already exists");

                // Hash password
                dto.Password = HashPassword(dto.Password);
                dto.CreatedDate = DateTime.UtcNow;
                dto.CreatedBy = 1; // TODO: Get current user

                // Map DTO -> Entity
                var entity = _mapper.Map<UserEntity>(dto);

                // Save
                userRepository.Add(entity);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<UserViewModel>(entity);
            }
            catch (Exception ex)
            {
                throw new SystemErrorException(ex.Message);
            }
        }

        public async Task<UserViewModel> UpdateAsync(UserDto dto)
        {
            var entity = await _unitOfWork.Repository<UserEntity>().GetByIdAsync(dto.Id);
            if (entity == null)
                throw new BaseApplicationException(UserErrorCodes.USER_NOT_FOUND,$"User with ID {dto.Id} not found", System.Net.HttpStatusCode.NotFound);

            // Map DTO -> Entity (ch? update các fields ???c phép)
            _mapper.Map(dto, entity);

            _unitOfWork.Repository<UserEntity>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserViewModel>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<UserEntity>().GetByIdAsync(id);
            if (entity == null)
                throw new BaseApplicationException(UserErrorCodes.USER_NOT_FOUND,$"User with ID {id} not found");

            await _unitOfWork.Repository<UserEntity>().DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        #region Helper Methods

        private string HashPassword(string password)
        {
            // TODO: Implement BCrypt or Argon2
            // Install: dotnet add package BCrypt.Net-Next
            // return BCrypt.Net.BCrypt.HashPassword(password);
            return password; // TEMPORARY - Replace in production
        }

        #endregion
    }
}
