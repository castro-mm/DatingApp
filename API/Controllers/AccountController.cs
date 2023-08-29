﻿using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    public IMapper _mapper { get; }

    public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
        this._mapper = mapper;
        this._context = context;
        this._tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto) 
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

        var user = this._mapper.Map<AppUser>(registerDto);

        using var hmac = new HMACSHA512();

        user.UserName = registerDto.Username.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        user.PasswordSalt = hmac.Key; // User Cypher Key. Each user has a specific key to decode his password.

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Username = user.UserName,
            Token = this._tokenService.CreateToken(user),
            PhotoUrl = user.Photos.Find(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) 
    {
        var user = await _context.Users.Include(p => p.Photos).SingleOrDefaultAsync(p => p.UserName == loginDto.Username);

        if (user == null) return Unauthorized("Invalid User");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for(int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
        }

        return new UserDto
        {
            Username = user.UserName,
            Token = this._tokenService.CreateToken(user),
            PhotoUrl = user.Photos.Find(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    private async Task<bool> UserExists(string username) {        

        return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
}
