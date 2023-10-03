namespace API.Models
{
    public class Interaccion
    {
        public int FkNumeroR { get; set; }
        public string FkDocumentoU { get; set; }
        public bool MeGusta { get; set; }
        public string Comentario { get; set; }
        public string HoraInteraccion { get; set; }
        public string HorarioSuceso { get; set; }
    }
}
