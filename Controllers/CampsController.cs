using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository campamentoRepositorio;
        private readonly IMapper mapperRepositorio;
        private readonly LinkGenerator linkGenerador;

        public CampsController(ICampRepository ICampamentoRepositorio, IMapper mapper, LinkGenerator linkGenerator)
        {
            campamentoRepositorio = ICampamentoRepositorio;
            mapperRepositorio = mapper;
            linkGenerador = linkGenerator;
        }

        [HttpGet("ObtenerObjetoSimple")]
        //http://localhost:6600/api/camps/ObtenerObjetoSimple
        public object Get()
        {
            return new { Monica = "ATL2019", Nombre = "Atlanta" };
        }

        [HttpGet("ObtenerObjetoIActionResult")]
        //http://localhost:6600/api/camps/ObtenerObjetoIActionResult
        public IActionResult ObtenerObjeto()
        {
            return Ok(new { Monica = "ATL2019", Nombfffffffffre = "Atlanta" });
        }

        [HttpGet("ObtenerCampamentosNormal")]
        //http://localhost:6600/api/camps/ObtenerCampamentosNormal
        public async Task<IActionResult> ObtenerCampamentosNormal()
        {
            try
            {
                var resultado = await campamentoRepositorio.GetAllCampsAsync();
                return Ok(resultado);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falla de la Base de Datos");

            }
        }

        //Metodo usando MapperExtensiones para llenar modelos de las entidades
        //http://localhost:6600/api/camps/ObtenerCampamentosMapper
        [HttpGet("ObtenerCampamentosMapper")]
        public async Task<IActionResult> ObtenerListaCampamentos()
        {
            try
            {

                var resultado = await campamentoRepositorio.GetAllCampsAsync();
                ModeloCampamento[] modeloCampamento = mapperRepositorio.Map<ModeloCampamento[]>(resultado);
                return Ok(modeloCampamento);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falla de la Base de Datos");

            }
        }

        //Un metodo mas limpio que devuelve los mismo del anterior un Modelo en lugar de una entidad usando Mapper

        [HttpGet("ObtenerCampmentosMapperLimpio")]
        //http://localhost:6600/api/camps/ObtenerCampmentosMapperLimpio
        public async Task<ActionResult<ModeloCampamento[]>> ObtenerCampamentos()
        {
            try
            {
                var resultado = await campamentoRepositorio.GetAllCampsAsync();
                return mapperRepositorio.Map<ModeloCampamento[]>(resultado);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falla de la Base de Datos");

            }
        }


        [HttpGet("{moniker}")]
        //http://localhost:6600/api/camps/ATL2018
        //Para postman el parametro a recibir debe ser llamado igual al nombre del metodo Get en este caso moniker 
        //y en postman lo mandamos asi: http://localhost:6600/api/camps/ATL2018 el ultimo slash tiene que ser el nombre del moniker
        public async Task<ActionResult<ModeloCampamento>> ObtenerCampamento(string moniker)
        {
            try
            {
                var resultado = await campamentoRepositorio.GetCampAsync(moniker);
                if (resultado == null)
                    return NotFound();
                ModeloCampamento modeloCampamento = mapperRepositorio.Map<ModeloCampamento>(resultado);
                return (modeloCampamento);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falla en la BD");

            }
        }



        [HttpGet("ObtenerCampmentosConEntidadTalk")]
        //http://localhost:6600/api/camps/ObtenerCampmentosConEntidadTalk?entidadTalk=true
        public async Task<ActionResult<ModeloCampamento[]>> ObtenerCampamentosConEntidadTalk(bool entidadTalk = false)
        {
            try
            {
                var resultado = await campamentoRepositorio.GetAllCampsAsync(entidadTalk);
                return mapperRepositorio.Map<ModeloCampamento[]>(resultado);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falla de la Base de Datos");

            }
        }


        [HttpGet("BuscarCampamentoPorFecha")]
        //http://localhost:6600/api/camps/BuscarCampamentoPorFecha?fecha=2018-10-18
        //http://localhost:6600/api/camps/BuscarCampamentoPorFecha?fecha=2018-10-18&incluyeModeloTalk=true
        public async Task<ActionResult<ModeloCampamento[]>> BuscarCampamentoPorFecha(DateTime fecha, bool incluyeModeloTalk = false)
        {
            try
            {
                var resultado = await campamentoRepositorio.GetAllCampsByEventDate(fecha, incluyeModeloTalk);
                if (!resultado.Any())
                {
                    return NotFound();
                }

                ModeloCampamento[] modeloCampamento = mapperRepositorio.Map<ModeloCampamento[]>(resultado);
                return (modeloCampamento);
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falla de la Base de Datos");
            }
        }

        [HttpPost("AgregarCampamento")]
        //http://localhost:6600/api/camps/AgregarCampamento
        //los datos se envian de esta forma por el body tipo "raw" = { "name" : "Edgar", "moniker" : "SD2018" }
        //Si le mandamos datos que no pertencen al modelo lo ignorara
        //No olvidar que el tipo de datos debe ser JSON
        //El frombody acepta valores aunque no vengan completo el modelo 
        public async Task<ActionResult<ModeloCampamento>> AgregarCampamento([FromBody] ModeloCampamento modeloCampamento)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest("LLena bien el modelo");
                }
                var campamentoExiste = await campamentoRepositorio.GetCampAsync(modeloCampamento.Moniker);
                if(campamentoExiste != null)
                {
                    return BadRequest("El apodo ya existe");
                }

                var ubicacion = linkGenerador.GetPathByAction("ObtenerCampamento", "Camps",
                    new { moniker = modeloCampamento.Moniker });
                if (string.IsNullOrEmpty(ubicacion))
                {
                    return BadRequest("no se puede usar el apodo actual");
                }

                var campamento = mapperRepositorio.Map<Camp>(modeloCampamento);
                campamentoRepositorio.Add(campamento);
                if (await campamentoRepositorio.SaveChangesAsync())
                {
                    return Created($"/api/camps/{campamento.Moniker}", mapperRepositorio.Map<ModeloCampamento>(campamento));
                }
                return Ok();
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falla en la BD");
            }
        }
        [HttpPut("{moniker}")]
        //http://localhost:6600/api/camps/ATL2018
        //El ultimo slash es el valor de la propiedad moniker que queremos buscar y el parametro que recibe el string debe llamarse igual que la propiedad moniker
        //De lo contrario no lo encontrara
        //los datos se envian de esta forma por el body tipo "raw" = { "name" : "Edgar", "moniker" : "SD2018" }
        //Si le mandamos datos que no pertencen al modelo lo ignorara
        //No olvidar que el tipo de datos debe ser JSON
        //El frombody acepta valores aunque no vengan completo el modelo 
        public async Task<ActionResult<ModeloCampamento>> ActualizarCampamento(string moniker, [FromBody]ModeloCampamento modeloCampamento) 
        {
            try
            {
                var campamentoViejo = await campamentoRepositorio.GetCampAsync(moniker);
                if(campamentoViejo == null)
                return NotFound("No se encontro el campo con es apodo ");
                
                mapperRepositorio.Map(modeloCampamento, campamentoViejo);
                
                if(await campamentoRepositorio.SaveChangesAsync())
                {
                    return mapperRepositorio.Map<ModeloCampamento>(campamentoViejo);
                }
                return Ok();
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falla en la BD");
            }
        }

        [HttpDelete("{moniker}")]
        //http://localhost:6600/api/camps/Neza2020
        public async Task<ActionResult<ModeloCampamento>> EliminarCampamento(string moniker)
        {
            try
            {
                var campamentoViejo = await campamentoRepositorio.GetCampAsync(moniker);
                if (campamentoViejo == null)
                    return NotFound("No se encontro el campo con es apodo ");

                campamentoRepositorio.Delete(campamentoViejo);

                if (await campamentoRepositorio.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Error al eliminar el campamento");
                }
                
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falla en la BD");
            }

            
        }


    }
}
