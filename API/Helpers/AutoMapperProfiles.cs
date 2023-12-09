using API.DTOs;
using API.Entities;
using API.Entity;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDTO>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(x => x.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(x => x.DateOfBirth.CalcuateAge()));
            CreateMap<Photo, PhotoDTO>();
            CreateMap<MemberUpdateDTO, AppUser>();
            CreateMap<RegisterDTO, AppUser>();
            CreateMap<Message, MessageDTO>()
                .ForMember(dest => dest.SenderPhotoUrl, 
                            o => o.MapFrom(s => s.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(dest => dest.RecipientPhotoUrl, 
                            o => o.MapFrom(s => s.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));

            //CreateMap<DateTime, DateTime>().ConstructUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            //CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ? 
            //    DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
        }
    }
}
