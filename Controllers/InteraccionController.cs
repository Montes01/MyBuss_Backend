﻿using API.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("Interaccion")]
    [ApiController]
    [Authorize]
    public class InteraccionController : ControllerBase
    {
        private readonly static SqlConnection _conn = Connection.GetConnection();
        [HttpPost]
        [Route("Add")]
        public IActionResult AddInteraction(InteraccionRegister input)
        {
            string tokenS = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenS);
            Claim? CDocumento = token.Claims.FirstOrDefault(c => c.Type == "Documento");
            if (CDocumento == null) return Unauthorized(new { Status = "Denegado", Message = "Solo los usuarios tienen acceso a esta accion" });
            string Documento = CDocumento.Value;
            string q = "EXECUTE usp_agregarInteraccion @fkNumeroR,@fkDocumentoU,@meGusta,@comentario,@horarioSuceso";
            SqlCommand com = new(q, _conn);
            com.Parameters.AddWithValue("@fkNumeroR", input.FkNumeroR);
            com.Parameters.AddWithValue("@fkDocumentoU", input.FkDocumentoU);
            com.Parameters.AddWithValue("@meGusta", input.MeGusta);
            com.Parameters.AddWithValue("@comentario", input.Comentario);
            com.Parameters.AddWithValue("@horarioSuceso", input.HorarioSuceso);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = "La interaccion fue agregada correctamente"
                });
            }
            catch (SqlException ex)
            {
                return Unauthorized(new
                {
                    Status = "Error",
                    messsage = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            }
            finally
            {
                _conn.Close();
            }
        }

        [HttpPatch]
        [Route("Like")]
        public IActionResult AddJustALike([FromQuery] int NumeroR)
        { 
            string tokenS = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenS);
            Claim? CDocumento = token.Claims.FirstOrDefault(C => C.Type == "Documento");
            if (CDocumento == null) return Unauthorized(new { Status = "Denegado", Message = "Solo los usuarios pueden acceder a este punto" });
            string Documento = CDocumento.Value;
            string q = $"EXECUTE usp_agregarMeGusta {NumeroR}, '{Documento}'";
            SqlCommand com = new(q, _conn);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = "Like agregado correctamente"
                });
            } catch (SqlException ex)
            {
                return Unauthorized(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            } catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            } finally
            {
                _conn.Close();
            }





        }

        [HttpDelete]
        [Route("RemoveLike")]
        public IActionResult RemoveALike([FromQuery] int NumeroR)
        {
            string tokenS = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenS);
            Claim? CDocumento = token.Claims.FirstOrDefault(C => C.Type == "Documento");
            if (CDocumento == null) return Unauthorized(new { Status = "Denegado", Message = "Solo los usuarios pueden acceder a este punto" });
            string Documento = CDocumento.Value;
            string q = $"EXECUTE usp_eliminarmeGusta {NumeroR}, '{Documento}'";
            SqlCommand com = new(q, _conn);
            try
            {
                com.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = "Me gusta eliminado correctamente"
                });
            } catch (SqlException ex)
            {
                return Unauthorized(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            } catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            } finally
            {
                _conn.Close();
            }
        }

        [HttpDelete]
        [Route("RemoveComment")]
        public IActionResult RemoveAComment([FromQuery] int NumeroR)
        {
            string tokenS = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenS);
            Claim? CDocumento = token.Claims.FirstOrDefault(C => C.Type == "Documento");
            if (CDocumento == null) return Unauthorized(new { Status = "Denegado", Message = "Solo los usuarios pueden acceder a este punto" });
            string Documento = CDocumento.Value;
            string q = $"EXECUTE usp_eliminarmeGusta {NumeroR}, '{Documento}'";
            SqlCommand com = new(q, _conn);
            try
            {
                com.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = "Comentario eliminado correctamente"
                });
            }
            catch (SqlException ex)
            {
                return Unauthorized(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            }
            finally
            {
                _conn.Close();
            }

        }

        [HttpPut]
        [Route("UpdateComment")]
        public IActionResult UpdateAComment([FromBody] UpdateComment data)
        {
            string tokenS = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenS);
            Claim? CDocumento = token.Claims.FirstOrDefault(C => C.Type == "Documento");
            if (CDocumento == null) return Unauthorized(new { Status = "Denegado", Message = "Solo los usuarios pueden acceder a este punto" });
            string Documento = CDocumento.Value;
            string q = $"EXECUTE usp_actualizarComentario {data.NumeroR}, '{Documento}', '{data.Comentario}', '{data.HorarioSuceso}'";
            SqlCommand com = new(q, _conn);
            try
            {
                com.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = "Comentario actualizado correctamente"
                });
            }
            catch (SqlException ex)
            {
                return Unauthorized(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            }
            finally
            {
                _conn.Close();
            }
        }
        
        [HttpPatch]
        [Route("Comment")]
        public IActionResult AddJustAComment([FromQuery] UpdateComment data)
        {
            string tokenS = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenS);
            Claim? CDocumento = token.Claims.FirstOrDefault(C => C.Type == "Documento");
            if (CDocumento == null) return Unauthorized(new { Status = "Denegado", Message = "Solo los usuarios pueden acceder a este punto" });
            string Documento = CDocumento.Value;
            string q = $"EXECUTE usp_agregarMeGusta {data.NumeroR}, '{Documento}', '{data.Comentario}', '{data.HorarioSuceso}'";
            SqlCommand com = new(q, _conn);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = "Comentario agregado correctamente"
                });
            }
            catch (SqlException ex)
            {
                return Unauthorized(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            }
            finally
            {
                _conn.Close();
            }
        }

    }

    public class UpdateComment
    {
    public int NumeroR { get; set; }
    public string? Comentario { get; set; }
    public string? HorarioSuceso { get; set; }
}

    public class InteraccionRegister
    {
        public int FkNumeroR { get; set; }
        public string FkDocumentoU { get; set; }
        public bool MeGusta { get; set; }
        public string? Comentario { get; set; }
        public string? HorarioSuceso { get; set; }
    }   
}