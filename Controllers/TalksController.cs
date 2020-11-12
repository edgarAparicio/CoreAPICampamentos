using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository campamentoRepositorio;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;
        public TalksController(ICampRepository campRepository, IMapper iMapper, LinkGenerator link)
        {
            campamentoRepositorio = campRepository;
            mapper = iMapper;
            linkGenerator = link;
        }

        [HttpGet]
        //http://localhost:6600/api/camps/ATL2018/talks
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var charlas = await campamentoRepositorio.GetTalksByMonikerAsync(moniker);
                TalkModel[] talkModel = mapper.Map<TalkModel[]>(charlas);
                return Ok(talkModel);
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Falla en la BD");
            }
        }

        [HttpGet ("{talkId:int}")]
        //http://localhost:6600/api/camps/ATL2018/talks/2
        public async Task<ActionResult<TalkModel>> Get(string moniker, int talkId)
        {
            try
            {
                var charla = await campamentoRepositorio.GetTalkByMonikerAsync(moniker, talkId);
                if (charla == null) return NotFound("La charla no existe");
                TalkModel talkModel = mapper.Map<TalkModel>(charla);
                return Ok(talkModel);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Falla en la BD");
            }
        }

        [HttpPost]
        //http://localhost:6600/api/camps/ATL2018/talks
            //{
            //    "title": "CharlaPrueba2",
            //    "abstract": "probando agregar una nueva charla 2 con 40 caracteres como minimo",
            //    "Level" : "300",
            //    "speaker": { "speakerId" : 1 }
            //}
    public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel modeloTalk)
        {
            try
            {
                var campamento = await campamentoRepositorio.GetCampAsync(moniker);
                if (campamento == null) return BadRequest("El campamento no existe");
                var charla = mapper.Map<Talk>(modeloTalk);
                charla.Camp = campamento;

                if(modeloTalk.Speaker == null)
                {
                    return BadRequest("El Speaker id es requerido");
                }
                var speaker = await campamentoRepositorio.GetSpeakerAsync(modeloTalk.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("El speaker no fue encontrado");
                charla.Speaker = speaker;
                campamentoRepositorio.Add(charla);

                if(await campamentoRepositorio.SaveChangesAsync())
                {
                    var url = linkGenerator.GetPathByAction(
                        HttpContext,
                        "Get",
                        values: new
                        {
                            moniker,
                            id = charla.TalkId
                        }
                        );
                    return Created(url, mapper.Map<TalkModel>(charla));
                }
                else 
                {
                    return BadRequest("Fallo en la BD");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Falla en la BD");
            }
        }

        [HttpPut("{idTalk:int}")]
        //http://localhost:6600/api/camps/ATL2018/talks/3
        //De esta forma se envian los datos
        //{
        //        "talkId" : 3,
        //        "title": "CharlaPrueba100",
        //        "abstract": "probando actualizr una nueva charla 100 con 40 caracteres como minimo",
        //        "Level" : "200",
        //        "speaker" : 
        //        {
        //            "speakerId" : 2
        //        }
        // }
    public async Task<ActionResult<TalkModel>> Put(string moniker, int idTalk, TalkModel modeloTalk)
        {
            try
            {
                //Recuperamos la charla por medio del moniker y el id e indicamos que el modeloTalk traiga todo mediante el true
                //lo asignamos a la variable charla
                var charla = await campamentoRepositorio.GetTalkByMonikerAsync(moniker, idTalk, true);
                //Validamos si la charla existe
                if (charla == null) return NotFound("No se encontro la charla indicada");
                
                //Validamos si el modelo trae valores en la propiedad speaker, si trae recuperamos el speaker y lo asignamos a la variable 
                //speaker, enseguida validamos si el speaker existe: la propiedad Charla.Speaker sera igual a la variable speaker
                if(modeloTalk.Speaker != null)
                {
                    var speaker = await campamentoRepositorio.GetSpeakerAsync(modeloTalk.Speaker.SpeakerId);
                    if(speaker != null)
                    {
                        charla.Speaker = speaker;
                    }
                }

                //Mapeamos el modelo a la variable charla
                mapper.Map(modeloTalk, charla);

                //Validamos que los cambiamos hayan sido guardados con exito y hacemos solo el mapeo para la pura charla
                //Recordemos que la charla tiene 2 propiedades navegables Camp y Speaker
                //Tenemos que ir a la clase PerfilCampamento e indicar que el CreateMap ignore los valores de las 2 propiedades tanto de camp como de speaker
                if(await campamentoRepositorio.SaveChangesAsync())
                {
                    return mapper.Map<TalkModel>(charla);
                }
                else
                {
                    return BadRequest("Error al actualizar la BD");
                }

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Fallo en la BD");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var charla = await campamentoRepositorio.GetTalkByMonikerAsync(moniker, id);
                if(charla == null)
                {
                    return NotFound("La Charla no fue encontrada");
                }

                campamentoRepositorio.Delete(charla);

                if(await campamentoRepositorio.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Fallo en la BD");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Fallo en la BD StatusCode");
            }
        }

    }
}
