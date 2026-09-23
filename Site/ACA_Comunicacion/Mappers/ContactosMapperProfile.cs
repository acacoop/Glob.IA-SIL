using AutoMapper;
using Comunicaciones.DataResponse;
using Domain.Entities.ListasContactos;
using Domain.Entities.Personas;

namespace Comunicaciones.Mappers
{
  public class ContactosMapperProfile : Profile
    {
        public ContactosMapperProfile()
        {
            CreateMap<Persona, PersonaDTO>();
            CreateMap<Rol, RolDTO>();
            CreateMap<Centro, CentroDTO>();
            CreateMap<ZonaComercial, ZonaComercialDTO>();
            CreateMap<Cuenta, CuentaDTO>();

            CreateMap<ContactoDTO, Contacto>().ReverseMap().ForMember(
                          dest => dest.Persona,
                          opt => opt.MapFrom(src => src.Persona.Id)
                      );
            CreateMap<TipoContactoDTO, TipoContacto>().ReverseMap();
            CreateMap<ListaContactos, ListaContactosDTO>().ForMember(
                    dest => dest.Contactos,
                    opt => opt.MapFrom(src => src.getHijos())
                ).ForMember(
                    dest => dest.esLista,
                    opt => opt.MapFrom(src => true)
                );
            CreateMap<IDestinatario, ListaContactosDTO>().ForMember(
                    dest => dest.Contactos,
                    opt => opt.MapFrom(src => src.getHijos())
                ).ForMember(
                    dest => dest.Nombre,
                    opt => opt.MapFrom(src => src.getNombre())
                ).ForMember(
                    dest => dest.Descripcion,
                    opt => opt.MapFrom(src => "")
                ).ForMember(
                    dest => dest.Id,
                    opt => opt.MapFrom(src => src.getId())
                ).ForMember(
                    dest => dest.esLista,
                    opt => opt.MapFrom(src => !src.esContacto())
                );
        }
    }
}
