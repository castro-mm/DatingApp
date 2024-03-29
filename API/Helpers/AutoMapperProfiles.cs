using System;
using System.Linq;
using API.DTO;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Map Entity to Dto and configure the property PhotoUrl of MemberDto to receive the Url's reference from Photos
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl, options => options.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age, options => options.MapFrom(src => src.DateOfBirth.CalculateAge()));

            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderPhotoUrl, options => options.MapFrom(src => src.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.RecipientPhotoUrl, options => options.MapFrom(src => src.Recipient.Photos.SingleOrDefault(x => x.IsMain).Url));

            CreateMap<Photo, PhotoDto>();
            CreateMap<Photo, PhotoForApprovalDto>()
                .ForMember(dest => dest.Username, options => options.MapFrom(src => src.AppUser.UserName));

            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();

            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
        }
    }
}