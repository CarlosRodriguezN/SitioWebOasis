﻿using GestorErrores;
using OAS_Seguridad.Cliente;
using SitioWebOasis.CommonClasses.GestionUsuarios;
using SitioWebOasis.Models;
using SitioWebOasis.WSDatosUsuario;
using System;
using System.Data;
using System.Net;

namespace SitioWebOasis.Library
{
    public class DatosCarrera
    {
        protected string _strNombreBD { get; set; }

        protected string _strUbicacion { get; set; }

        protected string _strCodAsignatura { get; set; }

        protected string _strCodNivel { get; set; }

        protected string _strCodParalelo { get; set; }


        protected WSInfoCarreras.dtstPeriodoVigente _dtstPeriodoVigente = new WSInfoCarreras.dtstPeriodoVigente();


        public DatosCarrera() {}

        
        public Usuario UsuarioActual
        {
            get { return (Usuario)System.Web.HttpContext.Current.Session["UsuarioActual"]; }
        }


        protected void _cargarInformacionCarrera()
        {
            try
            {
                //  Obtengo informacion de las carreras
                ProxySeguro.InfoCarreras ic = new ProxySeguro.InfoCarreras();

                WSInfoCarreras.dtstBDCarreras dsCarrera = ic.GetCarrera(UsuarioActual.CarreraActual.Codigo.ToString());
                this._strNombreBD = dsCarrera.BDCarreras.Rows[0]["strBaseDatos"].ToString();
                this._strUbicacion = UsuarioActual.CarreraActual.Codigo.ToString();

                //  Informacion del periodo vigente en carrera
                this._dtstPeriodoVigente = ic.GetPeriodoVigenteCarrera(UsuarioActual.CarreraActual.Codigo.ToString());
            }
            catch (Exception ex)
            {
                Errores err = new Errores();
                err.SetError(ex, "_cargarInformacionCarrera");
            }
        }


        public WSInfoCarreras.dtstPeriodoVigente _dataPeriodoAcademicoVigente()
        {
            WSInfoCarreras.dtstPeriodoVigente dsPeriodoVigente = new WSInfoCarreras.dtstPeriodoVigente();

            try
            {
                ProxySeguro.InfoCarreras ic = new ProxySeguro.InfoCarreras();
                dsPeriodoVigente = ic.GetPeriodoVigenteCarrera(UsuarioActual.CarreraActual.Codigo.ToString());
            }
            catch (Exception ex)
            {
                Errores err = new Errores();
                err.SetError(ex, "_dataPeriodoAcademicoVigente");
            }

            return dsPeriodoVigente;
        }


        protected string _getNumOrdinal(string numero, string tpo="nivel")
        {
            string[] ciclosAcademicos = new string[10] { "1er", "2do", "3er", "4to", "5to", "6to", "7mo", "8vo", "9no", "10mo" };
            string[] matricula = new string[3] { "1ra", "2da", "3ra" };

            return (tpo == "nivel") ? ciclosAcademicos[Convert.ToInt32(numero.ToString()) - 1]
                                    : matricula[Convert.ToInt32(numero.ToString()) - 1];
        }


        protected string _getPromedio(DataRow item, string strParcialActivo)
        {
            string prm = string.Empty;
            decimal n1 = default(decimal);
            decimal n2 = default(decimal);
            decimal n3 = default(decimal);
            decimal rst = default(decimal);
            decimal dtaParcial = default(decimal);

            try
            {
                n1 = Convert.ToDecimal(item["bytNota1"]);
                n2 = Convert.ToDecimal(item["bytNota2"]);
                n3 = Convert.ToDecimal(item["bytNota3"]);
                dtaParcial = (strParcialActivo != "P")  ? Convert.ToDecimal(strParcialActivo) 
                                                        : Convert.ToDecimal("3");

                rst = (n1 + n2 + n3);
                prm = Decimal.Round(Decimal.Divide(rst, dtaParcial), 2).ToString();
            }catch (Exception ex){
                Errores err = new Errores();
                err.SetError(ex, "_getPromedio");
            }

            return prm;
        }


        private WSDatosUsuario.dtstDatosCursosCarrera _getCursosCarrera()
        {
            WSDatosUsuario.dtstDatosCursosCarrera dsCC = new dtstDatosCursosCarrera();

            try
            {
                ProxySeguro.DatosUsuario du = new ProxySeguro.DatosUsuario();
                dsCC = du.getCursosCarrera( UsuarioActual.CarreraActual.Codigo.ToString(),
                                            this._dtstPeriodoVigente.Periodos.Rows[0]["strCodigo"].ToString());
            }
            catch (Exception ex)
            {
                Errores err = new Errores();
                err.SetError(ex, "_getCursosCarrera");
            }

            return dsCC;
        }


        protected string _getDescripcionCurso(string strCodNivel, string strCodParalelo)
        {
            string rst = default(string);

            try
            {
                dtstDatosCursosCarrera dsCursosCarrera = this._getCursosCarrera();
                DataRow[] rstFiltro = dsCursosCarrera.CursosCarrera.Select("strCodParalelo = '" + strCodParalelo + "' AND strCodNivel = '" + strCodNivel + "'");

                rst = (rstFiltro.Length > 0)
                        ? rstFiltro[0]["strDescripcionNivel"].ToString()
                        : "";
            }
            catch (Exception ex)
            {
                Errores err = new Errores();
                err.SetError(ex, "_getDescripcionCurso");
            }

            return rst;
        }


        protected WSInfoCarreras.ParametrosCarrera _getParametrosCarrera()
        {
            WSInfoCarreras.ParametrosCarrera pc = new WSInfoCarreras.ParametrosCarrera();

            try{
                WSInfoCarreras.InfoCarreras ic = new WSInfoCarreras.InfoCarreras();
                pc = ic.GetParametrosCarrera(this.UsuarioActual.CarreraActual.Codigo.ToString());
            }
            catch (Exception ex)
            {
                Errores err = new Errores();
                err.SetError(ex, "_getParametrosCarrera");
            }

            return pc;
        }


        protected void _getDatosMateria(ref string strAsignatura, 
                                        ref string strNivel, 
                                        ref string strPeriodo, 
                                        ref string strDocente, 
                                        ref string strSistema, 
                                        ref float numCreditos, 
                                        ref byte fHorasTeo, 
                                        ref byte fHorasPra)
        {
            try{
                ProxySeguro.GestorEvaluacion ge = new ProxySeguro.GestorEvaluacion();
                ge.CookieContainer = new CookieContainer();
                ge.set_fBaseDatos(this._strNombreBD);
                ge.set_fUbicacion(this._strUbicacion);

                //Buscando los datos de la materia
                ge.GetDatosMateriaActa( this._dtstPeriodoVigente.Periodos.Rows[0]["strCodigo"].ToString(),
                                        this._strCodAsignatura,
                                        this._strCodNivel,
                                        this._strCodParalelo, 
                                        ref strAsignatura, 
                                        ref strNivel, 
                                        ref strPeriodo, 
                                        ref strDocente, 
                                        ref strSistema);

                //Buscando los datos del pensum de la materia
                ge.GetDatosMateriaPensum(   this._dtstPeriodoVigente.Periodos.Rows[0]["strCodigo"].ToString(),
                                            this._strCodAsignatura, 
                                            ref numCreditos, 
                                            ref fHorasTeo, 
                                            ref fHorasPra);

            }
            catch(Exception ex)
            {
                Errores err = new Errores();
                err.SetError(ex, "getDatosMateria");

                strAsignatura = string.Empty;
                strNivel = string.Empty;
                strPeriodo = string.Empty;
                strDocente = string.Empty;
                strSistema = string.Empty;
            }
        }

        public string getCodigoAutenticacion(string correoUsuario)
        {
            string codAutenticacion = "false";
            bool rst = false;

            try{

                Random rnd = new Random();
                string numCodigo = Convert.ToString(rnd.Next(0, 1000));
                
                WSAcademicoWCF.Academico correo = new WSAcademicoWCF.Academico();
                rst = correo.EnviarEmailHtml(   "oasis@espoch.edu.ec",
                                                "Sistema académico DTIC - ESPOCH",
                                                correoUsuario,
                                                "Clave confirmacion impresion acta",
                                                "La clave es: " + numCodigo,
                                                "Academico",
                                                "Oasis20|3Desitel");

                codAutenticacion = (rst) ? numCodigo : "false";

            }catch(Exception ex){
                codAutenticacion = "false";

                Errores err = new Errores();
                err.SetError(ex, "getCodAutenticacion");
            }

            return codAutenticacion;
        }


        public string strParcialActivo
        {
            get{
                EvaluacionActiva pa = new EvaluacionActiva();
                return pa.getDtaEvaluacionActiva();
            }
        }

    }
}