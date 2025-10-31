﻿using AutoMapper;
using BaseNetCore.Core.src.Main.BLL.Helpers;
using BaseNetCore.Core.src.Main.BLL.Services;
using BaseNetCore.Core.src.Main.Common.Exceptions;
using BaseNetCore.Core.src.Main.Common.Models;
using BaseNetCore.Core.src.Main.DAL.Repository;
using BaseNetCore.Core.src.Main.Security.Algorithm;
using BaseSourceImpl.Application.DTOs.User;
using BaseSourceImpl.Common.ErrorCodes;
using BaseSourceImpl.Domains.Entities.User;
using BaseSourceImpl.Presentation.Controllers.User.Models;
using BaseSourceImpl.Domains.Entities.UserRole;
using System.Linq;

namespace BaseSourceImpl.Application.Services.User
{
    /// <summary>
    /// UserService Implementation
    /// Business Logic Layer - Sử dụng AutoMapper
    /// </summary>
    public class UserService : BaseService<UserEntity>, IUserService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
            : base(unitOfWork, httpContextAccessor)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ValueResponse<UserViewModel>> GetByIdAsync(int id)
        {
            UserEntity userEntity = await _unitOfWork.Repository<UserEntity>().GetByIdAsync(id);
            if (userEntity == null)
                throw new BaseApplicationException(UserErrorCodes.USER_NOT_FOUND, $"User with ID {id} not found");

            // populate role ids for view model
            var userRoles = await _unitOfWork.Repository<UserRoleEntity>().GetAllAsync(ur => ur.UserId == userEntity.Id);
            var roleIds = userRoles?.Select(ur => ur.RoleId).Distinct().ToList() ?? new List<int>();

            var vm = _mapper.Map<UserViewModel>(userEntity);
            vm.RoleIdList = roleIds;

            return new ValueResponse<UserViewModel>(vm);
        }

        public async Task<ValueResponse<UserViewModel>> GetByUserNameAsync(string userName)
        {
            UserEntity userEntity = await _unitOfWork.Repository<UserEntity>().FindAsync(i => i.UserName == userName);
            if (userEntity == null)
                throw new BaseApplicationException(UserErrorCodes.USER_NOT_FOUND, $"User with UserName {userName} not found");

            // load role ids for the user and map into the view model
            var userRoles = await _unitOfWork.Repository<UserRoleEntity>().GetAllAsync(ur => ur.UserId == userEntity.Id);
            var roleIds = userRoles?.Select(ur => ur.RoleId).Distinct().ToList() ?? new List<int>();

            var vm = _mapper.Map<UserViewModel>(userEntity);
            vm.RoleIdList = roleIds;

            return new ValueResponse<UserViewModel>(vm);
        }

        public async Task<PageResponse<UserViewModel>> GetPageAsync(UserSearchModel searchModel)
        {
            var spec = AuthSpecifications.PagingSpecification(searchModel.SearchText, searchModel.CurrentPage, searchModel.Size);
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
                dto.Password = PasswordEncoder.Encode(dto.Password);

                // Map DTO -> Entity
                var entity = _mapper.Map<UserEntity>(dto);
                AuditHelper.SetCreateAudit(entity, CurrentUserId);

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
                throw new BaseApplicationException(UserErrorCodes.USER_NOT_FOUND, $"User with ID {dto.Id} not found", System.Net.HttpStatusCode.NotFound);

            // Map DTO -> Entity (chỉ update các fields được phép)
            _mapper.Map(dto, entity);

            _unitOfWork.Repository<UserEntity>().Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserViewModel>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Repository<UserEntity>().GetByIdAsync(id);
            if (entity == null)
                throw new BaseApplicationException(UserErrorCodes.USER_NOT_FOUND, $"User with ID {id} not found");

            await _unitOfWork.Repository<UserEntity>().DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

    }
}
