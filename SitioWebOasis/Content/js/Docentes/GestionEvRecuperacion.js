﻿//  GESTION DE INFORMACION DE EVALUACION RECUPERACION
$(document).ready(function () {
    $.jgrid.defaults.width = '100%';
    $.jgrid.defaults.responsive = true;
    $.jgrid.defaults.styleUI = 'Bootstrap';

    var dtaEvRecuperacion = $('#dtaJsonEvRecuperacion').val();
    var grdEvRecuperacion = $("#grdEvRecuperacion");
    var lstEvaluacionRecuperacion = new Array();

    var lastsel;
    var selIRow = 1;
    var rowIds;

    var blnCambiosEvRecuperacion = false;
    var banControlImpresion = false;

    //  Objeto con informacion de dtaEvRecuperacion
    cargarDatosEvRecuperacion($('#dtaJsonEvRecuperacion').val());


    function cargarDatosEvRecuperacion(dtaEvRecuperacion) {
        var dtaEvaluacionRecuperacion = eval(dtaEvRecuperacion);
        if (dtaEvaluacionRecuperacion.length > 0) {
            var nef = dtaEvaluacionRecuperacion.length;
            for (var x = 0; x < nef; x++) {
                var objEvaluacionRecuperacion = new EvaluacionRecuperacion();
                objEvaluacionRecuperacion.setDtaEvaluacionRecuperacion(dtaEvaluacionRecuperacion[x]);
                lstEvaluacionRecuperacion.push(objEvaluacionRecuperacion);
            }
        }
    }


    //  Gestion de notas 
    grdEvRecuperacion.jqGrid({
        url: 'data.json',
        editurl: 'clientArray',
        datatype: "jsonstring",
        colModel: [ { name: 'No', index: 'No', label: 'No', align: 'center', width: '30', sortable: false },
                    { name: "sintCodMatricula", key: true, hidden: true },
                    { name: 'strCodigo', label: "Codigo", align: "center", width: "60", sortable: true },
                    { name: 'NombreCompleto', label: "Nombre estudiante", width: '300', align: 'left', sortable: true },
                    { name: 'bytNumMat', label: 'Matrícula', width: '80', align: 'center', sortable: true },
                    { name: 'bytAcumulado', label: 'Total evaluación formativa', width: '150', align: 'center', sortable: false },
                    
                    { name: 'bytNota', label: 'Nota evaluación recuperación', width: '170', align: 'center', sortable: true, editable: true, edittype: 'text', editoptions: { size: 1, maxlength: 2, dataInit: soloNumero }, editrules: { custom: true, custom_func: validarNota }, sortable: false, formatter: { integer: { thousandsSeparator: " ", defaultValue: '0' } } },

                    { name: 'Total', label: 'Total', width: '120', align: 'center', sortable: true },
                    { name: 'erAcumulado', label: 'Estado', width: '120', align: 'center', sortable: true },
                    { name: 'strObservaciones', label: 'Observación', width: '170', align: 'center', sortable: false }],

        loadonce: true,
        datastr: $("#dtaJsonEvRecuperacion").val(),
        autowidth: true,
        height: "auto",
        shrinkToFit: true,
        ignoreCase: true,
        rowNum: 100,
        onSelectRow: editarRegistroEvRecuperacion,

        loadComplete: function (data) {
            //  Resaltar contenido en columnas en la pagina
            updContenidoColumnasGrid();
        }
    });


    var clickedCell;
    var banUpdRow = false;
    $('#grdEvRecuperacion td').on('click', function (e) {
        clickedCell = this;
    });


    function editarRegistroEvRecuperacion(id, status, e)
    {
        if (id !== lastsel) {
            //  Cierro edicion de la ultima fila gestionada
            if (lastsel != undefined) {
                $('#grdEvRecuperacion').jqGrid('restoreRow', lastsel);
            }

            //
            //  Recalculo Acumulado - si el docente edito la nota recalcula el acumulado total, 
            //  cumplimiento y si esta en el ultimo parcial la equivalencia
            //  
            $("#grdEvRecuperacion").jqGrid("editRow", id, {
                keys: true,
                focusField: 4,
                oneditfunc: function () {
                    $('input, textarea', clickedCell).select();
                },
                aftersavefunc: function (id) {
                    //  Registro la informacion gestionada en el JSON
                    guardarDtaEvRecuperacion(id);

                    //  Actualizo contenido de la fila
                    updDtaEvRecuperacion(id);

                    //  Obtengo el identificador de la siguiente registro de notas a gestionar
                    var idNextRow = getIdNextRow(id);
                    $('#grdEvRecuperacion').jqGrid('setSelection', idNextRow, true);
                    $("#" + idNextRow + "_bytNota").select();
                }
            });

            lastsel = id;
        } else {
            //  Si el registro del estudiante es de EXONERADO - REPROBADO
            $('#grdEvRecuperacion').jqGrid('setSelection', id, false);
        }
    }



    $("#grdEvRecuperacion").jqGrid('setGroupHeaders', {
        useColSpanStyle: true,
        groupHeaders: [{ startColumnName: 'Total', numberOfColumns: 2, titleText: '<b>TOTALIZADO</b>' }]
    });


    function guardarDtaEvRecuperacion(id) {
        numReg = lstEvaluacionRecuperacion.length;
        for (var x = 0; x < numReg; x++) {
            if (lstEvaluacionRecuperacion[x].sintCodMatricula == id) {
                var dtaNota = $("#grdEvRecuperacion").jqGrid("getCell", id, "bytNota");
                lstEvaluacionRecuperacion[x]["bytNota"] = dtaNota;
                lstEvaluacionRecuperacion[x].banEstado = 1;

                //  Registro un cambio
                blnCambiosEvRecuperacion = true;

                return true;
            }
        }

        return false;
    }


    function getIdNextRow(idActualRow) {
        var num = rowIds.length;
        var idNextRow = idActualRow;

        for (var x = 0; x < num; x++) {
            if (rowIds[x] == idActualRow && x < num) {
                idNextRow = rowIds[x + 1];
                break;
            }
        }

        return idNextRow;
    }


    function soloNumero(element) {
        $(element).keypress(function (e) {
            if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
                return false;
            }
        });
    }


    function updContenidoColumnasGrid() {
        rowIds = $('#grdEvRecuperacion').jqGrid('getDataIDs');

        for (i = 0; i <= rowIds.length - 1 ; i++) {
            //  En funcion al parcial activo resalto el color de la columna 
            $("#grdEvRecuperacion").jqGrid('setCell',
                                    rowIds[i],
                                    'bytNota',
                                    "",
                                    { 'background-color': '#fcf8e3' });

            $("#grdEvRecuperacion").jqGrid('setRowData', rowIds[i], { bytNumMat: lstEvaluacionRecuperacion[i].getNumMatricula() });
            $("#grdEvRecuperacion").jqGrid('setRowData', rowIds[i], { erAcumulado: lstEvaluacionRecuperacion[i].getEstadoEvaluacionRecuperacion() });
        }
    }


    function updDtaEvRecuperacion(id) {
        numReg = lstEvaluacionRecuperacion.length;
        for (var x = 0; x < numReg; x++) {
            if (lstEvaluacionRecuperacion[x].sintCodMatricula == id) {
                $("#grdEvRecuperacion").jqGrid('setRowData', id, { Total: lstEvaluacionRecuperacion[x].getTotalEvRecuperacion() });
                $("#grdEvRecuperacion").jqGrid('setRowData', id, { erAcumulado: lstEvaluacionRecuperacion[x].getEstadoEvaluacionRecuperacion() });

                break;
            }
        }

        $("#grdEvRecuperacion").jqGrid( 'setCell',
                                        id,
                                        'bytNota',
                                        "",
                                        {   'background-color': '#dff0d8',
                                            'background-image': 'none',
                                            'text-align': 'center',
                                            'font-size': 'medium',
                                            'font-weight': 'bold'
                                        });



        $("#grdEvRecuperacion").jqGrid('setCell',
                                       id,
                                       'Total',
                                       "",
                                       {
                                           'background-color': '#dff0d8',
                                           'background-image': 'none',
                                           'text-align': 'center',
                                           'font-size': 'medium',
                                           'font-weight': 'bold'
                                       });

        return true;
    }


    function validarNota(value, colname) {
        if (value < 0 || value > 20)
            return [false, "Nota fuera de rango (0, 20)"];
        else
            return [true, ""];
    }


    $('#btnGuardarEvRecuperacion').on('click', function () {
        //  Muestro mensaje de proceso
        showLoadingProcess('');

        if (blnCambiosEvRecuperacion == true) {
            $.ajax({
                type: "POST",
                url: "/Docentes/registrarEvaluacionRecuperacion/" + $("#nivel").val() + "/" + $("#codAsignatura").val() + "/" + $("#paralelo").val() + "/" + $('#dtaParcialActivo').val(),
                data: JSON.stringify(lstEvaluacionRecuperacion),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                error: function (xhr, ajaxOptions, thrownError) {
                    alert(thrownError);
                }
            }).success(function (data) {
                if (data.dtaEvRecuperacionUpd != "false") {
                    lstEvaluacionRecuperacion = new Array();

                    //  Muestro mensaje de gestion de informacion
                    $('#msmGrdEvRecuperacion').removeAttr("hidden");

                    //  Actualizo el grid con la informacion gestionada a nivel BD
                    cargarDatosEvRecuperacion(data.dtaEvRecuperacionUpd);

                    //  Actualizo el grid de notas
                    updContenidoColumnasGrid();

                    //  Regreso a su valor original la variable de control de cambios en el grid de notas
                    blnCambiosEvRecuperacion = false;

                    //  Mostrar mensaje de estado de la transaccion
                    getMensajeTransaccion(true, data.MessageGestion);

                    //  limpio el ultimo registro de notas gestionado
                    lastsel = 0;

                    //  recargo el grid de notas
                    $('#grdEvRecuperacion').trigger('reloadGrid');

                } else {
                    //  Mostrar mensaje de estado de la transaccion
                    getMensajeTransaccion(false, data.MessageGestion);
                }

                //  Cierro la ventana GIF Proceso
                HoldOn.close();
            })
        } else {
            //  Cierro la ventana GIF Proceso
            HoldOn.close();
        }
    })


    //  Control de impresion de actas
    $('#pER_pdf, #pER_xls, #pER_blc').on('click', function () {
        $("#opImpEvRecuperacion").val($(this).attr("id"));

        if (banControlImpresion == false) {
            if (blnCambiosEvRecuperacion == true) {
                $('#btnGuardarEvRecuperacion').trigger("click");
            }

            controlImpresion();
        } else {
            imprimirActaEvRecuperacion();
        }
        
    })



    function getMensajeTransaccion(banEstado, mensaje) {
        //  Muestro mensaje de gestion de informacion
        $('#msmGrdEvRecuperacion').removeAttr("hidden");

        if (banEstado == true) {
            $('#msmGrdEvRecuperacion').attr("class", "alert alert-success fade in");
            $('#msmGrdEvRecuperacion').html("<button class='close' data-dismiss='alert'>×</button> <i class='fa fa-check' aria-hidden='true'></i> <strong>" + mensaje + "</strong>");
        } else if (banEstado == false) {
            $('#msmGrdEvRecuperacion').attr("class", "alert alert-danger fade in");
            $('#msmGrdEvRecuperacion').html("<button class='close' data-dismiss='alert'>×</button> <i class='fa fa-exclamation-circle' aria-hidden='true'></i> <strong>" + mensaje + "</strong>");
        }
    }



    function controlImpresion() {
        //  Control de impresion
        $.confirm({
            icon: 'glyphicon glyphicon-alert',
            columnClass: 'col-md-6 col-md-offset-3',
            title: 'Control de impresión',
            content: '<form action="#" class="formName">' +
                    '   <div class="alert alert-warning">' +
                    '       Compañero docente al ejecutar está acción usted' +
                    '       <strong> DA POR FINALIZADA LA GESTIÓN DE NOTAS DE EVALUACIÓN DE RECUPERACIÓN DE LA ASIGNATURA ' + $('#ddlLstPeriodosEstudiante :selected').text() + ' </strong>' +
                    '       y ninguna nota va a poder ser gestionada desde el modulo web del sistema académico.' +
                    '   </div>' +

                    '   <div class="alert alert-info">' +
                    '       Para su seguridad se enviara un código de impresión a su cuenta de correo institucional <strong>' + $('#dtaCtaUsuario').val() + '</strong> para continuar con la ejecución de esta tarea' +
                    '   </div>' +
                    '</form>',

            escapeKey: 'cancelar',
            buttons: {
                formSubmit: {
                    text: 'Enviar código de impresión',
                    btnClass: 'btn-blue',
                    action: function () {
                        showLoadingProcess('Enviando código de impresión ...');

                        $.ajax({
                            type: "POST",
                            url: "/Docentes/EnviarCorreoValidacionImpresion",
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            error: function (xhr, ajaxOptions, thrownError) {
                                console.log(xhr.responseText);

                                //  Mostrar mensaje de estado de la transaccion
                                getMensajeTransaccion(false, "Error, favor vuelva a intentarlo, si el problema persiste consulte en la secretaria de su carrara");

                                //  Cierro la ventana GIF Proceso
                                HoldOn.close();
                            }
                        }).complete(function (data) {
                            if (data.responseJSON.banEnviocodAutenticacion == true) {
                                //  Cierro la ventana GIF Proceso
                                HoldOn.close();

                                //  Muestro la ventana de ingreso de codigo de impresion
                                frmValidacionCodigoImpresion()
                            } else {
                                //  Si existe error, muestro el mensaje
                                $('#messageError').removeAttr("hidden");
                                $('#messageError').html("<a href='' class='close'>×</a><strong>" + data.responseJSON.errorMessage + "</strong>, Favor vuelva a intentarlo");
                            }
                        })

                    }
                },
                cancelar: function () {
                    //close
                },
            },

        });
    }



    function frmValidacionCodigoImpresion() {
        $.confirm({
            icon: 'glyphicon glyphicon-alert',
            columnClass: 'col-md-6 col-md-offset-3',
            title: 'Control de impresión',
            content: '<form action="#" class="formName">' +
                    '   <div class="form-group">' +
                    '       <input id="dtaNumConfirmacion" maxlength="4" type="text" placeholder="código de impresión" class="name form-control" required />' +
                    '   </div>' +
                    '</form>',

            escapeKey: 'cancelar',
            buttons: {
                formSubmit: {
                    text: 'Validar e imprimir',
                    btnClass: 'btn-blue',
                    action: function () {
                        var opImpresion = $("#opImpEvRecuperacion").val();
                        var numConfirmacion = this.$content.find('#dtaNumConfirmacion').val();
                        showLoadingProcess('Verificando código de impresión ...');

                        $.ajax({
                            type: "POST",
                            url: "/Docentes/ValidarCodigoImpresion",
                            data: '{strCodImpresion: "' + numConfirmacion + '", idActa: "' + opImpresion + '", idAsignatura: "' + $('#ddlLstPeriodosEstudiante').val() + '"}',
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",

                            error: function (xhr, ajaxOptions, thrownError) {
                                console.log(xhr.responseText);

                                //  Mostrar mensaje de estado de la transaccion
                                getMensajeTransaccion(false, "Error, favor vuelva a intentarlo, si el problema persiste consulte en la secretaria de su carrara");

                                //  Cierro la ventana GIF Proceso
                                HoldOn.close();
                            }

                        }).complete(function (data) {
                            if (data.responseJSON.fileName != "none" && data.responseJSON.fileName != "" && data.responseJSON.fileName != undefined) {
                                //  Cambio el color del boton
                                $('#btnER, #btnERF').attr("class", 'btn btn-warning btn-md');

                                //  Oculto el mensaje de error
                                $('#messageError').attr("hidden");

                                //  Cierro el formulario de ingreso de codigo de impresion
                                $.unblockUI();

                                //  Cierro la ventana GIF Proceso
                                HoldOn.close();

                                //  Cambio el grid de gestion de nota de evaluacion acumulativa a modo solo lectura
                                grdEvRecuperacionSoloLectura();

                                //  Actualizo la bandera de impresion
                                banControlImpresion = true;

                                //  Elimino el boton de guardar
                                $('#btnGuardarEvRecuperacion').remove();

                                //  Descarga de archivo
                                $.redirect("/Docentes/DownloadFile", { file: data.responseJSON.fileName }, "POST")
                            } else {
                                //  Cierro la ventana GIF Proceso
                                HoldOn.close();

                                $('#dtaNumConfirmacion').val("");

                                //  Si existe error, muestro el mensaje
                                alert(data.responseJSON.errorMessage);
                            }
                        })

                    }
                },
                cancelar: function () {
                    //close
                },
            },
        });
    }



    //$.find('#dtaNumConfirmacion').keypress(function (event) {
    //    var $this = $(this);
    //    if (((event.which < 48 || event.which > 57) && (event.which != 0 && event.which != 8))) {
    //        event.preventDefault();
    //    }
    //})


    $('#btnValidarImprimir').click(function () {
        if ($('#dtaNumConfirmacion').val() == "987") {
            $.unblockUI();

            //  Guardo los cambios registrados
            $('#btnGuardarEvRecuperacion').trigger("click");

            showLoadingProcess();

            var opImpresion = $("#opImpEvRecuperacion").val();
            
            $.ajax({
                type: "POST",
                url: "/Docentes/impresionActas",
                data: '{idActa: "' + opImpresion + '", idAsignatura: "' + $('#ddlLstPeriodosEstudiante').val() + '"}',
                contentType: "application/json; charset=utf-8",
                dataType: "json"
            }).complete(function (data) {
                //  Cambio el color del boton
                $('#btnER, #btnERF').attr("class", 'btn btn-warning btn-md');

                //  Oculto el mensaje de error
                $('#messageError').attr("hidden");

                //  Cierro la ventana GIF Proceso
                HoldOn.close();

                $("opImpEvRecuperacion").attr("value", "");

                if (data.responseJSON.fileName != "none" && data.responseJSON.fileName != "") {
                    //  Cambio el Grid a modo "soloLectura"
                    grdEvRecuperacionSoloLectura();

                    //  Elimino el boton de guardado de Ev recuperacion
                    $('#btnGuardarEvRecuperacion').remove();

                    $.redirect( "/Docentes/DownloadFile",
                                { file: data.responseJSON.fileName },
                                "POST")
                } else {
                    //  Si existe error, muestro el mensaje
                    $('#messageError').removeAttr("hidden");
                    $('#messageError').html("<a href='' class='close'>×</a><strong>FALLO !!!</strong> Favor vuelva a intentarlo");
                }
            })
        } else {
            alert('NUMERO DE CONFIRMACION NO VALIDO, favor vuelva a ingresarlo');
        }
    })


    function showLoadingProcess(mensaje) {
        var msg = (mensaje.length == 0) ? 'GUARDANDO INFORMACION ...'
                                        : mensaje;

        HoldOn.open({
            theme: 'sk-dot',
            message: "<h4>" + mensaje + "</h4>"
        });
    }

    function grdEvRecuperacionSoloLectura(){
        $('#grdEvRecuperacion').setColProp('bytNota', { editable: 'False' });
    }


    function imprimirActaEvRecuperacion() {
        showLoadingProcess();

        $.ajax({
            type: "POST",
            url: "/Docentes/impresionActas",
            data: '{idActa: "' + $("#opImpEvRecuperacion").val() + '", idAsignatura: "' + $('#ddlLstPeriodosEstudiante').val() + '"}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            error: function (xhr, ajaxOptions, thrownError) {
                console.log(xhr.responseText);

                //  Mostrar mensaje de estado de la transaccion
                getMensajeTransaccion(false, "Error, favor vuelva a intentarlo, si el problema persiste consulte en la secretaria de su carrara");

                //  Cierro la ventana GIF Proceso
                HoldOn.close();
            }
        }).complete(function (data) {
            //  Oculto el mensaje de error
            $('#messageError').attr("hidden");

            //  Cierro la ventana GIF Proceso
            HoldOn.close();

            if (data.responseJSON.fileName != "none" && data.responseJSON.fileName != "") {
                $.redirect("/Docentes/DownloadFile",
                            { file: data.responseJSON.fileName },
                                "POST")
            } else {
                //  Si existe error, muestro el mensaje
                $('#messageError').removeAttr("hidden");
                $('#messageError').html("<a href='' class='close'>×</a><strong>Problemas al generar archivo</strong>, favor vuelva a intentarlo");
            }
        })
    }


})