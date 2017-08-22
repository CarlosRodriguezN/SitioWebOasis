﻿using GestorErrores;
using SitioWebOasis.CommonClasses;
using SitioWebOasis.CommonClasses.GestionUsuarios;
using SitioWebOasis.Library;
using System;
using System.Collections.Generic;
using System.Web.Helpers;
using System.Web.Mvc;

namespace SitioWebOasis.Models
{
    public class DatosPersonalesEstudiantes
    {
        public string per_numCedula { get; set; }
        public string per_id = string.Empty;
        public bool existePersona = false;

        public Catalogos catalogo;
        public Persona dtaEstudiante;
        public DocumentoPersonal dtaDocPersonal;
        public Direccion dtaDireccionEstudiante;
        public Nacionalidad dtaNacionalidadEstudiante;

        public DatosPersonalesEstudiantes()
        {
            this.per_numCedula = UsuarioActual.Cedula.ToString().Replace("-", "");
            if ( !string.IsNullOrEmpty(this.per_numCedula.ToString() ))
            {
                this.catalogo = new Catalogos();

                //  Consumo del servicio web ObtenerPorDocumento (cedula)
                string jsonDtaPersona = ClienteServicio.ConsumirServicio( CENTRALIZADA.WS_URL.WS_PERSONAS + "ServiciosPersona.svc" + "/ObtenerPorDocumento/" + this.per_numCedula.ToString());
                var dtaPersona = Json.Decode(jsonDtaPersona);

                if( !string.IsNullOrEmpty( Convert.ToString( dtaPersona.per_id ) ) && Convert.ToString(dtaPersona.per_id) != "0"){
                    this.existePersona = true;
                    dtaEstudiante = new Persona(jsonDtaPersona);
                    dtaDocPersonal = new DocumentoPersonal(dtaEstudiante.per_id.ToString());
                    dtaDireccionEstudiante = new Direccion(dtaEstudiante.per_id.ToString());
                    dtaNacionalidadEstudiante = new Nacionalidad(dtaEstudiante.per_id.ToString());
                }
            }
        }

        public Usuario UsuarioActual
        {
            get { return (Usuario)System.Web.HttpContext.Current.Session["UsuarioActual"]; }
        }

        public bool updDatosEstudiantes(System.Web.HttpRequestBase dtaFrmEstudiante)
        {
            bool rst = true;

            try
            {
                if( this._updDatosPersonalesEstudiante(dtaFrmEstudiante) && this._updDireccionEstudiante(dtaFrmEstudiante)){
                    this._registrarDatosPersonalesEstudiante();
                    this._registrarDireccionEstudiante();
                }
            }catch (Exception ex){
                Errores err = new Errores();
                err.SetError(ex, "updDatosEstudiantes");
                rst = false;
            }

            return rst;
        }


        private bool _updDatosPersonalesEstudiante( System.Web.HttpRequestBase dtaFrmEstudiante )
        {
            bool rst = true;

            try
            {
                //  Informacion de persona
                this.dtaEstudiante.eci_id = Convert.ToInt32(dtaFrmEstudiante["ddlEstadoCivil"].ToString().Trim());
                this.dtaEstudiante.etn_id = Convert.ToInt32(dtaFrmEstudiante["ddlEtnia"].ToString().Trim());
                this.dtaEstudiante.tsa_id = Convert.ToInt32(dtaFrmEstudiante["ddlTipoSangre"].ToString().Trim());
                this.dtaEstudiante.gen_id = Convert.ToInt32(dtaFrmEstudiante["ddlGenero"].ToString().Trim());
                this.dtaEstudiante.per_telefonoCelular = dtaFrmEstudiante["txtTelefonoCelular"].ToString().Trim();
                this.dtaEstudiante.per_telefonoCasa = dtaFrmEstudiante["txtTelefonoFijo"].ToString().Trim();
                this.dtaEstudiante.per_emailAlternativo = dtaFrmEstudiante["txtCorreoAlternativo"].ToString().Trim();
                
                string DPA_FN = dtaFrmEstudiante["ddl_FNPais"].ToString().Trim() + "|" +
                                dtaFrmEstudiante["ddl_FNProvincias"].ToString().Trim() + "|" +
                                dtaFrmEstudiante["ddl_FNCiudades"].ToString().Trim() + "|" +
                                dtaFrmEstudiante["ddl_FNParroquias"].ToString().Trim();

            }catch (Exception ex){
                Errores err = new Errores();
                err.SetError(ex, "_updDatosPersonalesEstudiante");
                rst = false;
            }

            return rst;
        }


        private bool _updDireccionEstudiante(System.Web.HttpRequestBase dtaFrmEstudiante)
        {
            bool rst = true;

            try
            {
                string DPA_DE = dtaFrmEstudiante["ddl_DUPais"].ToString().Trim() + "|" +
                                dtaFrmEstudiante["ddl_DUProvincias"].ToString().Trim() + "|" +
                                dtaFrmEstudiante["ddl_DUCiudades"].ToString().Trim() + "|" +
                                dtaFrmEstudiante["ddl_DUParroquias"].ToString().Trim();

                this.dtaDireccionEstudiante.dir_callePrincipal = dtaFrmEstudiante["txtDirCallePrincipal"].ToString().Trim();
                this.dtaDireccionEstudiante.dir_numero = dtaFrmEstudiante["txtDirNumeroCasa"].ToString().Trim();
                this.dtaDireccionEstudiante.dir_calleTransversal = dtaFrmEstudiante["txtDirCalleSecundaria"].ToString().Trim();
                this.dtaDireccionEstudiante.dir_referencia = dtaFrmEstudiante["txtDirReferencia"].ToString().Trim();
                this.dtaDireccionEstudiante.dir_referencia = dtaFrmEstudiante["txtDirReferencia"].ToString().Trim();
            }catch( Exception ex){
                Errores err = new Errores();
                err.SetError(ex, "_updDireccionEstudiante");
                rst = false;
            }

            return rst;
        }


        private bool _registrarDatosPersonalesEstudiante()
        {
            bool rst = true;

            try
            {
                string urlWS = CENTRALIZADA.WS_URL.WS_PERSONAS + "ServiciosPersona.svc" + "/Modificar";
                object estado = ClienteServicio.ConsumirServicioPost(urlWS, dtaEstudiante);
            }
            catch( Exception ex)
            {
                rst = false;
                Errores err = new Errores();
                err.SetError(ex, "registrarDatosEstudiante");
            }

            return rst;
        }
        

        private bool _registrarDireccionEstudiante()
        {
            bool rst = true;

            try
            {
                string urlWS = CENTRALIZADA.WS_URL.WS_PERSONAS + "ServiciosDireccion.svc" + "/Modificar";
                object estado = ClienteServicio.ConsumirServicioPost(urlWS, dtaDireccionEstudiante);
            }
            catch (Exception ex)
            {
                rst = false;
                Errores err = new Errores();
                err.SetError(ex, "_registrarDireccionEstudiante");
            }

            return rst;
        }

    }
}