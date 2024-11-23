using AutoMapper;
using dms_dal_new.Entities;
using dms_bl.Models;
using DocumentManagementSystem.DTOs;

namespace DocumentManagementSystem.Mappings
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {//FileType, FileSize
            CreateMap<DocumentItem, DocumentDTO>()
                 .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt
                    => opt.MapFrom(src => $"*{src.Name ?? string.Empty}*"))
                .ForMember(dest => dest.FileType, opt
                    => opt.MapFrom(src => src.FileType))
                .ForMember(dest => dest.FileSize, opt
                    => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.OcrText, opt
                    => opt.MapFrom(src => src.OcrText))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt
                    => opt.MapFrom(src => (src.Name ?? string.Empty).Replace("*", "")))
                .ForMember(dest => dest.FileType, opt
                    => opt.MapFrom(src => src.FileType))
                .ForMember(dest => dest.FileSize, opt
                    => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.OcrText, opt
                    => opt.MapFrom(src => src.OcrText));

            CreateMap<DocumentDTO, Document>()
                 .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt
                    => opt.MapFrom(src => $"*{src.Name ?? string.Empty}*"))
                .ForMember(dest => dest.FileType, opt
                    => opt.MapFrom(src => src.FileType))
                .ForMember(dest => dest.FileSize, opt
                    => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.OcrText, opt
                    => opt.MapFrom(src => src.OcrText))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt
                    => opt.MapFrom(src => (src.Name ?? string.Empty).Replace("*", "")))
                .ForMember(dest => dest.FileType, opt
                    => opt.MapFrom(src => src.FileType))
                .ForMember(dest => dest.FileSize, opt
                    => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.OcrText, opt
                    => opt.MapFrom(src => src.OcrText));

            CreateMap<Document, DocumentItem>()
                 .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt
                    => opt.MapFrom(src => $"*{src.Name ?? string.Empty}*"))
                .ForMember(dest => dest.FileType, opt
                    => opt.MapFrom(src => src.FileType))
                .ForMember(dest => dest.FileSize, opt
                    => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.OcrText, opt
                    => opt.MapFrom(src => src.OcrText))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt
                    => opt.MapFrom(src => (src.Name ?? string.Empty).Replace("*", "")))
                .ForMember(dest => dest.FileType, opt
                    => opt.MapFrom(src => src.FileType))
                .ForMember(dest => dest.FileSize, opt
                    => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.OcrText, opt
                    => opt.MapFrom(src => src.OcrText));
        }
    }
}
