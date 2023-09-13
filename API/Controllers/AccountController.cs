using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace API;

public class AccountController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    public IMapper _mapper { get; }

    public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
    {
        this._mapper = mapper;
        this._userManager = userManager;
        this._tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto) 
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

        var user = this._mapper.Map<AppUser>(registerDto);

        user.UserName = registerDto.Username.ToLower();

        var result = await this._userManager.CreateAsync(user);

        if (!result.Succeeded) return BadRequest(result.Errors);

        var roleResult = await this._userManager.AddToRoleAsync(user, "Member");

        if(!roleResult.Succeeded) return BadRequest(roleResult.Errors);

        return new UserDto
        {
            Username = user.UserName,
            Token = await this._tokenService.CreateToken(user),
            PhotoUrl = user.Photos.Find(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) 
    {
        var user = await this._userManager.Users
            .IgnoreQueryFilters()
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(p => p.UserName == loginDto.Username);

        if (user == null) return Unauthorized("Invalid User");

        var result = await this._userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result) return Unauthorized("Invalid Password");

        return new UserDto
        {
            Username = user.UserName,
            Token = await this._tokenService.CreateToken(user),
            PhotoUrl = user.Photos.Find(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    private async Task<bool> UserExists(string username) {        

        return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
}
