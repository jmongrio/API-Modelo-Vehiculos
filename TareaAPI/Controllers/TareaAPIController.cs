using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TareaAPI.Models;

namespace TareaAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/[Controller]")]
    public class TareaAPIController : Controller
    {
        private static Random random = new Random();
        private readonly IMemoryCache _cache;

        public TareaAPIController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpPost("crearModelo")]
        [Route("crearModelo")]
        public ActionResult CrearModelo([FromBody] Modelo modelo)
        {
            try
            {
                List<Modelo> listaModelos = ObtenerListaModelos();
                Boolean formatoCorrecto = RevisonIdModelo(listaModelos, modelo);

                if (formatoCorrecto == false)
                {
                    return BadRequest("Se ingreso un ID que ya existe, vuelva a intentarlo.");
                }
                else
                {
                    guardarModelo(modelo);

                    return CreatedAtAction(nameof(CrearModelo), new { id = modelo.IdModelo }, modelo);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("crearVehiculo")]
        [Route("crearVehiculo")]
        public ActionResult CrearVehiculo([FromBody] Vehiculo vehiculo)
        {
            try
            {
                List<Vehiculo> listaVehiculos = ObtenerListaVehiculos();
                List<Modelo> listaModelos = ObtenerListaModelos();

                Boolean formatoCorrecto = RevisionPlacaVehiculo(listaVehiculos, vehiculo);
                Boolean idModeloExiste = RevisionIdModeloExista(listaModelos, vehiculo);

                if (formatoCorrecto == false)
                {
                    return BadRequest("Se ingreso una placa que ya existe, vuelva a intentarlo.");
                }
                else
                {
                    if (idModeloExiste == false)
                    {
                        return BadRequest("El id del modelo no existe, vuelva a intentarlo.");
                    }
                    else
                    {
                        guardarVehiculo(vehiculo);
                        return CreatedAtAction(nameof(CrearModelo), new { id = vehiculo.PlacaVehiculo }, vehiculo);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ObtenerModelo")]
        [Route("obtenerModelo")]
        public ActionResult<IEnumerable<Modelo>> ObtenerModelos()
        {
            try
            {
                List<Modelo> listaModelo = ObtenerListaModelos();

                return Ok(listaModelo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("ActualizarVehiculo")]
        [Route("actualizarVehiculo/{placaVehiculo}")]
        public ActionResult ActualizarVehiculo(string placaVehiculo, [FromBody] Vehiculo vehiculo)
        {
            try
            {
                List<Vehiculo> listaVehiculos = ObtenerListaVehiculos();
                Boolean existe = false;

                for (int i = 0; i < listaVehiculos.Count; i++)
                {
                    if (placaVehiculo == listaVehiculos[i].PlacaVehiculo)
                    {
                        vehiculoEditado(placaVehiculo, vehiculo);
                        existe = true;
                    }
                }

                if (existe == false)
                {
                    return NotFound();
                }

                return CreatedAtAction(nameof(ActualizarVehiculo), new { id = vehiculo.PlacaVehiculo }, vehiculo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("filtrarModelo")]
        [Route("filtrarModelo/{Id}")]
        public ActionResult<IEnumerable<Models.Vehiculo>> filtrarVehiculos(String id)
        {
            try
            {
                List<Models.Modelo> listaModelos = ObtenerListaModelos();
                List<Models.Vehiculo> listaVehiculos = ObtenerListaVehiculos();
                List<Models.Vehiculo> listaVehiculosFiltrados = new List<Vehiculo>();

                Boolean existe = false;

                for (int i = 0; i < listaModelos.Count; i++)
                {
                    if (id == listaModelos[i].IdModelo)
                    {
                        existe = true;
                    }
                }

                if (existe == true)
                {
                    for (int i = 0; i < listaVehiculos.Count; i++)
                    {
                        if (id == listaVehiculos[i].IdModelo)
                        {
                            listaVehiculosFiltrados.Add(listaVehiculos[i]);
                        }
                    }

                    return Ok(listaVehiculosFiltrados);
                }
                else
                {
                    return BadRequest("No existen ningún vehículo asociados a este modelo, vuelva a intentarlo.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region Métodos para obtener las listas
        //--------------------Métodos obtener--------------------
        private List<Modelo> ObtenerListaModelos()
        {
            List<Modelo> listaModelos;

            if (_cache.Get("listaModelos") is null)
            {
                listaModelos = new List<Modelo>();
                _cache.Set("listaModelos", listaModelos);
            }
            else
            {
                listaModelos = (List<Modelo>)_cache.Get("listaModelos");
            }

            return listaModelos;
        }

        private List<Vehiculo> ObtenerListaVehiculos()
        {
            List<Vehiculo> listaVehiculo;

            if (_cache.Get("listaVehiculos") is null)
            {
                listaVehiculo = new List<Vehiculo>();
                _cache.Set("listaVehiculos", listaVehiculo);
            }
            else
            {
                listaVehiculo = (List<Vehiculo>)_cache.Get("listaVehiculos");
            }

            return listaVehiculo;
        }
        #endregion

        #region Métodos para guardar
        /// <summary>
        /// Agrega los nuevos datos a la lista.
        /// </summary>
        /// <param name="modelo">Posee los datos que seran almacenados</param>
        private void guardarModelo(Modelo modelo)
        {
            List<Modelo> listaModelos;

            listaModelos = ObtenerListaModelos();

            listaModelos.Add(modelo);
        }

        /// <summary>
        /// Agrega los nuevos datos a la lista.
        /// </summary>
        /// <param name="vehiculo">Posee los datos que seran almacenados</param>
        private void guardarVehiculo(Vehiculo vehiculo)
        {
            List<Vehiculo> listaVehiculo;

            listaVehiculo = ObtenerListaVehiculos();

            listaVehiculo.Add(vehiculo);
        }
        #endregion

        #region Método para editar vehículo
        /// <summary>
        /// Edita los datos de un vehiculo seleccionado mediante la placa.
        /// </summary>
        /// <param name="placa">Identificación del objeto vehículo</param>
        /// <param name="vehiculo">Objeto con los valores editados</param>
        /// <returns>lista con el elemento editado</returns>
        private Vehiculo vehiculoEditado(string placa, Vehiculo vehiculo)
        {
            Vehiculo vehiculoEditado = null;

            List<Vehiculo> listaVehiculos = ObtenerListaVehiculos();

            for (int i = 0; i < listaVehiculos.Count; i++)
            {
                if (listaVehiculos[i].PlacaVehiculo == placa)
                {
                    listaVehiculos.RemoveAt(i);

                    listaVehiculos.Add(vehiculo);
                }
            }

            return vehiculoEditado;
        }
        #endregion

        #region Métodos para evaluar la Placa del Vehículo
        public static string RandomPlaca(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public Boolean RevisionPlacaVehiculo(List<Vehiculo> listaVehiculos, Vehiculo vehiculo)
        {
            String idRandom = RandomPlaca(6);
            Boolean correcto = true;

            if (listaVehiculos.Count == 0)
            {
                if (vehiculo.PlacaVehiculo == "")
                {
                    vehiculo.PlacaVehiculo = idRandom;
                }
            }
            else
            {
                for (int i = 0; i < listaVehiculos.Count; i++)
                {
                    if (vehiculo != null)
                    {
                        if (vehiculo.PlacaVehiculo == "")
                        {
                            if (listaVehiculos[i].PlacaVehiculo == idRandom)
                            {
                                idRandom = RandomId(6);
                            }
                            else
                            {
                                vehiculo.PlacaVehiculo = idRandom;
                            }
                        }
                        else
                        {
                            if (listaVehiculos[i].PlacaVehiculo == vehiculo.PlacaVehiculo)
                            {
                                correcto = false;
                            }
                        }
                    }
                    else
                    {
                        correcto = false;
                    }
                }
            }

            return correcto;
        }

        public Boolean RevisionIdModeloExista(List<Modelo> listaModelos, Vehiculo vehiculo)
        {
            Boolean correcto = true;

            if (listaModelos.Count == 0)
            {
                correcto = false;
            }
            else
            {
                for (int i = 0; i < listaModelos.Count; i++)
                {
                    if (listaModelos[i].IdModelo != vehiculo.IdModelo)
                    {
                        correcto = false;
                    }
                    else
                    {
                        return correcto = true;
                    }
                }
            }

            return correcto;
        }

        #endregion

        #region Método para evaluar Id del Modelo
        public static string RandomId(int length)
        {
            const string numbers = "0123456789";
            return new string(Enumerable.Repeat(numbers, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public Boolean RevisonIdModelo(List<Modelo> listaModelos, Modelo modelo)
        {
            String idRandom = RandomId(3);
            Boolean correcto = true;

            if (listaModelos.Count == 0)
            {
                if (modelo.IdModelo == "")
                {
                    modelo.IdModelo = idRandom;
                }
            }
            else
            {
                for (int i = 0; i < listaModelos.Count; i++)
                {
                    if (modelo != null)
                    {
                        if (modelo.IdModelo == "")
                        {
                            if (listaModelos[i].IdModelo == idRandom)
                            {
                                idRandom = RandomId(3);
                            }
                            else
                            {
                                modelo.IdModelo = idRandom;
                            }
                        }
                        else
                        {
                            if (listaModelos[i].IdModelo == idRandom)
                            {
                                correcto = false;
                            }
                        }
                    }
                    else
                    {
                        correcto = false;
                    }
                }
            }

            return correcto;
        }
        #endregion
    }
}
