using AutoMapper;
using SILData.Model.SolicitudTurno;

namespace SILData.Model.Mapping
{
  public class AutoMapperProfile : Profile
  {
    public AutoMapperProfile()
    {
      CreateMap<ZonaGeografica, ZonaGeograficaView>();
        //.ForMember(dest=> dest.Destinos, opt => opt.MapFrom(src => src.Destinos is not null? src.Destinos: new List<DestinoView>()));
      CreateMap<Destino, DestinoView>();
      CreateMap<CuposDisponible, ZonaGeograficaView>()
        .ForMember(dest=> dest.zonaGeoId, opt => opt.MapFrom(src => src.ZonaGeograficaId.ToString()))
        .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.ZonaGeografica))
        .ForMember(dest => dest.CentroId, opt => opt.MapFrom(src => src.Centro))
        .ForMember(dest => dest.Disponible, opt => opt.MapFrom(src => src.CuposTotalesADistribuir));
    }
  }
}
