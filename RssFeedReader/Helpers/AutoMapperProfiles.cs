using AutoMapper;
using RssFeedReader.DTOs;
using RssFeedReader.Models;

namespace RssFeedReader.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<News, NewsDTO>()
            .ForMember(dest => dest.FeedSource, opt => opt.MapFrom(src => src.FeedSource.Name))
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.Statistics == null ? 0 : src.Statistics.ViewCount));

        CreateMap<RegisterDto, AppUser>();
    }
}
