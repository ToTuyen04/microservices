using AutoMapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;
using eCommerce.Core.RepositoryContracts;
using eCommerce.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.Core.Services;
internal class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;
    public UsersService(IUsersRepository usersRepository, IMapper mapper)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid? userID)
    {
        ApplicationUser? user = await _usersRepository.GetUserByUserID(userID);
        if(user == null)
            return null;
        UserDTO? userDTO = _mapper.Map<UserDTO>(user);
        return userDTO;
    }

    public async Task<AuthenticationResponse?> Login(LoginRequest request)
    {
        ApplicationUser? user = await _usersRepository.GetUserByEmailAndPassword(request.Email, request.Password);
        if (user == null)
            return null;
        //return new AuthenticationResponse(
        //    user.UserID,
        //    user.Email, 
        //    user.PersonName, 
        //    user.Gender, "token", 
        //    Success: true);
        return _mapper.Map<AuthenticationResponse>(user) with { Token = "token", Success = true };
    }

    public async Task<AuthenticationResponse?> Register(RegisterRequest request)
    {
        ApplicationUser user = new ApplicationUser()
        {
            PersonName = request.PersonName,
            Email = request.Email,
            Password = request.Password,
            Gender = request.Gender.ToString(),
        };
        ApplicationUser? registeredUser = await _usersRepository.AddUser(user);
        if (registeredUser == null)
            return null;
        //return new AuthenticationResponse(
        //    registeredUser.UserID, 
        //    registeredUser.Email, 
        //    registeredUser.PersonName, 
        //    registeredUser.Gender, 
        //    "token", 
        //    Success: true
        //);
        return _mapper.Map<AuthenticationResponse>(user) with { Token = "token", Success = true };
    }
}

