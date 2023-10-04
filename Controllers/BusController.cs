using API.DAL;
using API.Helpers;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace API.Controllers
{
    [Route("Bus")]
    [ApiController]
    public class BusController : ControllerBase
    {
        private static readonly SqlConnection _con = Connection.GetConnection();

        [HttpPost]
        [Route("Agregar")]
        [Authorize]
        public IActionResult AddABus([FromBody] Bus bus)
        {

            string rol = Token.GetUserRol(HttpContext);
            if (rol != "ADMIN" && rol != "SUPERADMIN") 
                return Unauthorized(new ResponseSender (StatusMessages.DENIED, "Solo los ADMIN o SUPERADMIN tienen el permiso de agregar buses"));
            string q = "EXECUTE usp_agregarBus @placaB, @fotoB, @numeroR, @modeloB, @colorB,  @capacidadB, @cilindrajeB, @marcaB, @maximaVelocidad";
            SqlCommand com = new(q, _con);
            com.Parameters.AddWithValue("@placaB", bus.PlacaB);
            com.Parameters.AddWithValue("@fotoB", bus.fotoB);
            com.Parameters.AddWithValue("@numeroR", bus.NumeroR);
            com.Parameters.AddWithValue("@modeloB", bus.modeloB);
            com.Parameters.AddWithValue("@colorB", bus.colorB);
            com.Parameters.AddWithValue("@capacidadB", bus.capacidadB);
            com.Parameters.AddWithValue("@cilindrajeB", bus.cilindrajeB);
            com.Parameters.AddWithValue("@marcaB", bus.marcaB);
            com.Parameters.AddWithValue("@maximaVelocidad", bus.maximaVelocidad);
            _con.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new ResponseSender(StatusMessages.OK, "Bus agregado correctamente"));
            }
            catch (SqlException ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.ERROR, ex.Message));
            }
            finally
            {
                _con.Close();
            }
        }

        [HttpDelete]
        [Route("Eliminar")]
        [Authorize]
        public IActionResult DeleteABus([FromQuery] string placa)
        {

            string rol = Token.GetUserRol(HttpContext);
            if (rol != "ADMIN" && rol != "SUPERADMIN") 
                return Unauthorized(new ResponseSender(StatusMessages.DENIED, "Solo los usuarios con rol de ADMIN o SUPERADMIN pueden eliminar un bus"));
            string q = $"EXECUTE usp_eliminarBus '{placa}'";
            SqlCommand com = new(q, _con);
            _con.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new ResponseSender(StatusMessages.OK, "Bus eliminado correctamente junto con las cuentas de los conductores que manejaban dicho bus"));
            }
            catch (SqlException ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.ERROR, ex.Message));
            }
            finally
            {
                _con.Close();
            }
        }

        [HttpGet]
        [Route("Lista")]
        public IActionResult GetBusesByRange([FromQuery] int limit, [FromQuery] int offset = 0)
        {
            if (limit < 1) return BadRequest(new ResponseSender(StatusMessages.ERROR,"El limite debe ser mayor a 0"));

            string q = $"EXECUTE usp_listarBuses {limit}, {offset}";
            SqlDataAdapter da = new(q, _con);
            DataTable dt = new();
            try
            {
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.ERROR, ex.Message));
            }
            List<Bus> buses = new();
            foreach (DataRow el in dt.Rows)
            {
                buses.Add
                    (
                        new Bus
                        {
                            PlacaB = el["PlacaB"].ToString(),
                            fotoB = el["fotoB"].ToString(),
                            NumeroR = (int)el["fkNumeroR"],
                            modeloB = el["modeloB"].ToString(),
                            colorB = el["colorB"].ToString(),
                            capacidadB = (int)el["capacidadB"],
                            cilindrajeB = (int)el["cilindrajeB"],
                            marcaB = el["marcaB"].ToString(),
                            maximaVelocidad = (double)el["maximaVelocidad"]
                        }
                    );
            }

            return Ok(new ResponseSender(StatusMessages.OK, buses));

        }

        [HttpGet]
        [Route("Ruta")]
        public IActionResult GetBusesByRute([FromQuery] int NumeroR)
        {
            string q = $"EXECUTE usp_listarBusesPorRuta {NumeroR}";
            SqlDataAdapter da = new(q, _con);
            DataTable dt = new();
            List<Ruta> rutas = new();
            try
            {
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.ERROR, ex.Message));
            }

            foreach (DataRow el in dt.Rows)
            {
                rutas.Add
                (
                    new Ruta
                    {
                        NumeroR = (int)el["NumeroR"],
                        inicioR = el["inicioR"].ToString(),
                        finR = el["finR"].ToString(),
                        estadoR = (bool)el["estadoR"],
                    }
                );
            }
            return Ok(new ResponseSender(StatusMessages.OK, rutas));
        }

    }
}
