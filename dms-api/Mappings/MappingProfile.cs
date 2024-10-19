using AutoMapper;
using dms_dal.Entities;
using DocumentManagementSystem.DTOs;

namespace DocumentManagementSystem.Mappings
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<DocumentItem, DocumentDTO>()
                 .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt
                    => opt.MapFrom(src => $"*{src.Name ?? string.Empty}*"))
                .ForMember(dest => dest.IsComplete, opt
                    => opt.MapFrom(src => src.IsComplete))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt
                    => opt.MapFrom(src => (src.Name ?? string.Empty).Replace("*", "")))
                .ForMember(dest => dest.IsComplete, opt
                    => opt.MapFrom(src => src.IsComplete));
        }
    }
}
