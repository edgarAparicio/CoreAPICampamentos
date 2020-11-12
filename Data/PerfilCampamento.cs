using AutoMapper;
using CoreCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data
{
    public class PerfilCampamento : Profile
    {
        public PerfilCampamento()
        {
            //De la entidad Camp llena el modelo que creamos ModeloCamapamento
            this.CreateMap<Camp, ModeloCampamento>().ReverseMap();

            //De esta forma mapeamos de entidades que tienen relacion a un modelo
            //Para no poner el nombre como lo tiene el modeloCmapamento LocationVenueName
            //le cambiamos el nombre a la propiedad "VenueNameEjemplo" y lo mapeamos de la siguiente forma
            this.CreateMap<Camp, ModeloCampamento>()
                .ForMember(modeloCampamento => modeloCampamento.VenueNameEjemplo, entidadLocation => entidadLocation
                .MapFrom(m => m.Location.VenueName)
                ).ReverseMap();

            ////lo podemos hacer con las propiedades que queramos
            this.CreateMap<Camp, ModeloCampamento>()
                .ForMember(modeloCampamento => modeloCampamento.Direccion1, entidadLocation => entidadLocation
                .MapFrom(m => m.Location.Address1)
                ).ReverseMap();

            //this.CreateMap<Talk, TalkModel>().ReverseMap();

            //Esto se hace para cuando actualicemos un talk desde el controller acepte todos los valores aunque no mandemos un camp y un speaker
            this.CreateMap<Talk, TalkModel>()
                .ReverseMap()
                .ForMember(t => t.Camp, o => o.Ignore())
                .ForMember(t => t.Speaker, o => o.Ignore());

            this.CreateMap<Speaker, SpeakerModel>().ReverseMap();
        }
    }
}
