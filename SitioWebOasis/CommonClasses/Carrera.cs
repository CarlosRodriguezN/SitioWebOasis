using System;

namespace SitioWebOasis.CommonClasses.GestionUsuarios
{
	/// <summary>
	/// Representa una Carrera espec�fica
	/// dentro de la ESPOCH.
	/// </summary>
	[Serializable]
	public class Carrera
	{
		private string _Codigo = null;
		private string _Nombre = null;
        private string _tipoEntidad = null;
        private string _strSede = null;
        private string _codUsuario = null;


        public Carrera(string strCodigo, string strNombre, string strTpoEntidad, string strSede, string strCodUsuario)
		{
			this._Codigo = strCodigo;
			this._Nombre = strNombre;
            this._tipoEntidad = strTpoEntidad;
            this._strSede = strSede;
            this._codUsuario = strCodUsuario;
        }

        public string Codigo
		{
			get { return this._Codigo; }
		}

		public string Nombre
		{
			get { return this._Nombre; }
		}

		public string TipoEntidad
		{
			get { return this._tipoEntidad; }
		}

        public string SedeCarrera
        {
            get { return this._strSede; }
        }

        public string codUsuario
        {
            get { return this._codUsuario; }
        }

		public override string ToString()
		{
			return this._Nombre;
		}

	}
}
