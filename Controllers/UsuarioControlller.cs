using API.DAL;
using API.Helpers;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("Usuario")]
    [ApiController]
    public class UsuarioControlller : ControllerBase
    {
        private static readonly SqlConnection _conn = Connection.GetConnection();
        [HttpPost]
        [Route("Agregar")]
        public IActionResult AddAnUser(UserRegister usuario)
        {
            string q = Query.Make("usp_agregarUsuario", new string[] { "@DocumentoU", "@fotoU", "@nombreU", "@apellidoU", "@edadU", "@telefonoU", "@contraseñaU", "@correoU" });
            SqlCommand comando = new(q, _conn);
            comando.Parameters.AddWithValue("@DocumentoU", usuario.Documento);
            comando.Parameters.AddWithValue("@fotoU", usuario.Foto);
            comando.Parameters.AddWithValue("@nombreU", usuario.Nombre);
            comando.Parameters.AddWithValue("@apellidoU", usuario.Apellido);
            comando.Parameters.AddWithValue("@edadU", usuario.Edad);
            comando.Parameters.AddWithValue("@telefonoU", usuario.Telefono);
            comando.Parameters.AddWithValue("@contraseñaU", usuario.Contraseña);
            comando.Parameters.AddWithValue("@correoU", usuario.Correo);
            _conn.Open();
            try
            {
                comando.ExecuteNonQuery();
                return Ok(new ResponseSender(StatusMessages.OK, "Usuario correctamente agregado"));
            }
            catch (SqlException ex)
            {
                return BadRequest( new ResponseSender(StatusMessages.DENIED, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.ERROR, ex.Message));
            }
            finally
            {
                _conn.Close();
            }
        }

        [HttpDelete]
        [Route("Eliminar")]
        [Authorize]
        public IActionResult RemoveMyAccount()
        {
            string rol = Token.GetUserRol(HttpContext);
            if (rol == null)  return BadRequest(new ResponseSender(StatusMessages.DENIED, "Este metodo es unicamente para eliminar usuarios no conductores")); 
            string? Documento = Token.GetClaim(HttpContext, "Documento").Value;
            string query = "EXECUTE ";
            switch (rol)
            {
                case "SUPERADMIN":
                    query += $"usp_elminarUsuarioSuperAdmin {Documento}";
                    break;
                case "ADMIN":
                    query += $"usp_eliminarUsuarioAdmin {Documento}";
                    break;
                default:
                    query = $"usp_eliminarUsuario {Documento}";
                    break;
            }
            SqlCommand com = new(query, _conn);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new ResponseSender(StatusMessages.OK, "Usuario eliminado correctamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.ERROR, ex.Message));
            }
            finally
            {
                _conn.Close();
            }
        }

        [HttpPut]
        [Route("Actualizar")]
        [Authorize]
        public IActionResult UpdateAccount(UserRegister user)
        {
            string q = Query.Make("usp_actualizarUsuario", new string[] { "@DocumentoU", "@fotoU", "@nombreU", "@apellidoU", "@edadU", "@telefonoU", "@contraseñaU", "@correoU" });
            SqlCommand com = new(q, _conn);
            if (Token.GetUserRol(HttpContext) == null) return Unauthorized(new ResponseSender("Denegado", "Este Endpoint es unicamente para usuarios"));
            string Documento = Token.GetClaim(HttpContext, "Documento").Value;
            string Contraseña = Token.GetClaim(HttpContext, "Contraseña").Value;
            if (Documento != user.Documento && Contraseña != user.Contraseña)
            {
                return Unauthorized(new ResponseSender(StatusMessages.DENIED, "No puedes actualizar una cuenta que no sea la tuya"));
            }
            com.Parameters.AddWithValue("@DocumentoU", user.Documento);
            com.Parameters.AddWithValue("@fotoU", user.Foto);
            com.Parameters.AddWithValue("@nombreU", user.Nombre);
            com.Parameters.AddWithValue("@apellidoU", user.Apellido);
            com.Parameters.AddWithValue("@edadU", user.Edad);
            com.Parameters.AddWithValue("@telefonoU", user.Telefono);
            com.Parameters.AddWithValue("@contraseñaU", user.Contraseña);
            com.Parameters.AddWithValue("@correoU", user.Correo);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new ResponseSender(StatusMessages.OK, "Cuenta actualizada correctamente"));
            }
            catch (SqlException ex)
            {
                return Unauthorized(new ResponseSender(StatusMessages.DENIED, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.ERROR, ex.Message));
            }
            finally
            {
                _conn.Close();
            }
        }

        [HttpPatch]
        [Route("CambiarRol")]
        [Authorize]
        public IActionResult CambiarRolDeUsuario([FromQuery] string Documento, [FromQuery] string nuevoRol) 
        {
            string rol = Token.GetUserRol(HttpContext);
            if (rol == null) return Unauthorized(new ResponseSender("Denied", "Solo los usuarios pueden usar este metodo"));
            string q = Query.Make("usp_cambiarRolDeUsuario", new string[] { $"'{Documento}'", $"'{nuevoRol}'", $"'{rol}'" });
            SqlCommand com = new(q, _conn);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new ResponseSender(StatusMessages.OK, $"Usuario seteado correctamente como {rol}"));
            } catch (SqlException ex)
            {
                return Unauthorized(new ResponseSender(StatusMessages.DENIED, ex.Message));
            } catch (Exception ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.ERROR, ex.Message));
            } finally
            {
                _conn.Close();
            }
        }
        public class UserRegister : Persona
        {
            public string Documento { get; set; }
        }
    }
}
